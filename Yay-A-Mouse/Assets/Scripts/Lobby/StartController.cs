using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Collections;

public class StartController : MonoBehaviour {

    private LobbyManager lobbyManager; // script that subclasses NetworkLobbyManager
    private MyNetworkDiscovery networkDiscovery; //network discovery component 

    public void checkPlayerName()
    {
        #if UNITY_EDITOR
        PlayerPrefs.DeleteKey("Player Name"); // for testing purposes 
        # endif 
        string name = PlayerPrefs.GetString("Player Name");
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
        networkDiscovery.Initialize();
        networkDiscovery.StartAsClient();

    }

    void Start()
    {
        // check whether player has saved name
        checkPlayerName();

    }

}
