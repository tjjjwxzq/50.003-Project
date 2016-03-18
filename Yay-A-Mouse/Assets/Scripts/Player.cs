using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Assets.Scripts
{
    /// <summary>
    /// Class to contain player specific data, currently only contains the player's Abilities.
    /// </summary>
    public class Player : NetworkBehaviour
    {
        private AbilityController abilityController;
        public string name;
        public enum Status
        {
            None,
//
            FeedingFrenzy
        }
        public int score;
        public Abilities Abilities; //<! The player's Abilities
        public AbilityController abilityController;

        // network score so other players can see your progress
        [SyncVar]
        public int score = 0;

        // status showing which state is the mouse in (refer to Status enum)
        public int status = 0;

        // TODO: add in method to call list of abilities this player has

        public GameObject button; // this button (any one of the ability buttons on the right) calls the generic method CmdActivateAbilities

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
            Abilities = abilities;
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

        // to activate the correponding abilities on button press
        /*[Command] // by default on channel 0
        public void CmdActivateAbilities(GameObject button)
        {
            abilityController.ActivateAbility(button.name());
        }*/

        void Update()
        {
            if (!isLocalPlayer)
                return;

            // check if the ability button is pressed
            /*if (button.onPress())
            {
                CmdActivateAbilities(button); // this method is networked
            }*/
        }
    }
}
