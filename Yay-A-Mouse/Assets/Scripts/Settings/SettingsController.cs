using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SettingsController : MonoBehaviour {
    public AudioClip SoundLobby;
    public AudioClip SoundButton;

    private AudioSource audio;

    private InputField inputField;

	// Use this for initialization
	void Start () {
        audio = gameObject.AddComponent<AudioSource>();
        audio.PlayOneShot(SoundLobby);

        inputField = GameObject.Find("InputField").GetComponent<InputField>();

        string currentName = PlayerPrefs.GetString("Player Name", "None");
        inputField.text = currentName;

        // Destroy network manager, in case it persists into this scene
        GameObject lobbyManager = GameObject.Find("NetworkLobbyManager");
        if (lobbyManager != null)
            Destroy(lobbyManager);
	
	}

    /// <summary>
    /// Callback when OK button is pressed
    /// </summary>
    public void OnOKButton()
    {
        audio.PlayOneShot(SoundButton);

        if (inputField.text.Length > 0)
        {
            PlayerPrefs.SetString("Player Name", inputField.text);
            SceneManager.LoadScene("Lobby");
        }
    }

}
