using System;

namespace Assets.Scripts
{
    public class Abilities
    {
        public Immunity Immunity;
        public TreatsGalore TreatsGalore;
        public Fearless Fearless;
        public FatMouse FatMouse;
        public ScaryCat ScaryCat;
        public BeastlyBuffet BeastlyBuffet;
        public Thief Thief;

        public Ability this[AbilityName ability]
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
                        throw new ArgumentOutOfRangeException("ability", ability, null);
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

        public static Abilities StartingAbilities
        {
            get { return new Abilities(1, 1, 1, 1, 1, 1, 1); }
        }
    }

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

    public abstract class Ability
    {
        public int Cost { get; protected set; }
        public int Duration { get; protected set; }
    }

    public class Immunity : Ability
    {
        public Immunity(int level)
        {
            switch (level)
            {
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

    public class TreatsGalore : Ability
    {
        public int PointThreshold { get; private set; }
        public int SpawnWeightMultiplier { get; private set; }
        public int SpawnLimitMultiplier { get; private set; }

        public TreatsGalore(int level)
        {
            switch (level)
            {
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

    public class Fearless : Ability
    {
        public bool DropHappiness { get; private set; }
        public bool DropWeight { get; private set; }

        public Fearless(int level)
        {
            switch (level)
            {
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

    public class FatMouse : Ability
    {
        public int WeightMultiplier { get; private set; }

        public FatMouse(int level)
        {
            switch (level)
            {
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

    public class ScaryCat : Ability
    {
        public int HappinessReduction { get; private set; }
        public int WeightReduction { get; private set; }

        public ScaryCat(int level)
        {
            switch (level)
            {
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

    public class BeastlyBuffet : Ability
    {
        public int PointThreshold { get; private set; }
        public int SpawnWeightMultiplier { get; private set; }
        public int SpawnLimitMultiplier { get; private set; }

        public BeastlyBuffet(int level)
        {
            switch (level)
            {
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

    public class Thief : Ability
    {
        public int FoodUnitsTransferred { get; private set; }
        public float SpawnIntervalMultiplier { get; private set; }

        public Thief(int level)
        {
            switch (level)
            {
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
