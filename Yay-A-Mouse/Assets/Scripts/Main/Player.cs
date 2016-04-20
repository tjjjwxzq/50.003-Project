using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Networking;


/// <summary>
/// Class to contain player specific data, currently only contains the player's Abilities.
/// </summary>
public class Player : NetworkBehaviour
{
    // For testing
    public bool isLocal;

    private AbilityController abilityController;
    private LevelController levelController;
    private Mouse mouse;

    /// <summary>
    /// Player name synchronized from server
    /// </summary>
    [SyncVar]
    public string Name;

    /// <summary>
    /// Player color synchronized from client
    /// </summary>
    [SyncVar]
    public Color Color;

    /// <summary>
    /// Player abilities
    /// </summary>
    public Abilities PAbilities; 

    /// <summary>
    /// For synchronizing the number of players over host and clients
    /// since LobbyManager.numPlayers is only valid on the host
    /// </summary>
    [SyncVar]
    public int NumPlayers = -1; // set on the Server

    /// <summary>
    /// Player score synchronized from server
    /// </summary>
    [SyncVar]
    public int Score = 0;

    /// <summary>
    /// Player status synchronized from server
    /// </summary>
    [SyncVar]
    public Statuses Status = Statuses.Normal;

    /// <summary>
    /// Flag set when players have chosen their starting abilities and are ready to play
    /// </summary>
    [SyncVar]
    public bool readyToPlay = false;

    /// <summary>
    /// Flag set when player has won the game, synchronized from server
    /// </summary>
    [SyncVar]
    public bool PlayerWon = false;

    public enum Statuses
    {
        Normal,        
        Immunity,      
        TreatsGalore, 
        Fearless,    
        FatMouse,   
        ScaryCat,  
        BeastlyBuffet,
        Thief        
    };

    // Allow player to persist between ability selection and main scene
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        if (NumPlayers <= 0 && isServer)
        {
            NumPlayers = LobbyManager.singleton.numPlayers;
        }

        // Get level controller
        if(levelController == null && SceneManager.GetActiveScene().name == "Main")
        {
            var lc = GameObject.Find("LevelController");
            if (lc != null)
            {
                levelController = lc.GetComponent<LevelController>();
                Debug.Log("Attached to LevelController");
            }
        }

        // Set levelController.NumPlayers on clients and server
        if (levelController != null && NumPlayers > 0)
        {
            levelController.NumPlayers = NumPlayers;
        }

        if (isLocalPlayer)
        {
//            foreach(Ability ability in getAbilities())
//            {
//                Debug.Log("Abilities are " + ability.Name.ToString());
//            }

            // Update score
            if (mouse != null)
            {
                CmdUpdateScore((int)(mouse.Weight* 100f/ mouse.FinalWeight));
            }
        }

        // Check if player has won the game
        if(!PlayerWon)
            checkPlayerWon();
        
    }

    public override void OnStartClient()
    {

        // Add player to network manager
        Debug.Log("Adding player object starting client");
        (LobbyManager.singleton as LobbyManager).AddPlayer(gameObject);

    }

    // Get name of local player
    public override void OnStartLocalPlayer()
    {
        Debug.Log("Adding player object");
        isLocal = isLocalPlayer;
        Debug.Log("In player player name is " + PlayerPrefs.GetString("Player Name"));
        CmdChangeName(PlayerPrefs.GetString("Player Name"));
        Debug.Log("Player object Name is " + Name);
        CmdChangeColor((LobbyManager.singleton as LobbyManager).PlayerColor); // set local player color saved in lobby manager
        Debug.Log(isClient +  " Setting player color" + Color);

        // Initialize abilities for local player
        PAbilities = Abilities.EmptyAbilities;

        // Set local player object on ability selection controller
        GameObject.Find("AbilitySelectionController").GetComponent<AbilitySelectionController>().player = this;
        Debug.Log("Finding ability selection controller null?" + (GameObject.Find("AbilitySelectionController").GetComponent<AbilitySelectionController>() == null));

    }

    /// <summary>
    /// Changes player color on the server
    /// </summary>
    /// <param name="color"></param>
    [Command]
    public void CmdChangeColor(Color color)
    {
        Color = color;
    }

    /// <summary>
    /// Changes player name on the server
    /// </summary>
    /// <param name="name"></param>
    [Command]
    public void CmdChangeName(string name)
    {
        int nonce = PlayerPrefs.GetInt("nonce");
        PlayerPrefs.SetInt("nonce", ++nonce);
        Name = name; // + nonce.ToString();
    }

    /// <summary>
    /// For getting a reference on the Mouse
    /// object when the Main scene begins.
    /// Called by LevelController
    /// </summary>
    public void AttachToMouse()
    {
        mouse = GameObject.Find("Mouse").GetComponent<Mouse>();
        if (mouse != null) Debug.Log(Name + " Player component attached to mouse.");
    }

    /// <summary>
    /// Set the readyToPlay flag on the server
    /// </summary>
    /// <param name="ready"></param>
    [Command]
    public void CmdReadyToPlay(bool ready)
    {
        readyToPlay = ready;
    }



    /// <summary>
    /// Activates the selected ability on the server
    /// </summary>
    /// <param name="button"></param>
    [Command] 
    public void CmdActivateAbilities(GameObject button)
    {
        abilityController.ActivateAbility((AbilityName)System.Enum.Parse(typeof(AbilityName), button.name)); // i.e. use this to get AbilityName.Immunity when button.name is Immunity
        Status = 0; // change back to normal state
    }

    /// <summary>
    /// Get list of player abilities
    /// </summary>
    /// <returns></returns>
    public List<Ability> getAbilities()
    {
        return PAbilities.GetListOfAbilities();
    }

    /// <summary>
    /// Add an ability to the player's list
    /// </summary>
    /// <param name="ability"></param>
    public void addAbility(AbilityName ability)
    {
        if (PAbilities[ability].Level == 0)
            PAbilities.SetAbility(ability, 1);
        else
            Debug.LogWarning("Player already has an ability that you are trying to add");
    }

    /// <summary>
    /// Remove an ability from the player's list
    /// </summary>
    /// <param name="ability"></param>
    public void removeAbility(AbilityName ability)
    {
        if (PAbilities[ability].Level > 0)
            PAbilities.SetAbility(ability, 0);
        else
            Debug.LogWarning("PLayer doesn't have the ability you are trying to remvoe");
    }

    /// <summary>
    /// Checks if player has the ability indicated
    /// </summary>
    /// <param name="ability"></param>
    /// <returns></returns>
    public bool hasAbility(AbilityName ability)
    {
        if (PAbilities[ability].Level > 0)
            return true;
        return false;
    }

    /// <summary>
    /// Updates player score on server
    /// </summary>
    /// <param name="newScore"></param>
    [Command]
    public void CmdUpdateScore(int newScore)
    {
        Score = newScore;
    }

    /// <summary>
    /// Set PlayerWon flag on server
    /// </summary>
    /// <param name="won"></param>
    [Command]
    public void CmdUpdatePlayerWon(bool won)
    {
        PlayerWon = won;
    }

    /// <summary>
    /// Check which status the mouse is in
    /// for updating status on avatar (TODO)
    /// </summary>
    /// <returns></returns>
    public Statuses checkStatus()
    {
        return Status;
    }

    /// <summary>
    /// Checks whether player has won and updates flags accordingly
    /// </summary>
    public void checkPlayerWon()
    {
        if(Score == 100)
        {
            Debug.Log("Player has won!");
            CmdUpdatePlayerWon(true);
        }

    }


}
