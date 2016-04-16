using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Net;

/// <summary>
/// Handles start UI buttons as well as
/// checks whether player has saved his/her name before
/// Also sets StartPosition objects for the network manager
/// </summary>
public class StartController : MonoBehaviour {

    private const int MaxPlayers = 4;
    private GameObject canvasObj;
    private GameObject startUI;
    private GameObject waitingUI; // UI to show when player is waiting to join a game
    private GameObject readyUI;
    private GameObject readyWaitingText;
    private GameObject readyStartButton;
    private GameObject startPositionPrefab;
    private LobbyManager lobbyManager; // script that subclasses NetworkLobbyManager
    private MyNetworkDiscovery networkDiscovery; //network discovery component 

    void Start()
    {
        // Get UI components
        canvasObj = GameObject.Find("Canvas");
        startUI = GameObject.Find("StartUI");
        waitingUI = GameObject.Find("WaitingUI");
        readyUI = GameObject.Find("ReadyUI");
        readyWaitingText = readyUI.transform.Find("WaitingText").gameObject;
        readyStartButton = readyUI.transform.Find("StartButton").gameObject;

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

        // deactivate readyUI and waiting UI
        readyUI.SetActive(false);
        waitingUI.SetActive(false);

        // check whether player has saved name
        checkPlayerName();

    }



    public void checkPlayerName()
    {
        string name = PlayerPrefs.GetString("Player Name", "None");
        
        #if UNITY_EDITOR
        PlayerPrefs.DeleteKey("Player Name"); // for testing purposes, means local player will end up with no name when testing in editor 
        # endif 

        // if player hasn't saved name before, go to prompt name scene
        if (name.Equals("None", System.StringComparison.Ordinal))
        {
            SceneManager.LoadScene("PromptName");
        }
        else
        {
            // If not, then set the lobby manager to persist throughout scenes
            Debug.Log("SeT dont destory");
            DontDestroyOnLoad(LobbyManager.singleton);
            //LobbyManager.singleton.dontDestroyOnLoad = true;
        }


    }

    /// <summary>
    /// Callback when Host button is pressed
    /// </summary>
    public void OnHostGame()
    {
        lobbyManager.StartHost();
        Debug.Log("Host name " + Dns.GetHostName());
        foreach(IPAddress ipAdd in Dns.GetHostAddresses(Dns.GetHostName()))
        {
            Debug.Log(ipAdd.ToString());
        }
        // starts client and server host
        // OnStartHost() hook is called when this happens
        // and is used to modify the UI
        // as well as to start broadcasting
        // on the NetworkDiscovery component
    }

    /// <summary>
    /// Callback when Join button is pressed
    /// </summary>
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

        // show waiting UI
        ToggleStartUI(false);
        ToggleWaitingUI(true);
    }

    /// <summary>
    /// Callback when player presses button to cancel
    /// waiting to join a game
    /// </summary>
    public void OnJoinStopWaiting()
    {
        networkDiscovery.StopBroadcast();

        // show start UI
        ToggleWaitingUI(false);
        ToggleStartUI(true);
    }

    /// <summary>
    /// Callback when settings button is pressed
    /// </summary>
    public void OnSettings()
    {
        SceneManager.LoadScene("Settings");
    }

    public void OnHowToPlay()
    {
        SceneManager.LoadScene("HowToPlay");
    }


    /// <summary>
    /// To be called by the LobbyManager to
    /// toggle the startUI
    /// </summary>
    public void ToggleStartUI(bool on)
    {
        startUI.SetActive(on);
    }

    /// <summary>
    /// To be called by the LobbyManager to
    /// toggle the waiting UI
    /// </summary>
    /// <param name="on"></param>
    public void ToggleWaitingUI(bool on)
    {
        waitingUI.SetActive(on);
    }

    /// <summary>
    /// To be called by the LobbyManager
    /// for the Host when all players
    /// in the lobby are ready
    /// </summary>
    /// <param name="on"></param>
    public void ToggleHostReadyUI(bool on)
    {
        readyWaitingText.SetActive(false);
        readyUI.SetActive(on);
    }

    /// <summary>
    /// To be called by the LobbyManager
    /// for clients when all players in
    /// the lobby are ready
    /// </summary>
    /// <param name="on"></param>
    public void ToggleClientReadyUI(bool on)
    {
        readyStartButton.SetActive(false);
        readyUI.SetActive(on);
    }


}

