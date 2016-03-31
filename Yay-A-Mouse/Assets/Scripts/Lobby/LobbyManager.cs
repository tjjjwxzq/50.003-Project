using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Networking;

public class LobbyManager : NetworkLobbyManager {

    // For Lobby Scene
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

        DontDestroyOnLoad(gameObject);
    }
	
	// Update is called once per frame
	void Update () {
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
        Debug.Log("Lobby manager num players are " + players.Count);
        if(!changingScene && players.Count > 0)
        {
            checkAbilitiesReady();
        }

        if(readyToPlay)
        {
            ServerChangeScene("Main");
            readyToPlay = false;
            changingScene = true;
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
        // Disable start UI
        ToggleStartUI(false);
    }

    // Customize player spawning
    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
    {
        if(SceneManager.GetActiveScene().name.Equals("Lobby", System.StringComparison.Ordinal)){
            GameObject player = Instantiate(lobbyPlayerPrefab.gameObject, GetStartPosition().position, Quaternion.identity) as GameObject;
            //player.transform.SetParent(canvasObj.transform, false);
            NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);
        }
        else if(SceneManager.GetActiveScene().name.Equals("SelectAbilities", System.StringComparison.Ordinal))
        {
            GameObject player = Instantiate(gamePlayerPrefab) as GameObject;
            NetworkServer.ReplacePlayerForConnection(conn, player, playerControllerId);
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

    // Check players ready
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

    // Remove lobby player from list
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

    // Start Game button callback (only on host) 
    public void OnStartGame()
    {
        ServerChangeScene("SelectAbilities");
    }

    // Ability selection ready button callback
    private void checkAbilitiesReady()
    {
        readyToPlay = players.All(p => p.readyToPlay);
    }

}
