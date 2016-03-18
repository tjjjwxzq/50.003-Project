using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Handles game state, such as the player's combo streak,
/// whether or not the player is in frenzy feeding mode,
/// and the number of players in a game.
/// Handles state of combo and player avatar UI as well (?) 
/// </summary>
public class LevelController : MonoBehaviour {

    // Get this info from the NetworkManager
    private int numPlayers = 3;

    // Container UI game objects
    private GameObject comboUI;
    private Image[] comboImages = new Image[3];
    private GameObject playerAvatarUI;
    private GameObject playerAvatar; // player avatar prefab
    private GameObject[] playerAvatars; // player avatars in UI
    private Color[] playerColors; // player avatar colors
    private string[] playerScores; // player scores
    private Text[] playerScoresText; // player scores text components
    private string[] playerNames; // player names
    private Text[] playerNamesText; // player names text components


	// Use this for initialization
	void Start () {
        // Get Combo UI
        comboUI = GameObject.Find("Combo");
        for(int i = 0; i < comboImages.Length; i++)
        {
            comboImages[i] = comboUI.transform.GetChild(0).GetComponentInChildren<Image>();
        }

        // Get players UI
        playerAvatarUI = GameObject.Find("Players");

        // Get player colors, names and scores (from network manager?)
        playerColors = new Color[numPlayers];
        playerNames = new string[numPlayers];
        playerNamesText = new Text[numPlayers];
        playerScores = new string[numPlayers];
        playerScoresText = new Text[numPlayers];
        for(int i =0; i < playerColors.Length; i++)
        {
            playerColors[i] = new Color(100, 100, 100, 1);
            playerNames[i] = "Player " + i;
            playerScores[i] = "0";

        }


        // Get player avatar prefab
        playerAvatar = Resources.Load("Prefabs/PlayerAvatar") as GameObject;
        float width = playerAvatar.GetComponent<RectTransform>().rect.width;

        playerAvatars = new GameObject[numPlayers];

        for(int i = 0; i < playerAvatars.Length; i++)
        {
            playerAvatars[i] = (GameObject)Instantiate(playerAvatar);
            playerAvatars[i].GetComponent<Image>().color = playerColors[i];

            // Get text component and set name text
            playerNamesText[i] = playerAvatars[i].transform.Find("PlayerName").gameObject.GetComponent<Text>();
            playerNamesText[i].text = playerNames[i];

            // Get text component and set score text
            playerScoresText[i] = playerAvatars[i].transform.Find("PlayerScore").gameObject.GetComponent<Text>();
            playerScoresText[i].text = playerScores[i];

            playerAvatars[i].transform.SetParent(playerAvatarUI.transform, false);
            playerAvatars[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(
                (i-1) * playerAvatars[i].transform.localScale.x * width * 1.25f, 0);
        }
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
