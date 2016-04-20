using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts;
using UnityEngine.Assertions;
using Random = System.Random;

public class AbilityController : MonoBehaviour
{
    private Mouse mouse;
    private FoodController foodController;
    private Player player;

    private bool treatsGaloreIsActive;
    private string treatsGaloreBoostedFood;

    private int scaryCatDuration;

    private bool beastlyBuffetIsActive;
    private string beastlyBuffetBoostedFood;
    private int beastlyBuffetDuration;

    private IDictionary<AbilityName, DateTime> abilityLastActivatedTimes;

    // Use this for initialization
    void Start()
    {
        mouse = GameObject.Find("Mouse").GetComponent<Mouse>();
        foodController = GameObject.Find("FoodController").GetComponent<FoodController>();

        // placeholder for player data
        player = Player.MockPlayer;

        abilityLastActivatedTimes = new Dictionary<AbilityName, DateTime>(7);
    }

    // Update is called once per frame
    void Update()
    {
        if (mouse.Immunity)
            if (!IsStillActive(AbilityName.Immunity))
                mouse.Immunity = false;

        if (treatsGaloreIsActive)
        {
            if (!IsStillActive(AbilityName.TreatsGalore))
            {
                foodController.setMaxFoodCount(treatsGaloreBoostedFood, defaultMaxFoodCounts[treatsGaloreBoostedFood]);
                foodController.setFoodSpawnWeight(treatsGaloreBoostedFood, defaultFoodSpawnWeights[treatsGaloreBoostedFood]);
                treatsGaloreIsActive = false;
            }
        }

        if (mouse.Fearless)
            if (!IsStillActive(AbilityName.Fearless))
                mouse.Fearless = false;

        // todo: mouse needs Fat attribute
//        if (mouse.Fat)
//        {
//            if (!IsStillActive(AbilityName.FatMouse))
//            {
//                mouse.GrowthAbility = 1;
//                // mouse.Fat = false;
//            }
//        }

        if (mouse.Offscreen)
            if (!IsStillActive(AbilityName.ScaryCat, scaryCatDuration))
                mouse.Offscreen = false;

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

    private bool IsStillActive(AbilityName ability)
    {
        if (abilityLastActivatedTimes.ContainsKey(ability))
        {
            var timeSinceLastActivation = DateTime.Now.Subtract(abilityLastActivatedTimes[ability]).Seconds;
            return timeSinceLastActivation < player.abilities[ability].Duration;
        }
        else return false;
    }

    private bool IsStillActive(AbilityName ability, int duration)
    {
        if (abilityLastActivatedTimes.ContainsKey(ability))
        {
            var timeSinceLastActivation = DateTime.Now.Subtract(abilityLastActivatedTimes[ability]).Seconds;
            return timeSinceLastActivation < duration;
        }
        else return false;
    }

    public void ActivateImmunity()
    {
        if (mouse.Immunity) return;

        if (mouse.Happiness < player.Abilities.Immunity.Cost) return;

        if (mouse.Happiness < player.abilities.Immunity.Cost) return;
        mouseIsImmune = true;

        mouse.Immunity = true;
        abilityLastActivatedTimes[AbilityName.Immunity] = DateTime.Now;
        mouse.Happiness -= player.abilities.Immunity.Cost;
    }

    public void ActivateFearless()
    {
        if (mouse.Fearless) return;

        if (mouse.Happiness < player.Abilities.Fearless.Cost) return;

        if (mouse.Happiness < player.abilities.Fearless.Cost) return;
        mouseIsFearless = true;

        mouse.Fearless = true;
        abilityLastActivatedTimes[AbilityName.Fearless] = DateTime.Now;
        mouse.Happiness -= player.abilities.Fearless.Cost;
    }

    public void ActivateTreatsGalore()
    {

        if (mouse.Happiness < player.Abilities.TreatsGalore.Cost) return;
        var goodFoods = foodPointValues.Where(food => food.Value > player.Abilities.TreatsGalore.PointThreshold).ToList();
        var random = new Random();
        var boostedFood = goodFoods[random.Next(goodFoods.Count)].Key;
        foodController.setMaxFoodCount(boostedFood, foodController.getMaxFoodCount(boostedFood) * player.Abilities.TreatsGalore.SpawnLimitMultiplier);
        foodController.setFoodSpawnWeight(boostedFood, foodController.getFoodSpawnWeight(boostedFood) * player.Abilities.TreatsGalore.SpawnWeightMultiplier);

        if (mouse.Happiness < player.abilities.TreatsGalore.Cost) return;
        var goodFoods = foodController.FoodValues.Where(food => food.Value > player.abilities.TreatsGalore.PointThreshold).ToList();
        var random = new Random();
        var boostedFood = goodFoods[random.Next(goodFoods.Count)].Key;
        foodController.setMaxFoodCount(boostedFood, foodController.getMaxFoodCount(boostedFood)*player.abilities.TreatsGalore.SpawnLimitMultiplier);
        foodController.setFoodSpawnWeight(boostedFood, foodController.getFoodSpawnWeight(boostedFood)*player.abilities.TreatsGalore.SpawnWeightMultiplier);

        treatsGaloreBoostedFood = boostedFood;
        treatsGaloreIsActive = true;
        abilityLastActivatedTimes[AbilityName.TreatsGalore] = DateTime.Now;
        mouse.Happiness -= player.abilities.TreatsGalore.Cost;
    }

    public void ActivateFatMouse()
    {

        // todo: @junqi mouse needs Fat state
        // if (mouse.Fat) return;
        if (mouse.Happiness < player.Abilities.FatMouse.Cost) return;
        // mouse.Fat = true;
        mouse.GrowthAbility = player.Abilities.FatMouse.WeightMultiplier;

        if (mouseIsFat) return;
        if (mouse.Happiness < player.abilities.FatMouse.Cost) return;
        mouseIsFat = true;
        mouse.GrowthAbility = player.abilities.FatMouse.WeightMultiplier;

        abilityLastActivatedTimes[AbilityName.FatMouse] = DateTime.Now;
        mouse.Happiness -= player.abilities.FatMouse.Cost;
    }

    public void ActivateScaryCat()
    {
        if (mouse.Happiness < player.abilities.ScaryCat.Cost) return;
        // todo: networking part
        // call ReceiveScaryCat(player.Abilities.ScaryCat) on target player
        mouse.Happiness -= player.abilities.ScaryCat.Cost;
    }

    public void ReceiveScaryCat(ScaryCat scaryCat)
    {
        if (mouse.Offscreen)
        {
            // "A scary cat came over, but your mouse wasn't around!"
        }
        else
        {
            if (mouse.Fearless)
            {
                if (player.abilities.Fearless.DropHappiness && player.abilities.Fearless.DropWeight)
                {
                    // "A scary cat came over, but your mouse stood its ground. However, its happiness and weight still dropped
                    mouse.Happiness = mouse.Happiness < scaryCat.HappinessReduction
                        ? 0
                        : mouse.Happiness - scaryCat.HappinessReduction;
                    // todo: @junqi let me change the mouse weight please?
                    // mouse.Weight = mouse.Weight < scaryCat.WeightReduction
                    //  ? 0
                    //  : mouse.Weight - scaryCat.WeightReduction;
                }
                else if (player.abilities.Fearless.DropHappiness)
                {
                    // "A scary cat came over, but your mouse bravely stood its ground. However, its happiness still dropped."
                    mouse.Happiness = mouse.Happiness < scaryCat.HappinessReduction
                        ? 0
                        : mouse.Happiness - scaryCat.HappinessReduction;
                }
                else
                {
                    // "A scary cat came over, but your mouse was completely nonchalant."
                }
            }
            else
            {
                // "A scary cat came over, and your mouse ran away! Its happiness and weight also dropped."
                mouse.Happiness = mouse.Happiness < scaryCat.HappinessReduction
                ? 0
                : mouse.Happiness - scaryCat.HappinessReduction;
                // todo: @junqi let me change the mouse weight please?
                // mouse.Weight = mouse.Weight < scaryCat.WeightReduction
                //  ? 0
                //  : mouse.Weight - scaryCat.WeightReduction;
                mouse.Offscreen = true;
                abilityLastActivatedTimes[AbilityName.ScaryCat] = DateTime.Now;
                scaryCatDuration = scaryCat.Duration;
            }
        }
    }

    public void ActivateBeastlyBuffet()
    {
        if (mouse.Happiness < player.abilities.BeastlyBuffet.Cost) return;
        // todo: networking
        // call ReceiveBeastlyBuffet(player.Abilities.BeastlyBuffet) on target player
        mouse.Happiness -= player.abilities.BeastlyBuffet.Cost;
    }

    public void ReceiveBeastlyBuffet(BeastlyBuffet beastlyBuffet)
    {
        var badFoods = foodPointValues.Where(food => food.Value < beastlyBuffet.PointThreshold).ToList();
        var random = new Random();
        var boostedFood = badFoods[random.Next(badFoods.Count)].Key;

        foodController.setMaxFoodCount(boostedFood, foodController.getMaxFoodCount(boostedFood) * player.Abilities.TreatsGalore.SpawnLimitMultiplier);
        foodController.setFoodSpawnWeight(boostedFood, foodController.getFoodSpawnWeight(boostedFood) * player.Abilities.TreatsGalore.SpawnWeightMultiplier);

        foodController.setMaxFoodCount(boostedFood, foodController.getMaxFoodCount(boostedFood)*player.abilities.TreatsGalore.SpawnLimitMultiplier);
        foodController.setFoodSpawnWeight(boostedFood, foodController.getFoodSpawnWeight(boostedFood)*player.abilities.TreatsGalore.SpawnWeightMultiplier);

        beastlyBuffetBoostedFood = boostedFood;
        beastlyBuffetIsActive = true;
        abilityLastActivatedTimes[AbilityName.BeastlyBuffet] = DateTime.Now;
        beastlyBuffetDuration = beastlyBuffet.Duration;
    }

    public void ActivateThief()
    {
        // todo: not sure how to affect spawn interval and move food yet
    }

    // I think these could be somewhere else.
    private readonly Dictionary<string, int> foodPointValues = new Dictionary<string, int>()
    {
        {"Normal", 5 },
        {"Cheese", 10 },
        {"Carrot", 7 },
        {"Oat", 15 },
        {"Apple", 8 },
        {"Anchovy", 12 },
        {"Bread", 18 },
        {"Seed", 20 },

        {"Bad", -5 },
        {"Peanut", -7 },
        {"Orange", -10 },
        {"Garlic", -15 },
        {"Chocolate", -20 },
        {"Poison", -50 }
    };

    private readonly Dictionary<string, float> defaultFoodSpawnWeights = new Dictionary<string, float>
    {
         // Good foods
        {"Normal" , 6f},
        {"Cheese" , 2.5f },
        {"Carrot" , 3.5f},
        {"Oat" , 2f },
        {"Apple" , 3f },
        {"Anchovy" , 1.5f },
        {"Bread" , 1f },
        {"Seed" , 0.8f },

        // Bad foods
        {"Bad", 4f },
        {"Peanut", 2f },
        {"Orange", 1.5f },
        {"Garlic", 1.2f },
        {"Chocolate", 0.8f },
        {"Poison", 0.2f }
    };

    private readonly Dictionary<string, int> defaultMaxFoodCounts = new Dictionary<string, int>
    {
        // Good foods
        {"Normal" , 15 },
        {"Cheese" , 8 },
        {"Carrot" , 10 },
        {"Oat" , 5 },
        {"Apple" , 9 },
        {"Anchovy" , 3 },
        {"Bread" , 3 },
        {"Seed" , 2 },

        // Bad foods
        {"Bad", 10 },
        {"Peanut", 7 },
        {"Orange", 5 },
        {"Garlic", 3 },
        {"Chocolate", 2 },
        {"Poison", 1 }
    };
}
