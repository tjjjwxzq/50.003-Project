using System;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Linq;
using Random = System.Random;

/// <summary>
/// Script class to encapsulate mouse ability logic.
/// Implements functions to be called when abilities are activated by a player
/// or when a player is affected by an ability activated by another player.
/// Should be attached to a Player object, but only if it is the local player
/// This component is attached to the local player object when the scene is changed to Main
/// </summary>
public class AbilityController : NetworkBehaviour
{
    private bool mainStarted;

    // Other GameObjects
    private Mouse mouse;
    private Player player;
    private FoodController foodController;
    private LevelController levelController;

    // Simple status flags
    private bool mouseIsImmune;
    private bool mouseIsFearless;
    private bool mouseIsFat;

    // Mouse level
    private int prevLevel;
    public int abilityPoints;

    // Treats Galore status
    private bool treatsGaloreIsActive;
    private string treatsGaloreBoostedFood;

    // Scary Cat status
    private bool mouseIsOffscreen;
    private int scaryCatDuration;

    // Beastly Buffet status
    private bool beastlyBuffetIsActive;
    private string beastlyBuffetBoostedFood;
    private int beastlyBuffetDuration;

    // Thief status
    private bool mouseIsThief;
    private bool mouseIsThiefVictim;
    private DateTime lastStolenFrom;
    private int thiefVictimDuration;

    private Dictionary<string, float> defaultFoodSpawnWeights;

    private Dictionary<string, int> defaultMaxFoodCounts;

    private IDictionary<AbilityName, DateTime> abilityLastActivatedTimes;

    void Start()
    {
        mainStarted = false;

        Debug.Log(String.Format("AbilityController for {0} starting.", gameObject.GetComponent<Player>().Name));

        player = gameObject.GetComponent<Player>();

        //        prevLevel = mouse.Level;

        abilityLastActivatedTimes = new Dictionary<AbilityName, DateTime>(7);
    }

    // Why can't this attaching just be done in AbilityController Start?
    public void AttachToMouse()
    {
        Debug.Log(player.Name + " AbilityController component attached to mouse.");
        mouse = GameObject.Find("Mouse").GetComponent<Mouse>();
        if (foodController == null || mouse == null) return;
        Debug.Log(player.Name + " AbilityController ready.");
        mainStarted = true;
    }

    public void AttachToFoodController()
    {
        Debug.Log(player.Name + " AbilityController component attached to food controller.");
        foodController = GameObject.Find("FoodController").GetComponent<FoodController>();
        if (foodController == null || mouse == null) return;
        defaultMaxFoodCounts = new Dictionary<string, int>(foodController.MaxFoodCounts);
        defaultFoodSpawnWeights = new Dictionary<string, float>(foodController.FoodSpawnWeights);
        Debug.Log(player.Name + " AbilityController ready.");
        mainStarted = true;
    }

    /// <summary>
    /// To be called in level controller start???
    /// </summary>
    public void AttachToLevelController()
    {
        Debug.Log(player.Name + " AbilityController component attached to level controller.");
        levelController = GameObject.Find("LevelController").GetComponent<LevelController>();
        if (levelController == null || mouse == null) return;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isLocalPlayer) return;
        if (!mainStarted) return;

        //        if (mouse.Level > prevLevel)
        //        {
        //            abilityPoints += 2;
        //        }
        //        prevLevel = mouse.Level;

        if (mouseIsImmune)
        {
            if (!IsStillActive(AbilityName.Immunity))
            {
                mouseIsImmune = false;
                mouse.Immunity = false;
            }
        }

        if (treatsGaloreIsActive)
        {
            if (!IsStillActive(AbilityName.TreatsGalore))
            {
                foodController.setMaxFoodCount(treatsGaloreBoostedFood, defaultMaxFoodCounts[treatsGaloreBoostedFood]);
                foodController.setFoodSpawnWeight(treatsGaloreBoostedFood, defaultFoodSpawnWeights[treatsGaloreBoostedFood]);
                treatsGaloreIsActive = false;
            }
        }

        if (mouseIsFearless)
        {
            if (!IsStillActive(AbilityName.Fearless))
            {
                mouseIsFearless = false;
                mouse.Fearless = false;
            }
        }

        if (mouseIsFat)
        {
            if (!IsStillActive(AbilityName.FatMouse))
            {
                mouse.GrowthAbility = 1;
                mouseIsFat = false;
            }
        }

