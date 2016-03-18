using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts
{
    /// <summary>
    /// Class to contain player specific data, currently only contains the player's Abilities.
    /// </summary>
    public class Player
    {
        public Abilities Abilities; //<! The player's Abilities

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
                return new Player(Abilities.StartingAbilities);
            }
        }
    }
}
