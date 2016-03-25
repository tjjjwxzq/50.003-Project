using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class LobbyPlayer : NetworkLobbyPlayer{

    public bool isLocal;
    public bool isServe;

    [SyncVar(hook = "OnChangeName")]
    private string PlayerName;

    [SyncVar(hook = "OnChangeColor")]
    public Color PlayerColor;

    // UI components
    public float playerScale = 0.3f;
    private GameObject canvasObj;
    private Image playerImage;
    private Text playerNameText;

	// Use this for initialization
	void Start () {
        Debug.Log("NUm players is " + LobbyManager.singleton.numPlayers);
	}
	
    public override void OnStartLocalPlayer()
    {
        Debug.Log("Local player starting");
        // Set name for local player
        CmdChangeName(PlayerPrefs.GetString("Player Name"));
        // Set color for local player
        setColor();
        isLocal = isLocalPlayer;
        isServe = isServer;
    }

    public override void OnClientEnterLobby()
    {
        // Add player object to canvas and intialize UI components
        canvasObj = GameObject.Find("Canvas");
        transform.SetParent(canvasObj.transform, true);
        transform.localScale = new Vector3(playerScale, playerScale, playerScale);
        playerImage = GetComponent<Image>();
        playerNameText = transform.GetChild(0).GetComponent<Text>();
    }

    // Update UI
    public void OnChangeName(string newName)
    {
        PlayerName = newName;
        playerNameText.text = PlayerName;
    }

    public void OnChangeColor(Color newColor)
    {
        PlayerColor = newColor;
        playerImage.color = PlayerColor;
    }

    // Commands to server
    [Command]
    public void CmdChangeName(string name)
    {
        PlayerName = name;
    }

    [Command]
    public void CmdChangeColor(Color color)
    {
        PlayerColor = color;
    }


    // Quit Button
    public void OnQuit()
    {
        if (isLocalPlayer)
        {
            // so what happens to the player object on other clients
            // when one client quits?
            // Disconnect from server
            if (isServer)
            {
                // if hosting, stop client and server
                LobbyManager.singleton.StopHost();
                Debug.Log("Stopping host");
            }
            else
            {
                // else simply stop client
                LobbyManager.singleton.StopClient();
                Debug.Log("Stopping client");
            }
            // Reset the UI
            (LobbyManager.singleton as LobbyManager).ToggleStartUI(true);

        }
    }

    // Set color
    private void setColor()
    {
        foreach(Color color in ColorController.Colors)
        {
            if (!ColorController.UsedColors.Contains(color))
            {
                ColorController.UsedColors.Add(color);
                Debug.Log("Setting color " + color);
                CmdChangeColor(color);
                break;
            }
        }

    }

	// Update is called once per frame
	void Update () {
	
	}
}
