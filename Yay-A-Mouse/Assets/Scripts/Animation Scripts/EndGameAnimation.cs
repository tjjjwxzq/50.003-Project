using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Script to be called on the end of the end game animation.
/// Stops host and client processes and returns the player to the Start/Lobby scene
/// </summary>
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
