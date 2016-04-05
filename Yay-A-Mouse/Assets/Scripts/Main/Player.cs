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

    [SyncVar]
    public string Name;

    [SyncVar]
    public Color Color;

    public Abilities PAbilities; // The player's Abilities

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

    public bool readyToPlay = false;

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

    // Allow player to persist between ability selection and main scene
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        if (isServer)
        {
            NumPlayers = LobbyManager.singleton.numPlayers;
        }
        // Set levelController.NumPlayers on clients and server
        if(levelController != null && NumPlayers > 0)
            levelController.NumPlayers = NumPlayers;

        if (isLocalPlayer)
        {
            foreach(Ability ability in getAbilities())
            {
                Debug.Log("Abilities are " + ability.Name.ToString());
            }

            // Add ability controller to local player
            if (abilityController == null && SceneManager.GetActiveScene().name.Equals("Main", System.StringComparison.Ordinal))
            {
                Debug.Log("Adding ability controller");
                abilityController = gameObject.AddComponent<AbilityController>();
            }
        }
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
        //levelController = GameObject.Find("LevelController").GetComponent<LevelController>();

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

        // Add ability controller but disable it
        abilityController = gameObject.AddComponent<AbilityController>();
        abilityController.enabled = false;

        // Set local player object on ability selection controller
        GameObject.Find("AbilitySelectionController").GetComponent<AbilitySelectionController>().player = this;
        Debug.Log("Finding ability selection controller null?" + (GameObject.Find("AbilitySelectionController").GetComponent<AbilitySelectionController>() == null));

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

    // Tell server that local player is ready to play
    [Command]
    public void CmdReadyToPlay(bool ready)
    {
        readyToPlay = ready;
    }



    // to activate the correponding abilities on button press -> Command function is called from the client, but invoked on the server
    // to call a method from the server to the client, use [ClientRpc] instead
    [Command] // by default on channel 0
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

    // check which status the mouse is in (by index in the enum above)
    public Statuses checkStatus()
    {
        return Status;
    }


}
