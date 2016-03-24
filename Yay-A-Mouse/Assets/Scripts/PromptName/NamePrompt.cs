using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class NamePrompt : MonoBehaviour {

    // not sure whether it's more efficient to assign through the editor
    private InputField nameInput;
    private Text promptText;
  
	// Use this for initialization
	void Start () {
        nameInput = GameObject.Find("NameInput").GetComponent<InputField>();
        promptText = GameObject.Find("Prompt").GetComponent<Text>();
	}

    public void OnOKButton()
    {
        string name = nameInput.text;
        if(name.Length > 0)
        {
            PlayerPrefs.SetString("Player Name", name);
        }
        else
        {
            promptText.text = "Please enter your name!";
            nameInput.Select();
        }



    }
	
}
