using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LobbyButton : MonoBehaviour
{
    Text lobbyButtonText;

    // Use this for initialization
    void Start()
    {
        lobbyButtonText = transform.FindChild("Text").GetComponent<Text>() as Text;
        lobbyButtonText.text = "Enter Lobby";
    }

    public void ButtonPressed()
    {
        Debug.Log("Button was pressed");
    }
    //SceneManager.LoadScene("Lobby");
}
