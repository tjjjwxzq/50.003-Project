using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Networking;

public class LobbyManager : NetworkLobbyManager {

    // for tracking if lobby players are ready on the client, and saving local player state
    private List<LobbyPlayer> lobbyPlayers = new List<LobbyPlayer>(); 
    // for tracking if lobby players are ready in ability selection scene, and saving selected abilities
    private List<Player> players = new List<Player>(); 
    private MyNetworkDiscovery networkDiscovery; // for local discovery
    private StartController startController; // for toggling UI
    private bool readyUIActive = false;

    // To position lobby players
    private RectTransform lobbyPlayerRectTransform;
    private float lobbyPlayerStartPos = 140f; // starting position of lobby players
    private float lobbyPlayerOffset; // for positioning lobby players
    private int lobbyPlayerPosition; // position id to be stored by each player

    private GameObject countdownUI;
    private float readyCountdown = 3; // counts down after all players have chosen their abilities and before the game starts
    private bool readyToPlay = false; // whether all players have selected their abilities and are ready to play
    private bool changingScene = false;// whether in the midst of changing to main scene
    private bool isHost = false; // whether or not the current process is hosting the game

    // For saving local player state from pre-game scenes
    public Color PlayerColor;
    public Abilities PlayerAbilities;

    // Use this for initialization
    void Start () {

        // Get NetworkDiscovery to initialize broadcast in OnStartHost hook
        networkDiscovery = GetComponent<MyNetworkDiscovery>();

        // Get StartController for handling UI changes
        startController = GameObject.Find("StartController").GetComponent<StartController>();

        // Get LobbyPlayer RectTransform for positioning lobby players when they spawn
        lobbyPlayerRectTransform = lobbyPlayerPrefab.GetComponent<RectTransform>();
        lobbyPlayerOffset = lobbyPlayerRectTransform.rect.height * lobbyPlayerRectTransform.localScale.y * 1.2f;
        Debug.Log("lobby player offset is " + lobbyPlayerOffset);
        /*for(int i = 0; i < MaxPlayers; i++)
        {
            GameObject start = Instantiate(startPositionPrefab, new Vector2(0, height - offset * i), Quaternion.identity) as GameObject;
            start.transform.SetParent(canvasObj.transform, false);

        }*/


        // Set round robin player spawn method
        playerSpawnMethod = PlayerSpawnMethod.RoundRobin;
    }
	
	// Update is called once per frame
	void Update () {

        Debug.Log("Number of connected players are " + numPlayers);
        Debug.Log("Number of lobby players are " + lobbyPlayers.Count);
        Debug.Log("Number of game players are " + players.Count);
        foreach (LobbyPlayer player in lobbyPlayers)
            Debug.Log("Lobby player is " + player);
        UpdateLobbyPlayers();

        // Make sure lobbyPlayer position is reset accordingly
        if (lobbyPlayerPosition > lobbyPlayers.Count)
        {
            lobbyPlayerPosition = lobbyPlayers.Count;
            Debug.Log("Resetting player position");

        }

        // To check and update UI when all lobby players are ready
        if (!readyUIActive && lobbyPlayers.Count >= minPlayers && checkAllReady())
        {
            if (isHost)
                startController.ToggleHostReadyUI(true);
            else
                startController.ToggleClientReadyUI(true);
            readyUIActive = true;
        }

        // To check and update UI when lobby players are no longer ready
        if(readyUIActive && !checkAllReady())
        {
            if (isHost)
                startController.ToggleHostReadyUI(false);
            else
                startController.ToggleClientReadyUI(false);
            readyUIActive = false;
        }

        // Check if players have selected abilities and are ready to play
        // Have to put changing scene flag or the scene change code 
        // will execute again in the midst of changing scene
        // which will crash the game
        if(!readyToPlay && !changingScene && players.Count > 0)
        {
            Debug.Log("Checking if abilities are ready");
            checkAbilitiesReady();
            Debug.Log("Ready to play? " + readyToPlay);
        }

        if(readyToPlay && readyCountdown > 0)
        {
            Debug.Log("Ready to countdown");
            // Extra 1 second for setup time of UI
            if(readyCountdown == 3)
            {
                // Start animation
                GameObject.Find("AbilitySelectionController").GetComponent<AbilitySelectionController>().StartCountdown();
                countdownUI.transform.Find("CountdownText").GetComponent<Animator>().Play("ReadyCountdown",0, 0f);
            }

            readyCountdown -= Time.deltaTime;
            //Debug.Log("REady countdown is " + readyCountdown);

            if (readyCountdown < 0)
            {
                ServerChangeScene("Main");
                readyToPlay = false;
                changingScene = true;
            }
        }
	}

    // Start network discovery broadcasting
    public override void OnStartHost()
    {
        networkDiscovery.Initialize();
        networkDiscovery.StartAsServer();
        isHost = true;
    }

