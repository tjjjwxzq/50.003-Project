using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Networking;

public class LobbyManager : NetworkLobbyManager {

    // For Lobby Scene
    private bool serverStarted = false;
    private bool colorControllerSpawned = false;
    // for tracking if lobby players are ready on the client, and saving local player state
    private List<LobbyPlayer> lobbyPlayers = new List<LobbyPlayer>(); 
    // for tracking if lobby players are ready in ability selection scene, and saving selected abilities
    private List<Player> players = new List<Player>(); 
    private MyNetworkDiscovery networkDiscovery; // for local discovery
    private GameObject canvasObj;
    private GameObject startUI;
    private GameObject readyUI;
    private GameObject readyWaitingText;
    private GameObject readyStartButton;
    private bool readyUIActive = false;
    public RectTransform playerTransform; // to be accessed by StartController to set spawn positions

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
        // Set round robin player spawn method
        playerSpawnMethod = PlayerSpawnMethod.RoundRobin;

        networkDiscovery = GetComponent<MyNetworkDiscovery>();
        canvasObj = GameObject.Find("Canvas");
        startUI = GameObject.Find("StartUI");
        readyUI = GameObject.Find("ReadyUI");
        readyWaitingText = readyUI.transform.Find("WaitingText").gameObject;
        readyStartButton = readyUI.transform.Find("StartButton").gameObject;

        readyUI.SetActive(false);
        playerTransform = lobbyPlayerPrefab.GetComponent<RectTransform>();

    }
	
	// Update is called once per frame
	void Update () {

        Debug.Log("Number of connected players are " + numPlayers);
        Debug.Log("Number of lobby players are " + lobbyPlayers.Count);
        Debug.Log("NUmber of game players are " + players.Count);
        foreach (LobbyPlayer player in lobbyPlayers)
            Debug.Log("Lobby player is " + player);
        UpdateLobbyPlayers();

        if (serverStarted && !colorControllerSpawned)
        {
            Debug.Log("Spawning color ocntroller");
            // Spawn color controller
            colorControllerSpawned = true;
            NetworkServer.SpawnObjects(); // Maybe shift this to OnLobbyClientEnter? Or would that duplicate the object
            // I am not sure why the plain vanilla spawn with a registered
            // color controller prefab as an argument doesn't seem to work
            // Now a ColorController object is placed in the scene but is is disabled by default
            // SpawnObjects() enables and spawns it
        }
        // To check and update UI when all lobby players are ready
        if (!readyUIActive && lobbyPlayers.Count >= minPlayers && checkAllReady())
        {
            if (isHost)
                ToggleHostReadyUI(true);
            else
                ToggleClientReadyUI(true);
            readyUIActive = true;
        }

        // To check and update UI when lobby players are no longer ready
        if(readyUIActive && !checkAllReady())
        {
            if (isHost)
                ToggleHostReadyUI(false);
            else
                ToggleClientReadyUI(false);
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
        serverStarted = true;

    }

    // Disable start UI when client enters
    public override void OnLobbyClientEnter()
    {
        Debug.Log("Entering lobby");
        Debug.Log("Enter Num lobby players are " + lobbyPlayers.Count);
        // Disable start UI
        ToggleStartUI(false);

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
            GameObject player = Instantiate(lobbyPlayerPrefab.gameObject, GetStartPosition().position, Quaternion.identity) as GameObject;
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
        ToggleHostReadyUI(true);
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

    // Set host UI once all players are ready
    public void ToggleHostReadyUI(bool on)
    {
        readyWaitingText.SetActive(false);
        readyUI.SetActive(on);
    }

    // Set client UI once all players are ready
    public void ToggleClientReadyUI(bool on)
    {
        readyStartButton.SetActive(false);
        readyUI.SetActive(on);
    }


    // Disable start UI
    public void ToggleStartUI( bool on)
    {
        Debug.Log("startUI object " + startUI);
        startUI.SetActive(on);
    }

    // Add spawned lobby player to list
    public void AddLobbyPlayer(GameObject player)
    {
        lobbyPlayers.Add(player.GetComponent<LobbyPlayer>());
    }

    // Update lobby players list
    public void UpdateLobbyPlayers()
    {
        if (lobbyPlayers.Count != numPlayers)
            lobbyPlayers = lobbyPlayers.Where(p => p != null).ToList();
    }

    // Remove lobby player from list
    // This doesn't seem to have use
    // Since even if a client calls this in OnExitClient
    // the server and other clients will not be updated
    public void RemoveLobbyPlayer(GameObject player)
    {
        lobbyPlayers.Remove(player.GetComponent<LobbyPlayer>());
    }

    // Add spawned player to list
    public void AddPlayer(GameObject player)
    {
        Debug.Log("Adding player to lobby manager");
        players.Add(player.GetComponent<Player>());
    }

    // Remove spawned player from list
    public void RemovePlayer(GameObject player)
    {
        players.Remove(player.GetComponent<Player>());
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
    /// Set local player abilities
    /// </summary>
    /// <param name="abilities"></param>
    public void setLocalPlayerAbilities(Abilities abilities)
    {
        PlayerAbilities = abilities;
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

}