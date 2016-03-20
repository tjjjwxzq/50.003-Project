using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts
{
    public class Player
    {
        public readonly Abilities Abilities;

        private Player(Abilities abilities)
        {
            Abilities = abilities;
        }

        public static Player MockPlayer
        {
            get
            {
                return new Player(Abilities.StartingAbilities);
            }
        }
    }
}