    // Disable start UI when client enters
    public override void OnLobbyClientEnter()
    {
        Debug.Log("Entering lobby");
        Debug.Log("Enter Num lobby players are " + lobbyPlayers.Count);
        // Disable start UI
        startController.ToggleStartUI(false);
        startController.ToggleWaitingUI(false);

    }

    // Check that lobbyplayers list is updated
    public override void OnLobbyClientExit()
    {

        Debug.Log("Exit Num lobby players are " + lobbyPlayers.Count);

    }
    // Customize player spawning
    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
    {
        if(SceneManager.GetActiveScene().name.Equals("Lobby", System.StringComparison.Ordinal)){
            GameObject player = Instantiate(lobbyPlayerPrefab.gameObject);
            // Assign player position
            Debug.Log("Player position is " + lobbyPlayerPosition);
            player.GetComponent<LobbyPlayer>().PlayerPosition = lobbyPlayerPosition++;
            NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);
        }
        else if(SceneManager.GetActiveScene().name.Equals("SelectAbilities", System.StringComparison.Ordinal))
        {
            Debug.Log("INstantiating player in abilities scene");
            GameObject player = Instantiate(gamePlayerPrefab) as GameObject;
            NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);
        }
    }

    public void OnLobbyClientDisconnect()
    {
        Debug.Log("Client disconnected");
    }

    public override void OnStopServer()
    {
        Debug.Log("Server stopped");
    }

    // Players are ready, show start button on host
    public override void OnLobbyServerPlayersReady()
    {
        // Show start button but not waiting text
        startController.ToggleHostReadyUI(true);
    }

    public override void OnLobbyClientSceneChanged(NetworkConnection conn)
    {
        Debug.Log("Client scene changed");
        // Get countdown UI in ability selection scene
        if(SceneManager.GetActiveScene().name == "SelectAbilities")
        {
            countdownUI = GameObject.Find("AbilitySelectionController").GetComponent<AbilitySelectionController>().countdownUI;
        }
    }

    public override void OnClientSceneChanged(NetworkConnection conn)
    {
        base.OnClientSceneChanged(conn);
        Debug.Log("CLIENT SCENE CHANGED");
    }

    public override void OnServerSceneChanged(string sceneName)
    {
        Debug.Log("Server scene changed");
        Debug.Log("Active scene is " + SceneManager.GetActiveScene().name);
    }

    // According to the docs this should be able to be used to pass player preferences selected before the
    // play scene to the players in the play scene, but somehow it's not being called,
    // maybe because I've overriden some default methods.
    // But I've decided to store the local player state in the LobbyManager, since it persists
    // throughout the scenes, so the local players on each client can update the server and 
    // every player object on each client is updated from there
    public override bool OnLobbyServerSceneLoadedForPlayer(GameObject lobbyPlayer, GameObject gamePlayer)
    {
        Debug.Log("Scnee loaded for player");
        gamePlayer.GetComponent<Player>().Color = lobbyPlayer.GetComponent<LobbyPlayer>().PlayerColor;
        return true;
    }

    // Check players ready in lobby
    private bool checkAllReady()
    {
        return lobbyPlayers.All(p => p.PlayerReady);
    }

    /// <summary>
    /// Add lobby player to list when lobby player network behavior client starts
    /// </summary>
    /// <param name="player"></param>
    public void AddLobbyPlayer(GameObject player)
    {
        lobbyPlayers.Add(player.GetComponent<LobbyPlayer>());
    }

    /// <summary>
    /// Add spawned player to list when the player network behavior client starts
    /// </summary>
    /// <param name="player"></param>
    public void AddPlayer(GameObject player)
    {
        players.Add(player.GetComponent<Player>());
    }

    /// <summary>
    /// Updates list of lobby players,
    /// removing objects which have become null (player has quit)
    /// </summary>
    private void UpdateLobbyPlayers()
    {
        if (lobbyPlayers.Count != numPlayers)
            lobbyPlayers = lobbyPlayers.Where(p => p != null).ToList();
    }

    /// <summary>
    /// Set the local player color
    /// Called by LobbyPlayer when the player
    /// clicks the ready button in the lobby
    /// </summary>
    /// <param name="color"></param>
    public void setLocalPlayerColor(Color color)
    {
        PlayerColor = color;
    }

    /// <summary>
    /// Get the list of currently used colors
    /// To be called by lobby player when setting color
    /// </summary>
    /// <returns></returns>
    public List<Color> getUsedColors()
    {
        return lobbyPlayers.Select(p => p.PlayerColor).ToList<Color>();
    }

    // Start Game button callback (only on host) 
    public void OnStartGame()
    {
        ServerChangeScene("SelectAbilities");
    }

    // Check whether players have selected their abilities and are ready
    private void checkAbilitiesReady()
    {
        readyToPlay = players.All(p => p.readyToPlay);
    }

    /// <summary>
    /// Flag for whether a player is a host or just a normal client
    /// </summary>
    public bool IsHost
    {
        get { return isHost; }
    }




}