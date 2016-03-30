using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
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

    [SyncVar]
    public string Name;

    [SyncVar]
    public Color Color;

    public Abilities PAbilities; //<! The player's Abilities

    // For synchronizing the number of players over host and clients
    // scine LobbyManager.numPlayers is only valid on the host
    [SyncVar]
    public int NumPlayers = -1; // set on the Server

    // network score so other players can see your progress
    // *** @junqi: to access this in other scripts/clients, first initialise Player player, followed by player.score
    [SyncVar]
    public int Score = 0;

    // status showing which state is the mouse in (refer to Status enum)
    // *** to access this in other scripts/clients, first initialise Player player, followed by player.status
    [SyncVar]
    public Statuses Status = 0;

    // *** this button (any one of the ability buttons on the right) calls the generic method CmdActivateAbilities
    public GameObject button;

    public enum Statuses
    {
        Normal,        // index 0
        Immunity,      // index 1
        TreatsGalore,  // index 2
        Fearless,      // index 3
        FatMouse,      // index 4
        ScaryCat,      // index 5
        BeastlyBuffet, // index 6
        Thief          // index 7
    };

    /// <summary>
    /// Creates a new Player with a specified set of abilities.
    /// </summary>
    /// <param name="abilities"></param>
    public Player(Abilities abilities)
    {
        this.PAbilities = abilities;
    }

    /// <summary>
    /// Mock player with all abilities at level 1.
    /// </summary>
    public static Player MockPlayer
    {
        get
        {
            return new Player(Abilities.LevelOneAbilities);
        }
    }

    void Start()
    {
    }

    void Update()
    {
        if (isServer)
        {
            NumPlayers = LobbyManager.singleton.numPlayers;
        }
        // Set levelController.NumPlayers on clients and server
        if(NumPlayers > 0)
            levelController.NumPlayers = NumPlayers;
        /*
        // if player is at normal state, activate correponding ability
        if (checkStatus() == Statuses.Normal)
        {
            // Command function is called from the client, but invoked on the server
            CmdActivateAbilities(button);
        }*/
    }

    public override void OnStartClient()
    {
        // Get level controller
        levelController = GameObject.Find("LevelController").GetComponent<LevelController>();

    }

    // Get name of local player
    public override void OnStartLocalPlayer()
    {
        isLocal = isLocalPlayer;
        Debug.Log("In player player name is " + PlayerPrefs.GetString("Player Name"));
        CmdChangeName(PlayerPrefs.GetString("Player Name"));
        Debug.Log("Player object Name is " + Name);
        CmdChangeColor((LobbyManager.singleton as LobbyManager).PlayerColor); // set local player color saved in lobby manager
        Debug.Log(isClient +  " Setting player color" + Color);
        // Get and set number of players if on server
        // On clients NumPlayers will have to be set in Update()
        // after it has been synchronized from the server
        // Player.NumPlayers is only guaranteed to be set correctly on the local player
    }

    // Synchronize local player color set on client with the server
    [Command]
    public void CmdChangeColor(Color color)
    {
        Color = color;
    }

    // Syncrhonize local player name set on client with the server
    [Command]
    public void CmdChangeName(string name)
    {
        Name = name;
    }



    // to activate the correponding abilities on button press -> Command function is called from the client, but invoked on the server
    // to call a method from the server to the client, use [ClientRpc] instead
    // *** @junqi/jiayu: is this function already taken care of by AbilityControls?
    [Command] // by default on channel 0
    public void CmdActivateAbilities(GameObject button)
    {
        abilityController.ActivateAbility((AbilityName)System.Enum.Parse(typeof(AbilityName), button.name)); // i.e. use this to get AbilityName.Immunity when button.name is Immunity
        Status = 0; // change back to normal state
    }

    // method to call list of abilities this player has
    // *** @junqi/jiayu: where's the option for the player to choose the abilities he/she wants such that we can initalise all players to have EmptyAbilities then set the leve of those they choose to be 1?
    public List<Ability> getAbilities()
    {
        return PAbilities.GetListOfAbilities();
    }
    // check which status the mouse is in (by index in the enum above)
    public Statuses checkStatus()
    {
        return Status;
    }


}
