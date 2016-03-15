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
    private Mouse _mouse;
    private FoodController _foodController;
    private Player _player;

    private bool _treatsGaloreIsActive;
    private string _treatsGaloreBoostedFood;

    private int _scaryCatDuration;

    private bool _beastlyBuffetIsActive;
    private string _beastlyBuffetBoostedFood;
    private int _beastlyBuffetDuration;

    private IDictionary<AbilityName, DateTime> _abilityLastActivatedTimes;

    // Use this for initialization
    void Start()
    {
        _mouse = GameObject.Find("Mouse").GetComponent<Mouse>();
        _foodController = GameObject.Find("FoodController").GetComponent<FoodController>();

        // placeholder for player data
        _player = Player.MockPlayer;

        _abilityLastActivatedTimes = new Dictionary<AbilityName, DateTime>(7);
    }

    // Update is called once per frame
    void Update()
    {
        if (_mouse.Immunity)
            if (!IsStillActive(AbilityName.Immunity))
                _mouse.Immunity = false;

        if (_treatsGaloreIsActive)
        {
            if (!IsStillActive(AbilityName.TreatsGalore))
            {
                _foodController.setMaxFoodCount(_treatsGaloreBoostedFood, _defaultMaxFoodCounts[_treatsGaloreBoostedFood]);
                _foodController.setFoodSpawnWeight(_treatsGaloreBoostedFood, _defaultFoodSpawnWeights[_treatsGaloreBoostedFood]);
                _treatsGaloreIsActive = false;
            }
        }

        if (_mouse.Fearless)
            if (!IsStillActive(AbilityName.Fearless))
                _mouse.Fearless = false;

        // todo: mouse needs Fat attribute
//        if (_mouse.Fat)
//        {
//            if (!IsStillActive(AbilityName.FatMouse))
//            {
//                _mouse.GrowthAbility = 1;
//                // _mouse.Fat = false;
//            }
//        }

        if (_mouse.Offscreen)
            if (!IsStillActive(AbilityName.ScaryCat, _scaryCatDuration))
                _mouse.Offscreen = false;

        if (_beastlyBuffetIsActive)
        {
            if (!IsStillActive(AbilityName.BeastlyBuffet, _beastlyBuffetDuration))
            {
                _foodController.setMaxFoodCount(_beastlyBuffetBoostedFood, _defaultMaxFoodCounts[_beastlyBuffetBoostedFood]);
                _foodController.setFoodSpawnWeight(_beastlyBuffetBoostedFood, _defaultFoodSpawnWeights[_beastlyBuffetBoostedFood]);
                _beastlyBuffetIsActive = false;
            }
        }

        // todo: deactivate thief
    }

    private bool IsStillActive(AbilityName ability)
    {
        if (_abilityLastActivatedTimes.ContainsKey(ability))
        {
            var timeSinceLastActivation = DateTime.Now.Subtract(_abilityLastActivatedTimes[ability]).Seconds;
            return timeSinceLastActivation < _player.Abilities[ability].Duration;
        }
        else return false;
    }

    private bool IsStillActive(AbilityName ability, int duration)
    {
        if (_abilityLastActivatedTimes.ContainsKey(ability))
        {
            var timeSinceLastActivation = DateTime.Now.Subtract(_abilityLastActivatedTimes[ability]).Seconds;
            return timeSinceLastActivation < duration;
        }
        else return false;
    }

    public void ActivateImmunity()
    {
        if (_mouse.Immunity) return;
        if (_mouse.Happiness < _player.Abilities.Immunity.Cost) return;
        _mouse.Immunity = true;
        _abilityLastActivatedTimes[AbilityName.Immunity] = DateTime.Now;
        _mouse.Happiness -= _player.Abilities.Immunity.Cost;
    }

    public void ActivateFearless()
    {
        if (_mouse.Fearless) return;
        if (_mouse.Happiness < _player.Abilities.Fearless.Cost) return;
        _mouse.Fearless = true;
        _abilityLastActivatedTimes[AbilityName.Fearless] = DateTime.Now;
        _mouse.Happiness -= _player.Abilities.Fearless.Cost;
    }

    public void ActivateTreatsGalore()
    {
        if (_mouse.Happiness < _player.Abilities.TreatsGalore.Cost) return;
        var goodFoods = _foodPointValues.Where(food => food.Value > _player.Abilities.TreatsGalore.PointThreshold).ToList();
        var random = new Random();
        var boostedFood = goodFoods[random.Next(goodFoods.Count)].Key;
        _foodController.setMaxFoodCount(boostedFood, _foodController.getMaxFoodCount(boostedFood) * _player.Abilities.TreatsGalore.SpawnLimitMultiplier);
        _foodController.setFoodSpawnWeight(boostedFood, _foodController.getFoodSpawnWeight(boostedFood) * _player.Abilities.TreatsGalore.SpawnWeightMultiplier);
        _treatsGaloreBoostedFood = boostedFood;
        _treatsGaloreIsActive = true;
        _abilityLastActivatedTimes[AbilityName.TreatsGalore] = DateTime.Now;
        _mouse.Happiness -= _player.Abilities.TreatsGalore.Cost;
    }

    public void ActivateFatMouse()
    {
        // todo: @junqi mouse needs Fat state
        // if (_mouse.Fat) return;
        if (_mouse.Happiness < _player.Abilities.FatMouse.Cost) return;
        // _mouse.Fat = true;
        _mouse.GrowthAbility = _player.Abilities.FatMouse.WeightMultiplier;
        _abilityLastActivatedTimes[AbilityName.FatMouse] = DateTime.Now;
        _mouse.Happiness -= _player.Abilities.FatMouse.Cost;
    }

    public void ActivateScaryCat()
    {
        if (_mouse.Happiness < _player.Abilities.ScaryCat.Cost) return;
        // todo: networking part
        // call ReceiveScaryCat(_player.Abilities.ScaryCat) on target player
        _mouse.Happiness -= _player.Abilities.ScaryCat.Cost;
    }

    public void ReceiveScaryCat(ScaryCat scaryCat)
    {
        if (_mouse.Offscreen)
        {
            // "A scary cat came over, but your mouse wasn't around!"
        }
        else
        {
            if (_mouse.Fearless)
            {
                if (_player.Abilities.Fearless.DropHappiness && _player.Abilities.Fearless.DropWeight)
                {
                    // "A scary cat came over, but your mouse stood its ground. However, its happiness and weight still dropped
                    _mouse.Happiness = _mouse.Happiness < scaryCat.HappinessReduction
                        ? 0
                        : _mouse.Happiness - scaryCat.HappinessReduction;
                    // todo: @junqi let me change the mouse weight please?
                    // _mouse.Weight = _mouse.Weight < scaryCat.WeightReduction
                    //  ? 0
                    //  : _mouse.Weight - scaryCat.WeightReduction;
                }
                else if (_player.Abilities.Fearless.DropHappiness)
                {
                    // "A scary cat came over, but your mouse bravely stood its ground. However, its happiness still dropped."
                    _mouse.Happiness = _mouse.Happiness < scaryCat.HappinessReduction
                        ? 0
                        : _mouse.Happiness - scaryCat.HappinessReduction;
                }
                else
                {
                    // "A scary cat came over, but your mouse was completely nonchalant."
                }
            }
            else
            {
                // "A scary cat came over, and your mouse ran away! Its happiness and weight also dropped."
                _mouse.Happiness = _mouse.Happiness < scaryCat.HappinessReduction
                ? 0
                : _mouse.Happiness - scaryCat.HappinessReduction;
                // todo: @junqi let me change the mouse weight please?
                // _mouse.Weight = _mouse.Weight < scaryCat.WeightReduction
                //  ? 0
                //  : _mouse.Weight - scaryCat.WeightReduction;
                _mouse.Offscreen = true;
                _abilityLastActivatedTimes[AbilityName.ScaryCat] = DateTime.Now;
                _scaryCatDuration = scaryCat.Duration;
            }
        }
    }

    public void ActivateBeastlyBuffet()
    {
        if (_mouse.Happiness < _player.Abilities.BeastlyBuffet.Cost) return;
        // todo: networking
        // call ReceiveBeastlyBuffet(_player.Abilities.BeastlyBuffet) on target player
        _mouse.Happiness -= _player.Abilities.BeastlyBuffet.Cost;
    }

    public void ReceiveBeastlyBuffet(BeastlyBuffet beastlyBuffet)
    {
        var badFoods = _foodPointValues.Where(food => food.Value < beastlyBuffet.PointThreshold).ToList();
        var random = new Random();
        var boostedFood = badFoods[random.Next(badFoods.Count)].Key;
        _foodController.setMaxFoodCount(boostedFood, _foodController.getMaxFoodCount(boostedFood) * _player.Abilities.TreatsGalore.SpawnLimitMultiplier);
        _foodController.setFoodSpawnWeight(boostedFood, _foodController.getFoodSpawnWeight(boostedFood) * _player.Abilities.TreatsGalore.SpawnWeightMultiplier);
        _beastlyBuffetBoostedFood = boostedFood;
        _beastlyBuffetIsActive = true;
        _abilityLastActivatedTimes[AbilityName.BeastlyBuffet] = DateTime.Now;
        _beastlyBuffetDuration = beastlyBuffet.Duration;
    }

    public void ActivateThief()
    {
        // todo: not sure how to affect spawn interval and move food yet
    }

    // I think these could be somewhere else.
    private readonly Dictionary<string, int> _foodPointValues = new Dictionary<string, int>()
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

    private readonly Dictionary<string, float> _defaultFoodSpawnWeights = new Dictionary<string, float>
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

    private readonly Dictionary<string, int> _defaultMaxFoodCounts = new Dictionary<string, int>
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
