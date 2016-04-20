using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class NamePrompt : MonoBehaviour {

    public AudioClip SoundButton;
    private AudioSource audio;

    // not sure whether it's more efficient to assign through the editor
   
    private InputField nameInput;
    private Text promptText;
  
	// Use this for initialization
	void Start () {
        audio = GetComponent<AudioSource>();
        nameInput = GameObject.Find("NameInput").GetComponent<InputField>();
        promptText = GameObject.Find("Prompt").GetComponent<Text>();
	}

    public void OnOKButton()
    {
        audio.PlayOneShot(SoundButton);

        string name = nameInput.text;
        if(name.Length > 0)
        {
            PlayerPrefs.SetString("Player Name", name);
            SceneManager.LoadScene("Lobby");
        }
        else
        {
            promptText.text = "Please enter your name!";
            nameInput.Select();
        }



    }
	
}
