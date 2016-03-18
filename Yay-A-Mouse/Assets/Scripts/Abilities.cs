using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngineInternal;

namespace Assets.Scripts
{
    /// <summary>
    /// Container class for a player's abilities at any point in time. Always associated with a Player instance.
    /// </summary>
    public class Abilities
    {
        public Immunity Immunity;             //!< Player's Immunity ability
        public TreatsGalore TreatsGalore;     //<! Player's Treats Galore ability
        public Fearless Fearless;             //!< Player's Fearless ability
        public FatMouse FatMouse;             //!< Player's Fat Mouse ability
        public ScaryCat ScaryCat;             //!< Player's Scary Cat ability
        public BeastlyBuffet BeastlyBuffet;   //!< Player's Beastly Buffet ability
        public Thief Thief;                   //!< Player's Thief ability

        public Ability this[AbilityName ability] //!< Convenience property to index abilities by AbilityName enum
        {
            get
            {
                switch (ability)
                {
                    case AbilityName.Immunity:
                        return Immunity;
                    case AbilityName.TreatsGalore:
                        return TreatsGalore;
                    case AbilityName.Fearless:
                        return Fearless;
                    case AbilityName.FatMouse:
                        return FatMouse;
                    case AbilityName.ScaryCat:
                        return ScaryCat;
                    case AbilityName.BeastlyBuffet:
                        return BeastlyBuffet;
                    case AbilityName.Thief:
                        return Thief;
                    default:
                        throw new ArgumentOutOfRangeException("ability", ability, "No such ability.");
                }
            }
        }

        private Abilities(int immunityLevel, int treatsGaloreLevel, int fearlessLevel, int fatMouseLevel, int scaryCatLevel, int beastlyBuffetLevel, int thiefLevel)
        {
            Immunity = new Immunity(immunityLevel);
            TreatsGalore = new TreatsGalore(treatsGaloreLevel);
            Fearless = new Fearless(fearlessLevel);
            FatMouse = new FatMouse(fatMouseLevel);
            ScaryCat = new ScaryCat(scaryCatLevel);
            BeastlyBuffet = new BeastlyBuffet(beastlyBuffetLevel);
            Thief = new Thief(thiefLevel);
        }

        public static Abilities LevelOneAbilities //<! Abilities instance containing all abilities at level 1
        {
            get { return new Abilities(1, 1, 1, 1, 1, 1, 1); }
        }

        public static Abilities EmptyAbilities //<! Empty set of abilities
        {
            get { return new Abilities(0, 0, 0, 0, 0, 0, 0); }
        }

        /// <summary>
        /// Method to set a player's ability to a certain level.
        /// </summary>
        /// <param name="ability">Ability to be changed</param>
        /// <param name="level">New level</param>
        public void SetAbility(AbilityName ability, int level)
        {
            switch (ability)
            {
                case AbilityName.Immunity:
                    Immunity = new Immunity(level);
                    break;
                case AbilityName.TreatsGalore:
                    TreatsGalore = new TreatsGalore(level);
                    break;
                case AbilityName.Fearless:
                    Fearless = new Fearless(level);
                    break;
                case AbilityName.FatMouse:
                    FatMouse = new FatMouse(level);
                    break;
                case AbilityName.ScaryCat:
                    ScaryCat = new ScaryCat(level);
                    break;
                case AbilityName.BeastlyBuffet:
                    BeastlyBuffet = new BeastlyBuffet(level);
                    break;
                case AbilityName.Thief:
                    Thief = new Thief(level);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("ability", ability, "No such ability.");
            }
        }

        public List<Ability> GetListOfAbilities()
        {
            return (from AbilityName abilityName in Enum.GetValues(typeof (AbilityName)) where this[abilityName].Level != 0 select (Ability) this[abilityName]).ToList();
        }
    }

    /// <summary>
    /// Enum containing the names of all mouse abilities.
    /// </summary>
    public enum AbilityName
    {
        Immunity,
        TreatsGalore,
        Fearless,
        FatMouse,
        ScaryCat,
        BeastlyBuffet,
        Thief
    }

    /// <summary>
    /// Base class to where all abilities with a Happiness cost and limited duration are derived from.
    //// Ability instances are always associated with an Abilities instance which is associated with a Player instance.
    /// </summary>
    public abstract class Ability
    {
        public int Level { get; protected set; }
        public int Cost { get; protected set; }
        public int Duration { get; protected set; }
    }

    /// <summary>
    /// A player's Immunity ability.
    /// </summary>
    public class Immunity : Ability
    {
        public Immunity(int level)
        {
            Level = level;
            switch (level)
            {
                case 0:
                    break;
                case 1:
                    Cost = 30;
                    Duration = 10;
                    break;
                case 2:
                    Cost = 30;
                    Duration = 30;
                    break;
                default:
                    throw new NotImplementedException("level out of range");
            }
        }
    }

