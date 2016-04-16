using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class EndGameAnimation : MonoBehaviour
{
    public void OnEndGame()
    {
        LobbyManager lobbyManager = LobbyManager.singleton as LobbyManager;
        if(lobbyManager.IsHost)
        {
            lobbyManager.StopHost();
        }
        else
        {
            lobbyManager.StopClient();
        }

        SceneManager.LoadScene("Lobby");
    }


}
