using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts;
using Random = System.Random;

/// <summary>
/// Script class to encapsulate mouse ability logic.
/// Implements functions to be called when abilities are activated by a player
/// or when a player is affected by an ability activated by another player.
/// </summary>
public class AbilityController : MonoBehaviour
{
    // Other GameObjects
    private Mouse mouse;
    private FoodController foodController;
    private Player player;

    // Simple status flags
    private bool mouseIsImmune;
    private bool mouseIsFearless;
    private bool mouseIsFat;

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

    private Dictionary<string, float> defaultFoodSpawnWeights;

    private Dictionary<string, int> defaultMaxFoodCounts;

    private IDictionary<AbilityName, DateTime> abilityLastActivatedTimes;

    // Use this for initialization
    void Start()
    {
        mouse = GameObject.Find("Mouse").GetComponent<Mouse>();
        foodController = GameObject.Find("FoodController").GetComponent<FoodController>();

        // placeholder for player data
        player = Player.MockPlayer;

        abilityLastActivatedTimes = new Dictionary<AbilityName, DateTime>(7);

        defaultMaxFoodCounts = new Dictionary<string, int>(foodController.MaxFoodCounts);
        defaultFoodSpawnWeights = new Dictionary<string, float>(foodController.FoodSpawnWeights);
    }

