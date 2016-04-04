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
    //    private List<GameObject> players;

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

        Debug.LogWarning(String.Format("AbilityController for {0} starting.", gameObject.GetComponent<Player>().Name));

        player = gameObject.GetComponent<Player>();

//        prevLevel = mouse.Level;

        abilityLastActivatedTimes = new Dictionary<AbilityName, DateTime>(7);
    }

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
        defaultMaxFoodCounts = new Dictionary<string, int>(foodController.MaxFoodCounts);
        defaultFoodSpawnWeights = new Dictionary<string, float>(foodController.FoodSpawnWeights);
        if (foodController == null || mouse == null) return;
        Debug.Log(player.Name + " AbilityController ready.");
        mainStarted = true;
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
                // todo: reset spawn rate
                mouseIsThief = false;
            }
        }

        if (mouseIsThiefVictim)
        {
            if (DateTime.Now.Subtract(lastStolenFrom).Seconds > thiefVictimDuration)
            {
                mouseIsThiefVictim = false;
                // todo: reset spawn rate
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
                break;
            case AbilityName.BeastlyBuffet:
                break;
            case AbilityName.Thief:
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
        if (!isLocalPlayer) return;
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
        var random = new Random();
        var boostedFood = goodFoods[random.Next(goodFoods.Count)].Key;
        foodController.setMaxFoodCount(boostedFood, foodController.getMaxFoodCount(boostedFood) * player.PAbilities.TreatsGalore.SpawnLimitMultiplier);
        foodController.setFoodSpawnWeight(boostedFood, foodController.getFoodSpawnWeight(boostedFood) * player.PAbilities.TreatsGalore.SpawnWeightMultiplier);
        treatsGaloreBoostedFood = boostedFood;
        treatsGaloreIsActive = true;
        abilityLastActivatedTimes[AbilityName.TreatsGalore] = DateTime.Now;
        mouse.Happiness -= player.PAbilities.TreatsGalore.Cost;
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
    }

    /// <summary>
    /// Function to be called when a player targets another player with
    /// the Scary Cat ability. Checks that the mouse has enough Happiness and 
    /// calls the RpcReceiveScaryCat function on the targeted player.
    /// </summary>
    public void ActivateScaryCat()
    {
        if (mouse.Happiness < player.PAbilities.ScaryCat.Cost) return;
        // todo: networking part
        // call RpcReceiveScaryCat(player.Abilities.ScaryCat) on target player
        mouse.Happiness -= player.PAbilities.ScaryCat.Cost;
    }

    /// <summary>
    /// Function to be called when a player is affected by another player's
    /// Scary Cat ability. May send the player's mouse offscreen and drop its
    /// Weight and Happiness depending on whether the player's Fearless ability
    /// is activated and its level.
    /// </summary>
    /// <param name="scaryCat"></param>
    [ClientRpc]
    public void RpcReceiveScaryCat(ScaryCat scaryCat)
    {
        // todo: implement UI response

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
                    mouse.Happiness = mouse.Happiness < scaryCat.HappinessReduction ? 0 : mouse.Happiness - scaryCat.HappinessReduction;
                    mouse.Weight = mouse.Weight < scaryCat.WeightReduction
                     ? 0
                     : mouse.Weight - scaryCat.WeightReduction;
                }
                else if (player.PAbilities.Fearless.DropHappiness)
                {
                    // "A scary cat came over, but your mouse bravely stood its ground. However, its happiness still dropped."
                    mouse.Happiness = mouse.Happiness < scaryCat.HappinessReduction ? 0 : mouse.Happiness - scaryCat.HappinessReduction;
                }
                else
                {
                    // "A scary cat came over, but your mouse was completely nonchalant."
                }
            }
            else
            {
                // "A scary cat came over, and your mouse ran away! Its happiness and weight also dropped."
                mouse.Happiness = mouse.Happiness < scaryCat.HappinessReduction ? 0 : mouse.Happiness - scaryCat.HappinessReduction;
                mouse.Weight = mouse.Weight < scaryCat.WeightReduction
                 ? 0
                 : mouse.Weight - scaryCat.WeightReduction;
                mouse.Offscreen = true;
                mouseIsOffscreen = true;
                abilityLastActivatedTimes[AbilityName.ScaryCat] = DateTime.Now;
                scaryCatDuration = scaryCat.Duration;
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
        // todo: networking
        // todo: call RpcReceiveBeastlyBuffet(player.Abilities.BeastlyBuffet) on target player
        mouse.Happiness -= player.PAbilities.BeastlyBuffet.Cost;

    }

    /// <summary>
    /// Function to be called when a player is affected by another player's Beastly Buffet ability.
    /// Chooses a random food below a certain point threshold specified by the attacker's ability
    /// and boosts its max food count and spawn weight through the associated Foodcontroller instance.
    /// </summary>
    /// <param name="beastlyBuffet"></param>
    [ClientRpc]
    public void RpcReceiveBeastlyBuffet(BeastlyBuffet beastlyBuffet)
    {
        var badFoods = foodController.FoodValues.Where(food => food.Value < beastlyBuffet.PointThreshold).ToList();
        var random = new Random();
        var boostedFood = badFoods[random.Next(badFoods.Count)].Key;
        foodController.setMaxFoodCount(boostedFood, foodController.getMaxFoodCount(boostedFood) * player.PAbilities.TreatsGalore.SpawnLimitMultiplier);
        foodController.setFoodSpawnWeight(boostedFood, foodController.getFoodSpawnWeight(boostedFood) * player.PAbilities.TreatsGalore.SpawnWeightMultiplier);
        beastlyBuffetBoostedFood = boostedFood;
        beastlyBuffetIsActive = true;
        abilityLastActivatedTimes[AbilityName.BeastlyBuffet] = DateTime.Now;
        beastlyBuffetDuration = beastlyBuffet.Duration;
    }

    /// <summary>
    /// WORK IN PROGRESS
    /// </summary>
    public void ActivateThief()
    {
        if (mouse.Happiness < player.PAbilities.Thief.Cost) return;
        // todo: networking
        // todo: call RpcReceiveThief(player.Abilities.Thief) on target player
        // todo: increase spawn interval
        mouseIsThief = true;
        abilityLastActivatedTimes[AbilityName.Thief] = DateTime.Now;
        mouse.Happiness -= player.PAbilities.Thief.Cost;
    }

    [ClientRpc]
    public void RpcReceiveThief(Thief thief)
    {
        mouseIsThiefVictim = true;
        lastStolenFrom = DateTime.Now;
        thiefVictimDuration = thief.Duration;

        var foods = foodController.GetComponents<ObjectPool>();
        var goodFoods =
            foods.Where(food => food.PoolObject.GetComponent<Food>().NutritionalValue > 0).ToList();
        var random = new Random();
        var toTransfer = new List<string>();
        var foodsTaken = 0;
        while (foodsTaken < thief.FoodUnitsTransferred)
        {
            if (goodFoods.Count == 0) break;
            var poolToStealFrom = goodFoods[random.Next(goodFoods.Count)];
            if (poolToStealFrom.ActiveObjects > 0)
            {
                toTransfer.Add(poolToStealFrom.GetComponent<Food>().Type);
                poolToStealFrom.transform.GetChild(0).GetComponent<PoolMember>().Deactivate();
                foodsTaken++;
            }
            else goodFoods.Remove(poolToStealFrom);
        }

        // todo: decrease spawn interval

        // todo: call RpcReceiveStolenFood(toTransfer) on player who stole food
    }

    [ClientRpc]
    public void RpcReceiveStolenFood(List<ObjectPool> loot)
    {
        // todo: spawn each food from each string in loot
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
        Debug.LogWarning("CmdIncreaseScore called as local player: " + isLocalPlayer.ToString());
        var players = GameObject.FindGameObjectsWithTag("Player");
        var targetPlayer = players.ElementAt(new Random().Next(players.Length));
        Debug.LogWarning(String.Format("Increasing score for player {0}", targetPlayer.GetComponent<Player>().Name));
        targetPlayer.GetComponent<AbilityController>().RpcIncrementMouseWeight();
    }
}