        if (mouseIsOffscreen)
        {
            if (!IsStillActive(AbilityName.ScaryCat, scaryCatDuration))
            {
                mouse.Offscreen = false;
                mouseIsOffscreen = false;
            }
        }

        if (beastlyBuffetIsActive)
        {
            if (!IsStillActive(AbilityName.BeastlyBuffet, beastlyBuffetDuration))
            {
                foodController.setMaxFoodCount(beastlyBuffetBoostedFood, defaultMaxFoodCounts[beastlyBuffetBoostedFood]);
                foodController.setFoodSpawnWeight(beastlyBuffetBoostedFood,
                    defaultFoodSpawnWeights[beastlyBuffetBoostedFood]);
                beastlyBuffetIsActive = false;
            }
        }

        if (mouseIsThief)
        {
            if (!IsStillActive(AbilityName.Thief))
            {
                foodController.SpawnRate = 1f;
                mouseIsThief = false;
            }
        }

        if (mouseIsThiefVictim)
        {
            if (DateTime.Now.Subtract(lastStolenFrom).Seconds > thiefVictimDuration)
            {
                foodController.SpawnRate = 1f;
                mouseIsThiefVictim = false;
            }
        }
    }

    // Function to check if an ability is still active using the duration specified in the player's instance of the ability.
    private bool IsStillActive(AbilityName ability)
    {
        if (abilityLastActivatedTimes.ContainsKey(ability))
        {
            var timeSinceLastActivation = DateTime.Now.Subtract(abilityLastActivatedTimes[ability]).Seconds;
            return timeSinceLastActivation < player.PAbilities[ability].Duration;
        }
        else return false;
    }

    // Function to check if an ability is still active using the specified duration.
    private bool IsStillActive(AbilityName ability, int duration)
    {
        if (abilityLastActivatedTimes.ContainsKey(ability))
        {
            var timeSinceLastActivation = DateTime.Now.Subtract(abilityLastActivatedTimes[ability]).Seconds;
            return timeSinceLastActivation < duration;
        }
        else return false;
    }

    /// <summary>
    /// To be called by the Player script
    /// to activate the ability based on 
    /// the tapped icon
    /// </summary>
    /// <param name="ability"></param>
    public void ActivateAbility(AbilityName ability)
    {
        switch (ability)
        {
            case AbilityName.Immunity:
                ActivateImmunity();
                break;
            case AbilityName.TreatsGalore:
                ActivateTreatsGalore();
                break;
            case AbilityName.Fearless:
                ActivateFearless();
                break;
            case AbilityName.FatMouse:
                ActivateFatMouse();
                break;
            case AbilityName.ScaryCat:
                ActivateScaryCat();
                break;
            case AbilityName.BeastlyBuffet:
                ActivateBeastlyBuffet();
                break;
            case AbilityName.Thief:
                ActivateThief();
                break;
            default:
                throw new ArgumentOutOfRangeException("ability", ability, null);
        }
    }

    public void ImproveAbility(AbilityName ability)
    {
        if (player.PAbilities[ability].Level >= player.PAbilities[ability].MaxLevel) return;
        player.PAbilities.SetAbility(ability, player.PAbilities[ability].Level + 1);
        abilityPoints--;
    }

    /// <summary>
    /// Function to be called when a player activates the Immunity ability.
    /// Checks that Immunity is not already activated and the mouse has 
    /// sufficient Happiness and sets the mouse Immunity state.
    /// </summary>
    public void ActivateImmunity()
    {
        CmdIncreaseScore();

        //        if (mouse.Immunity) return;
        //        if (mouse.Happiness < player.PAbilities.Immunity.Cost) return;
        //        mouseIsImmune = true;
        //        mouse.Immunity = true;
        //        abilityLastActivatedTimes[AbilityName.Immunity] = DateTime.Now;
        //        mouse.Happiness -= player.PAbilities.Immunity.Cost;
    }

    /// <summary>
    /// Function to be called when a player activates the Fearless ability.
    /// Checks that Fearless is not already activated and the mouse
    /// has sufficient Happiness and sets the mouse Fearless state.
    /// </summary>
    public void ActivateFearless()
    {
        if (mouse.Fearless) return;
        if (mouse.Happiness < player.PAbilities.Fearless.Cost) return;
        mouseIsFearless = true;
        mouse.Fearless = true;
        abilityLastActivatedTimes[AbilityName.Fearless] = DateTime.Now;
        mouse.Happiness -= player.PAbilities.Fearless.Cost;
    }

    /// <summary>
    /// Function to be called when the player activates the Treats Galore ability.
    /// Checks that Treats Galore is not already activated and the mouse has
    /// sufficient Happiness. Chooses a random food above a certain point threshold
    /// specified in the ability level and boosts its max food count and spawn weight 
    /// through the associated FoodController instance.
    /// </summary>
    public void ActivateTreatsGalore()
    {
        Debug.LogWarning("Activating treats galore.");
        if (mouse.Happiness < player.PAbilities.TreatsGalore.Cost) return;
        var goodFoods = foodController.FoodValues.Where(food => food.Value > player.PAbilities.TreatsGalore.PointThreshold).ToList();
        var boostedFood = goodFoods[new Random().Next(goodFoods.Count)].Key;
        foodController.setMaxFoodCount(boostedFood, foodController.getMaxFoodCount(boostedFood) * player.PAbilities.TreatsGalore.SpawnLimitMultiplier);
        foodController.setFoodSpawnWeight(boostedFood, foodController.getFoodSpawnWeight(boostedFood) * player.PAbilities.TreatsGalore.SpawnWeightMultiplier);
        treatsGaloreBoostedFood = boostedFood;
        treatsGaloreIsActive = true;
        abilityLastActivatedTimes[AbilityName.TreatsGalore] = DateTime.Now;
        mouse.Happiness -= player.PAbilities.TreatsGalore.Cost;

        Debug.Log(string.Format("{0} activated Treats Galore and boosted {1}", player.Name, boostedFood));
    }

    /// <summary>
    /// Function to be called when the player activates the Fat Mouse ability.
    /// Checks that Fat Mouse is not already activated and the mouse has 
    /// sufficient Happiness. Sets the mouse GrowthAbility to the multiplier
    /// specified by the ability level.
    /// </summary>
    public void ActivateFatMouse()
    {
        if (mouseIsFat) return;
        if (mouse.Happiness < player.PAbilities.FatMouse.Cost) return;
        mouseIsFat = true;
        mouse.GrowthAbility = player.PAbilities.FatMouse.WeightMultiplier;
        abilityLastActivatedTimes[AbilityName.FatMouse] = DateTime.Now;
        mouse.Happiness -= player.PAbilities.FatMouse.Cost;

        Debug.Log(string.Format("{0} activated Fat Mouse. Weight gain multiplier: {1}", player.Name, mouse.GrowthAbility));
    }

    /// <summary>
    /// Function to be called when a player targets another player with
    /// the Scary Cat ability. Checks that the mouse has enough Happiness and 
    /// calls the RpcReceiveScaryCat function on the targeted player.
    /// </summary>
    public void ActivateScaryCat()
    {
        if (mouse.Happiness < player.PAbilities.ScaryCat.Cost) return;
        CmdDispatchScaryCat(player.Name, player.PAbilities.ScaryCat.Duration, player.PAbilities.ScaryCat.HappinessReduction, player.PAbilities.ScaryCat.WeightReduction);
        mouse.Happiness -= player.PAbilities.ScaryCat.Cost;
    }

    [Command]
    public void CmdDispatchScaryCat(string caller, int duration, int happinessReduction, int weightReduction)
    {
        if (!isServer) return;
        // todo: implement choice of target player rather than random
        var players = GameObject.FindGameObjectsWithTag("Player").Where(o => !o.GetComponent<Player>().Name.Equals(caller)).ToArray();
        var victim = players.ElementAt(new Random().Next(players.Length));
        victim.GetComponent<AbilityController>().RpcReceiveScaryCat(duration, happinessReduction, weightReduction);
    }

    /// <summary>
    /// Function to be called when a player is affected by another player's
    /// Scary Cat ability. May send the player's mouse offscreen and drop its
    /// Weight and Happiness depending on whether the player's Fearless ability
    /// is activated and its level.
    /// </summary>
    /// <param name="duration"></param>
    /// <param name="happinessReduction"></param>
    /// <param name="weightReduction"></param>
    [ClientRpc]
    public void RpcReceiveScaryCat(int duration, int happinessReduction, int weightReduction)
    {
        if (!isLocalPlayer) return;
        // todo: implement UI response
        levelController.ScaryCatAnimation();

        if (mouseIsOffscreen)
        {
            // "A scary cat came over, but your mouse wasn't around!"
        }
        else
        {
            if (mouseIsFearless)
            {
                if (player.PAbilities.Fearless.DropHappiness && player.PAbilities.Fearless.DropWeight)
                {
                    // "A scary cat came over, but your mouse stood its ground. However, its happiness and weight still dropped
                    mouse.Happiness -= happinessReduction;
                    mouse.Weight -= weightReduction;
                }
                else if (player.PAbilities.Fearless.DropHappiness)
                {
                    // "A scary cat came over, but your mouse bravely stood its ground. However, its happiness still dropped."
                    mouse.Happiness -= happinessReduction;
                }
                else
                {
                    // "A scary cat came over, but your mouse was completely nonchalant."
                }
            }
            else
            {
                // "A scary cat came over, and your mouse ran away! Its happiness and weight also dropped."
                mouse.Happiness -= happinessReduction;
                mouse.Weight -= weightReduction;
                mouse.Offscreen = true;
                mouseIsOffscreen = true;
                abilityLastActivatedTimes[AbilityName.ScaryCat] = DateTime.Now;
                scaryCatDuration = duration;
            }
        }
    }

    /// <summary>
    /// Function to be called when a player targets another player with the Beastly Buffet ability. 
    /// Checks that the mouse has sufficient Happiness and calls the RpcReceiveBeastlyBuffet function
    /// on the targeted player.
    /// </summary>
    public void ActivateBeastlyBuffet()
    {
        if (mouse.Happiness < player.PAbilities.BeastlyBuffet.Cost) return;
        CmdDispatchBeastlyBuffet(player.Name, player.PAbilities.BeastlyBuffet.Duration, player.PAbilities.BeastlyBuffet.PointThreshold, player.PAbilities.BeastlyBuffet.SpawnLimitMultiplier, player.PAbilities.BeastlyBuffet.SpawnWeightMultiplier);
        mouse.Happiness -= player.PAbilities.BeastlyBuffet.Cost;

    }

    [Command]
    public void CmdDispatchBeastlyBuffet(string caller, int duration, int pointThreshold, int spawnLimitMultiplier, int spawnWeightMultiplier)
    {
        if (!isServer) return;
        // todo: implement choice of target player rather than random
        var players = GameObject.FindGameObjectsWithTag("Player").Where(o => !o.GetComponent<Player>().Name.Equals(caller)).ToArray();
        var victim = players.ElementAt(new Random().Next(players.Length));
        victim.GetComponent<AbilityController>().RpcReceiveBeastlyBuffet(duration, pointThreshold, spawnLimitMultiplier, spawnWeightMultiplier);
    }

    /// <summary>
    /// Function to be called when a player is affected by another player's Beastly Buffet ability.
    /// Chooses a random food below a certain point threshold specified by the attacker's ability
    /// and boosts its max food count and spawn weight through the associated Foodcontroller instance.
    /// </summary>
    /// <param name="duration"></param>
    /// <param name="pointThreshold"></param>
    /// <param name="spawnLimitMultiplier"></param>
    /// <param name="spawnWeightMultiplier"></param>
    [ClientRpc]
    public void RpcReceiveBeastlyBuffet(int duration, int pointThreshold, int spawnLimitMultiplier, int spawnWeightMultiplier)
    {
        if (!isLocalPlayer) return;
        var badFoods = foodController.FoodValues.Where(food => food.Value < pointThreshold).ToList();
        var boostedFood = badFoods[new Random().Next(badFoods.Count)].Key;
        foodController.setMaxFoodCount(boostedFood, foodController.getMaxFoodCount(boostedFood) * spawnLimitMultiplier);
        foodController.setFoodSpawnWeight(boostedFood, foodController.getFoodSpawnWeight(boostedFood) * spawnWeightMultiplier);
        beastlyBuffetBoostedFood = boostedFood;
        beastlyBuffetIsActive = true;
        abilityLastActivatedTimes[AbilityName.BeastlyBuffet] = DateTime.Now;
        beastlyBuffetDuration = duration;
    }

    /// <summary>
    /// WORK IN PROGRESS
    /// </summary>
    public void ActivateThief()
    {
        if (mouse.Happiness < player.PAbilities.Thief.Cost) return;
        CmdDispatchThief(player.Name, player.PAbilities.Thief.Duration, player.PAbilities.Thief.FoodUnitsTransferred);
        //        mouseIsThief = true;
        //        abilityLastActivatedTimes[AbilityName.Thief] = DateTime.Now;
        //        mouse.Happiness -= player.PAbilities.Thief.Cost;
    }

    [Command]
    public void CmdDispatchThief(string caller, int duration, int foodUnitsTransferred)
    {
        if (!isServer) return;
        // todo: implement choice of target player rather than random
        var players = GameObject.FindGameObjectsWithTag("Player").Where(o => !o.GetComponent<Player>().Name.Equals(caller)).ToArray();
        var victim = players.ElementAt(new Random().Next(players.Length));
        Debug.Log("Possible targets: " + string.Join(" ", players.Select(o => o.GetComponent<Player>().Name).ToArray()) + ", chosen victim: " + victim.GetComponent<Player>().Name);
        victim.GetComponent<AbilityController>().RpcReceiveThief(caller, duration, foodUnitsTransferred);
    }

    [ClientRpc]
    public void RpcReceiveThief(string caller, int duration, int foodUnitsTransferred)
    {
        if (!isLocalPlayer) return;
        if (foodController == null)
        {
            Debug.LogError("foodController is null");
            return;
        } 
        var toTransfer = new List<string>();

        for (int i = 0; i < foodUnitsTransferred; i++)
        {
            try
            {
                var pieceOfFoodToBeStolen = foodController.GetComponentsInChildren<PoolMember>()
                    .First(o => o.gameObject.activeSelf && o.gameObject.GetComponent<Food>().NutritionalValue > 0);
                toTransfer.Add(pieceOfFoodToBeStolen.GetComponent<Food>().Type);
                pieceOfFoodToBeStolen.Deactivate();
            }
            catch (InvalidOperationException)
            {
                break;
            }
        }

        //        var foods = foodController.GetComponents<ObjectPool>();
        //        Debug.LogWarning("Number of object pools: " + foods.Length);
        //        var goodFoods =
        //            foods.Where(food => food.PoolObject.GetComponent<Food>().NutritionalValue > 0).ToList();
        //        var random = new Random();
        //        var foodsTaken = 0;
        //        while (foodsTaken < foodUnitsTransferred)
        //        {
        //            Debug.Log("Number of foods stolen: " + foodsTaken);
        //            if (goodFoods.Count == 0) break;
        //            var poolToStealFrom = goodFoods[random.Next(goodFoods.Count)];
        //            if (poolToStealFrom.ActiveObjects > 0)
        //            {
        //                toTransfer.Add(poolToStealFrom.PoolObject.GetComponent<Food>().Type);
        //                poolToStealFrom.transform.GetChild(0).GetComponent<PoolMember>().Deactivate();
        //                foodsTaken++;
        //                Debug.Log("Stole a " + poolToStealFrom.GetComponent<Food>().Type);
        //            }
        //            else goodFoods.Remove(poolToStealFrom);
        //        }

        //        mouseIsThiefVictim = true;
        //        lastStolenFrom = DateTime.Now;
        //        thiefVictimDuration = duration;

        foodController.SpawnRate = 0.5f;

        CmdDispatchStolenFood(caller, toTransfer.ToArray());
    }

    [Command]
    public void CmdDispatchStolenFood(string caller, string[] foodsTaken)
    {
        if (!isServer) return;
        Debug.Log(caller + " stole " + string.Join(" ", foodsTaken));
        var thief = GameObject.FindGameObjectsWithTag("Player").First(o => o.GetComponent<Player>().Name.Equals(caller));
        thief.GetComponent<AbilityController>().RpcReceiveStolenFood(foodsTaken);
    }

    [ClientRpc]
    public void RpcReceiveStolenFood(string[] foodsTaken)
    {
        if (!isLocalPlayer) return;
        foreach (var food in foodsTaken)
        {
            foodController.SpawnFood(food);
        }
        foodController.SpawnRate = 2f;
    }

    [ClientRpc]
    public void RpcIncrementMouseWeight()
    {
        if (!isLocalPlayer) return;
        mouse.Weight++;
    }

    [Command]
    public void CmdIncreaseScore()
    {
        if (!isServer) return;
        Debug.LogWarning("CmdIncreaseScore called as local player: " + isLocalPlayer.ToString());
        var players = GameObject.FindGameObjectsWithTag("Player");
        var targetPlayer = players.ElementAt(new Random().Next(players.Length));
        Debug.LogWarning(string.Format("Increasing score for player {0}", targetPlayer.GetComponent<Player>().Name));
        targetPlayer.GetComponent<AbilityController>().RpcIncrementMouseWeight();
    }
}
