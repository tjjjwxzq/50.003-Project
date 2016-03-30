using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Handles start UI buttons as well as
/// checks whether player has saved his/her name before
/// Also sets StartPosition objects for the network manager
/// </summary>
public class StartController : MonoBehaviour {

    private const int MaxPlayers = 4;
    private GameObject canvasObj;
    private GameObject startPositionPrefab;
    private LobbyManager lobbyManager; // script that subclasses NetworkLobbyManager
    private MyNetworkDiscovery networkDiscovery; //network discovery component 

    void Start()
    {
        canvasObj = GameObject.Find("Canvas");
        lobbyManager = LobbyManager.singleton as LobbyManager;
        networkDiscovery = LobbyManager.singleton.GetComponent<MyNetworkDiscovery>();
        startPositionPrefab = Resources.Load("Prefabs/StartPosition") as GameObject;
        RectTransform playerTransform = lobbyManager.lobbyPlayerPrefab.GetComponent<RectTransform>();

        float height = 140f;
        float offset = playerTransform.rect.height * lobbyManager.lobbyPlayerPrefab.GetComponent<LobbyPlayer>().playerScale * 1.2f; //refactor this later
        //float offset = lobbyManager.playerTransform.rect.height * lobbyManager.playerTransform.localScale.y * 1.2f;
        for(int i = 0; i < MaxPlayers; i++)
        {
            GameObject start = Instantiate(startPositionPrefab, new Vector2(0, height - offset * i), Quaternion.identity) as GameObject;
            start.transform.SetParent(canvasObj.transform, false);

        }

        // check whether player has saved name
        checkPlayerName();

    }



    public void checkPlayerName()
    {
       /* #if UNITY_EDITOR
        PlayerPrefs.DeleteKey("Player Name"); // for testing purposes 
        # endif */
        string name = PlayerPrefs.GetString("Player Name", "None");
        Debug.Log("Player name is " + name);
        // if player hasn't saved name before, go to prompt name scene
        if (name.Equals("None", System.StringComparison.Ordinal))
        {
            SceneManager.LoadScene("PromptName");
        }

    }

    public void OnHostGame()
    {
        lobbyManager.StartHost(); 
        // starts client and server host
        // OnStartHost() hook is called when this happens
        // and is used to modify the UI
        // as well as to start broadcasting
        // on the NetworkDiscovery component
    }

    public void OnJoinGame()
    {
        // start client network discovery
        // client listens for broadcast messages
        // from the server, and the OnReceivedBroadcast
        // hook is called when it receives a message
        // In the hook method, the network manager
        // starts a client connected to the server
        // address specified address
        networkDiscovery.Initialize();
        networkDiscovery.StartAsClient();

    }

}
