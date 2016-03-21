using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Class to contain player specific data, currently only contains the player's Abilities.
/// </summary>
public class Player : NetworkBehaviour
{
    private AbilityController abilityController;

    public string name;

    public Color color;

    public Abilities PAbilities; //<! The player's Abilities

    // network score so other players can see your progress
    // *** @junqi: to access this in other scripts/clients, first initialise Player player, followed by player.score
    [SyncVar]
    public int score = 0;

    // status showing which state is the mouse in (refer to Status enum)
    // *** to access this in other scripts/clients, first initialise Player player, followed by player.status
    [SyncVar]
    public int status = 0;

    // *** this button (any one of the ability buttons on the right) calls the generic method CmdActivateAbilities
    public GameObject button;

    public enum Status
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

    // check which status the mouse is in (by index in the enum above)
    public int checkStatus()
    {
        int check = status;
        return check;
    }

    // to activate the correponding abilities on button press -> Command function is called from the client, but invoked on the server
    // to call a method from the server to the client, use [ClientRpc] instead
    // *** @junqi/jiayu: is this function already taken care of by AbilityControls?
    [Command] // by default on channel 0
    public void CmdActivateAbilities(GameObject button)
    {
        abilityController.ActivateAbility((AbilityName)System.Enum.Parse(typeof(AbilityName), button.name)); // i.e. use this to get AbilityName.Immunity when button.name is Immunity
        status = 0; // change back to normal state
    }

    // method to call list of abilities this player has
    // *** @junqi/jiayu: where's the option for the player to choose the abilities he/she wants such that we can initalise all players to have EmptyAbilities then set the leve of those they choose to be 1?
    public List<Ability> getAbilities()
    {
        return PAbilities.GetListOfAbilities();
    }

    void Update()
    {
        if (!isLocalPlayer)
            return;

        // if player is not at normal state, activate correponding ability
        if (checkStatus() == 0)
        {
            // Command function is called from the client, but invoked on the server
            CmdActivateAbilities(button);
        }
    }
}