    // Update is called once per frame
    void Update()
    {
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
                foodController.setFoodSpawnWeight(beastlyBuffetBoostedFood, defaultFoodSpawnWeights[beastlyBuffetBoostedFood]);
                beastlyBuffetIsActive = false;
            }
        }

        // todo: deactivate thief
    }

    // Function to check if an ability is still active using the duration specified in the player's instance of the ability.
    private bool IsStillActive(AbilityName ability)
    {
        if (abilityLastActivatedTimes.ContainsKey(ability))
        {
            var timeSinceLastActivation = DateTime.Now.Subtract(abilityLastActivatedTimes[ability]).Seconds;
            return timeSinceLastActivation < player.Abilities[ability].Duration;
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

    /// <summary>
    /// Function to be called when a player activates the Immunity ability.
    /// Checks that Immunity is not already activated and the mouse has 
    /// sufficient Happiness and sets the mouse Immunity state.
    /// </summary>
    public void ActivateImmunity()
    {
        if (mouse.Immunity) return;
        if (mouse.Happiness < player.Abilities.Immunity.Cost) return;
        mouseIsImmune = true;
        mouse.Immunity = true;
        abilityLastActivatedTimes[AbilityName.Immunity] = DateTime.Now;
        mouse.Happiness -= player.Abilities.Immunity.Cost;
    }

    /// <summary>
    /// Function to be called when a player activates the Fearless ability.
    /// Checks that Fearless is not already activated and the mouse
    /// has sufficient Happiness and sets the mouse Fearless state.
    /// </summary>
    public void ActivateFearless()
    {
        if (mouse.Fearless) return;
        if (mouse.Happiness < player.Abilities.Fearless.Cost) return;
        mouseIsFearless = true;
        mouse.Fearless = true;
        abilityLastActivatedTimes[AbilityName.Fearless] = DateTime.Now;
        mouse.Happiness -= player.Abilities.Fearless.Cost;
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
        if (mouse.Happiness < player.Abilities.TreatsGalore.Cost) return;
        var goodFoods = foodController.FoodValues.Where(food => food.Value > player.Abilities.TreatsGalore.PointThreshold).ToList();
        var random = new Random();
        var boostedFood = goodFoods[random.Next(goodFoods.Count)].Key;
        foodController.setMaxFoodCount(boostedFood, foodController.getMaxFoodCount(boostedFood)*player.Abilities.TreatsGalore.SpawnLimitMultiplier);
        foodController.setFoodSpawnWeight(boostedFood, foodController.getFoodSpawnWeight(boostedFood)*player.Abilities.TreatsGalore.SpawnWeightMultiplier);
        treatsGaloreBoostedFood = boostedFood;
        treatsGaloreIsActive = true;
        abilityLastActivatedTimes[AbilityName.TreatsGalore] = DateTime.Now;
        mouse.Happiness -= player.Abilities.TreatsGalore.Cost;
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
        if (mouse.Happiness < player.Abilities.FatMouse.Cost) return;
        mouseIsFat = true;
        mouse.GrowthAbility = player.Abilities.FatMouse.WeightMultiplier;
        abilityLastActivatedTimes[AbilityName.FatMouse] = DateTime.Now;
        mouse.Happiness -= player.Abilities.FatMouse.Cost;
    }

    /// <summary>
    /// Function to be called when a player targets another player with
    /// the Scary Cat ability. Checks that the mouse has enough Happiness and 
    /// calls the ReceiveScaryCat function on the targeted player.
    /// </summary>
    public void ActivateScaryCat()
    {
        if (mouse.Happiness < player.Abilities.ScaryCat.Cost) return;
        // todo: networking part
        // call ReceiveScaryCat(player.Abilities.ScaryCat) on target player
        mouse.Happiness -= player.Abilities.ScaryCat.Cost;
    }

    /// <summary>
    /// Function to be called when a player is affected by another player's
    /// Scary Cat ability. May send the player's mouse offscreen and drop its
    /// Weight and Happiness depending on whether the player's Fearless ability
    /// is activated and its level.
    /// </summary>
    /// <param name="scaryCat"></param>
    public void ReceiveScaryCat(ScaryCat scaryCat)
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
                if (player.Abilities.Fearless.DropHappiness && player.Abilities.Fearless.DropWeight)
                {
                    // "A scary cat came over, but your mouse stood its ground. However, its happiness and weight still dropped
                    mouse.Happiness = mouse.Happiness < scaryCat.HappinessReduction ? 0 : mouse.Happiness - scaryCat.HappinessReduction;
                    // todo: @junqi let me change the mouse weight please?
                    // mouse.Weight = mouse.Weight < scaryCat.WeightReduction
                    //  ? 0
                    //  : mouse.Weight - scaryCat.WeightReduction;
                }
                else if (player.Abilities.Fearless.DropHappiness)
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
                // todo: @junqi let me change the mouse weight please?
                // mouse.Weight = mouse.Weight < scaryCat.WeightReduction
                //  ? 0
                //  : mouse.Weight - scaryCat.WeightReduction
                mouse.Offscreen = true;
                mouseIsOffscreen = true;
                abilityLastActivatedTimes[AbilityName.ScaryCat] = DateTime.Now;
                scaryCatDuration = scaryCat.Duration;
            }
        }
    }

    /// <summary>
    /// Function to be called when a player targets another player with the Beastly Buffet ability. 
    /// Checks that the mouse has sufficient Happiness and calls the ReceiveBeastlyBuffet function
    /// on the targeted player.
    /// </summary>
    public void ActivateBeastlyBuffet()
    {
        if (mouse.Happiness < player.Abilities.BeastlyBuffet.Cost) return;
        // todo: networking
        // call ReceiveBeastlyBuffet(player.Abilities.BeastlyBuffet) on target player
        mouse.Happiness -= player.Abilities.BeastlyBuffet.Cost;
    }

    /// <summary>
    /// Function to be called when a player is affected by another player's Beastly Buffet ability.
    /// Chooses a random food below a certain point threshold specified by the attacker's ability
    /// and boosts its max food count and spawn weight through the associated Foodcontroller instance.
    /// </summary>
    /// <param name="beastlyBuffet"></param>
    public void ReceiveBeastlyBuffet(BeastlyBuffet beastlyBuffet)
    {
        var badFoods = foodController.FoodValues.Where(food => food.Value < beastlyBuffet.PointThreshold).ToList();
        var random = new Random();
        var boostedFood = badFoods[random.Next(badFoods.Count)].Key;
        foodController.setMaxFoodCount(boostedFood, foodController.getMaxFoodCount(boostedFood)*player.Abilities.TreatsGalore.SpawnLimitMultiplier);
        foodController.setFoodSpawnWeight(boostedFood, foodController.getFoodSpawnWeight(boostedFood)*player.Abilities.TreatsGalore.SpawnWeightMultiplier);
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
        // todo: not sure how to affect spawn interval and move food yet
    }
}
