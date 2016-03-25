using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class LobbyManager : NetworkLobbyManager {

    private MyNetworkDiscovery networkDiscovery;
    private GameObject canvasObj;
    private GameObject startUI;
    public RectTransform playerTransform; // to be accessed by StartController to set spawn positions

	// Use this for initialization
	void Start () {
        // Set round robin player spawn method
        playerSpawnMethod = PlayerSpawnMethod.RoundRobin;

        networkDiscovery = GetComponent<MyNetworkDiscovery>();
        canvasObj = GameObject.Find("Canvas");
        startUI = GameObject.Find("StartUI");
        playerTransform = playerPrefab.GetComponent<RectTransform>();

    }
	
	// Update is called once per frame
	void Update () {
	
	}

    // Start network discovery broadcasting
    public override void OnStartHost()
    {
        networkDiscovery.Initialize();
        networkDiscovery.StartAsServer();
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
        GameObject player = Instantiate(playerPrefab.gameObject, GetStartPosition().position, Quaternion.identity) as GameObject;
        //player.transform.SetParent(canvasObj.transform, false);
        NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);
    }

    public void OnLobbyClientDisconnect()
    {
        Debug.Log("Client disconnected");

    }

    public override void OnStopServer()
    {
        Debug.Log("Server stopped");
    }

    // Disable start UI
    public void ToggleStartUI( bool on)
    {
        startUI.SetActive(on);
    }


}