    /// <summary>
    /// A player's Treats Galore ability.
    /// </summary>
    public class TreatsGalore : Ability
    {
        public int PointThreshold { get; private set; }
        public int SpawnWeightMultiplier { get; private set; }
        public int SpawnLimitMultiplier { get; private set; }

        public TreatsGalore(int level)
        {
            Level = level;
            switch (level)
            {
                case 0:
                    break;
                case 1:
                    Cost = 80;
                    Duration = 15;
                    PointThreshold = 0;
                    SpawnWeightMultiplier = 10;
                    SpawnLimitMultiplier = 10;
                    break;
                case 2:
                    Cost = 80;
                    Duration = 15;
                    PointThreshold = 10;
                    SpawnWeightMultiplier = 10;
                    SpawnLimitMultiplier = 10;
                    break;
                case 3:
                    Cost = 80;
                    Duration = 15;
                    PointThreshold = 10;
                    SpawnWeightMultiplier = 100;
                    SpawnLimitMultiplier = 100;
                    break;
                default:
                    throw new NotImplementedException("level out of range");
            }
        }
    }

    /// <summary>
    /// A player's Fearless ability.
    /// </summary>
    public class Fearless : Ability
    {
        public bool DropHappiness { get; private set; }
        public bool DropWeight { get; private set; }

        public Fearless(int level)
        {
            Level = level;
            switch (level)
            {
                case 0:
                    break;
                case 1:
                    Cost = 40;
                    Duration = 30;
                    DropHappiness = true;
                    DropWeight = true;
                    break;
                case 2:
                    Cost = 40;
                    Duration = 30;
                    DropHappiness = true;
                    DropWeight = false;
                    break;
                case 3:
                    Cost = 40;
                    Duration = 30;
                    DropHappiness = false;
                    DropWeight = false;
                    break;
                default:
                    throw new NotImplementedException("level out of range");
            }
        }
    }

    /// <summary>
    /// A player's Fat Mouse ability.
    /// </summary>
    public class FatMouse : Ability
    {
        public int WeightMultiplier { get; private set; }

        public FatMouse(int level)
        {
            Level = level;
            switch (level)
            {
                case 0:
                    break;
                case 1:
                    Cost = 70;
                    Duration = 15;
                    WeightMultiplier = 2;
                    break;
                case 2:
                    Cost = 70;
                    Duration = 15;
                    WeightMultiplier = 4;
                    break;
                default:
                    throw new NotImplementedException("level out of range");
            }
        }
    }

    /// <summary>
    ///  A player's Scary Cat ability.
    /// </summary>
    public class ScaryCat : Ability
    {
        public int HappinessReduction { get; private set; }
        public int WeightReduction { get; private set; }

        public ScaryCat(int level)
        {
            Level = level;
            switch (level)
            {
                case 0:
                    break;
                case 1:
                    Cost = 60;
                    Duration = 5;
                    HappinessReduction = 10;
                    WeightReduction = 50;
                    break;
                case 2:
                    Cost = 60;
                    Duration = 10;
                    HappinessReduction = 20;
                    WeightReduction = 100;
                    break;
                default:
                    throw new NotImplementedException("level out of range");
            }
        }
    }

    /// <summary>
    /// A player's Beastly Buffet ability.
    /// </summary>
    public class BeastlyBuffet : Ability
    {
        public int PointThreshold { get; private set; }
        public int SpawnWeightMultiplier { get; private set; }
        public int SpawnLimitMultiplier { get; private set; }

        public BeastlyBuffet(int level)
        {
            Level = level;
            switch (level)
            {
                case 0:
                    break;
                case 1:
                    Cost = 50;
                    Duration = 15;
                    PointThreshold = 0;
                    SpawnWeightMultiplier = 10;
                    SpawnLimitMultiplier = 10;
                    break;
                case 2:
                    Cost = 50;
                    Duration = 15;
                    PointThreshold = -10;
                    SpawnWeightMultiplier = 10;
                    SpawnLimitMultiplier = 10;
                    break;
                case 3:
                    Cost = 50;
                    Duration = 15;
                    PointThreshold = -10;
                    SpawnWeightMultiplier = 100;
                    SpawnLimitMultiplier = 100;
                    break;
                default:
                    throw new NotImplementedException("level out of range");
            }
        }
    }

    /// <summary>
    /// A player's Thief ability.
    /// </summary>
    public class Thief : Ability
    {
        public int FoodUnitsTransferred { get; private set; }
        public float SpawnIntervalMultiplier { get; private set; }

        public Thief(int level)
        {
            Level = level;
            switch (level)
            {
                case 0:
                    break;
                case 1:
                    Cost = 70;
                    Duration = 15;
                    FoodUnitsTransferred = 10;
                    SpawnIntervalMultiplier = 1.5f;
                    break;
                case 2:
                    Cost = 70;
                    Duration = 20;
                    FoodUnitsTransferred = 15;
                    SpawnIntervalMultiplier = 2f;
                    break;
                default:
                    throw new NotImplementedException("level out of range");
            }
        }
    }
}
