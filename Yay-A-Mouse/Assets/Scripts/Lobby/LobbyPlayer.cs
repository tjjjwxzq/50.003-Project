﻿using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class LobbyPlayer : NetworkLobbyPlayer{

    // For testing, to check local player in inspector
    public bool isLocal;
    public bool isServe;

    [SyncVar]
    public bool PlayerReady;

    [SyncVar(hook = "OnChangeName")]
    public string PlayerName;

    [SyncVar(hook = "OnChangeColor")]
    public Color PlayerColor;
    private Color[] Colors = { Color.red, Color.green, Color.blue, Color.yellow };
    private int currentColorIndex = 0;

    // For positioning player in lobby
    private RectTransform lobbyPlayerRectTransform;
    private float lobbyPlayerStartPos = 140f; // starting position of lobby players
    private float lobbyPlayerOffset; // offset between each player

    [SyncVar]
    public int PlayerPosition; // set by LobbyManager when player is added on server

    // UI components
    public float playerScale = 0.3f;
    private GameObject canvasObj;
    private Image playerImage;
    private Text playerNameText;
    private GameObject playerQuitButtonObj; // for quitting lobby
    private GameObject playerReadyButtonObj; // for sending ready message
    private Button playerButton; // for switching colors

    // Use this for initialization
    void Start () {
    }
	
    public override void OnStartLocalPlayer()
    {
        // Set name for local player
        Debug.Log("Name is " + PlayerPrefs.GetString("Player Name"));
        CmdChangeName(PlayerPrefs.GetString("Player Name"));
        // Set color for local player
        // I don't really understand why I don't need to call
        // setColor to set the color correctly for a newly joined
        // client, but it seems to set correctly like this, so...
        setColor();
        CmdChangeColor(PlayerColor);
        // Show quit and ready button if local player
        playerQuitButtonObj.SetActive(true);
        playerReadyButtonObj.SetActive(true);
        // Enable toggle color button
        playerButton.interactable = true;
        //Debug purposes (identify local player in inspector)
        isLocal = isLocalPlayer;
        isServe = isServer;
    }

    public override void OnClientEnterLobby()
    {
        Debug.Log("Client entering lobby");
        Debug.Log("Player color client is " + PlayerColor);

        // Get rect transform
        lobbyPlayerRectTransform = GetComponent<RectTransform>();
        lobbyPlayerOffset = lobbyPlayerRectTransform.rect.height * lobbyPlayerRectTransform.localScale.y * 1.2f;

        // Add player object to canvas and intialize UI components
        canvasObj = GameObject.Find("Canvas");
        lobbyPlayerRectTransform.anchoredPosition = new Vector2(0, lobbyPlayerStartPos - lobbyPlayerOffset * PlayerPosition);
        transform.SetParent(canvasObj.transform, false);
        transform.localScale = new Vector3(playerScale, playerScale, playerScale);

        playerImage = GetComponent<Image>();
        playerNameText = transform.Find("Name").GetComponent<Text>();
        playerQuitButtonObj = transform.Find("QuitButton").gameObject;
        playerReadyButtonObj = transform.Find("ReadyButton").gameObject;
        playerButton = GetComponent<Button>();
        // Disable quit and ready buttons on all players
        // Reenable only on local player in OnStartLocalPlayer
        playerQuitButtonObj.SetActive(false);
        playerReadyButtonObj.SetActive(false);
        // Disable toggle color button on all players
        // Reenable only on local player in OnStartLocalPlayer
        playerButton.interactable = false;

        // Update the UI instantly, before syncvar hooks
        // This is to ensure the UI changes even if the
        // syncvar is not changed from the server
        OnChangeColor(PlayerColor); 
        OnChangeName(PlayerName);

        // Add player object to list in lobby manager
        (LobbyManager.singleton as LobbyManager).AddLobbyPlayer(gameObject);
        
    }

    public override void OnClientReady(bool readyState)
    {
        PlayerReady = readyState; // update player ready sync var on clients
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

    // Button Callbacks

    // Ready button
    public void OnReady()
    {
        if (!readyToBegin)
        {
            Debug.Log("readying");
            // Change button image
            // Disable color selection
            playerButton.interactable = false;
            // Save player color in LobbyManager
            (LobbyManager.singleton as LobbyManager).setLocalPlayerColor(PlayerColor);
            SendReadyToBeginMessage();
        }
        else
        {
            Debug.Log("Not ready");
            // Reset button image
            // Reenable color selection
            playerButton.interactable = true;
            SendNotReadyToBeginMessage();
        }
    }

    // Callback for button to toggle Player color
    public void OnToggleColor()
    {
        setColor();
        CmdChangeColor(PlayerColor);
    }


    // Quit Button
    public void OnQuit()
    {
        if (isLocalPlayer)
        {
            // so what happens to the player object on other clients
            // when one client quits? It seems to be removed, which is good
            // Disconnect from server
            if (isServer)
            {
                // Remove player object
                RemovePlayer();
                // if hosting, stop client and server
                LobbyManager.singleton.StopHost();
                Debug.Log("Stopping host");
            }
            else
            {
                // Remove player object
                RemovePlayer();
                // else simply stop client
                LobbyManager.singleton.StopClient();
                Debug.Log("Stopping client");
            }
            // Reset the UI
            GameObject.Find("StartController").GetComponent<StartController>().ToggleStartUI(true);

        }
    }

    // Set color
    private void setColor()
    {
        bool colorSet = false;
        LobbyManager lobbyManager = LobbyManager.singleton as LobbyManager;
        for(int i = currentColorIndex; i < Colors.Length; i++)
        {
            Color color = Colors[i];
           if (! lobbyManager.getUsedColors().Contains(color))
            {
                Debug.Log("Setting color");
                //colorController.CmdRemoveUsedColors(currentColorIndex);
                //colorController.CmdAddUsedColors(i);
                PlayerColor = color;
                currentColorIndex = i;
                colorSet = true;
                break;
            }
       }

        // If color is not yet set, start looking through color list from beginning
        if (!colorSet)
        {
            for(int i = 0; i < Colors.Length; i++)
            {
                Color color = Colors[i];
                if(!lobbyManager.getUsedColors().Contains(color))
                {
                    Debug.Log("Setting color 2");
                    PlayerColor = color;
                    currentColorIndex = i;
                    break;
                }
            }
 
        }
    }

	// Update is called once per frame
	void Update () {
	
	}
}
