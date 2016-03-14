using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts;
using UnityEngine.Assertions;

public class AbilityController : MonoBehaviour
{
    private Mouse _mouse;
    private FoodController _foodController;
    private Player _player;

    //    private string _treatsGaloreAffectedFood;

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
            if (!IsAbilityActive(AbilityName.Immunity))
                _mouse.Immunity = false;

        // todo: deactivate treats galore

        if (_mouse.Fearless)
            if (!IsAbilityActive(AbilityName.Fearless))
                _mouse.Fearless = false;

        // todo: deactivate fat mouse

        // todo: deactivate scary cat

        // todo: deactivate beastly buffet

        // todo: deactivate thief
    }

    private bool IsAbilityActive(AbilityName abilityName)
    {
        if (_abilityLastActivatedTimes.ContainsKey(abilityName))
        {
            var timeSinceLastActivation = DateTime.Now.Subtract(_abilityLastActivatedTimes[abilityName]).Seconds;
            return timeSinceLastActivation < _player.Abilities[abilityName].Duration;
        }
        else return false;
    }

    public void ActivateImmunity()
    {
        if (_mouse.Immunity) return;
        if (_mouse.Happiness < _player.Abilities.Immunity.Cost) return;
        _mouse.Immunity = true;
        _abilityLastActivatedTimes[AbilityName.Immunity] = DateTime.Now;
    }

    public void ActivateFearless()
    {
        if (_mouse.Fearless) return;
        if (_mouse.Happiness < _player.Abilities.Fearless.Cost) return;
        _mouse.Fearless = true;
        _abilityLastActivatedTimes[AbilityName.Fearless] = DateTime.Now;
    }

    public void ActivateTreatsGalore()
    {
        // todo:
    }

    public void ActivateFatMouse()
    {
        // todo: @junqi update mouse api
        // if (_mouse.Fat) return;
        if (_mouse.Happiness < _player.Abilities.FatMouse.Cost) return;
        // _mouse.Fat = true;
        // _mouse.WeightMultiplier = _player.Abilities.FatMouse.WeightMultiplier;
        _abilityLastActivatedTimes[AbilityName.FatMouse] = DateTime.Now;
    }

    public void ActivateScaryCat()
    {
        // todo:
    }

    public void ActivateBeastlyBuffet()
    {
        // todo:
    }

    public void ActivateThief()
    {
        // todo:
    }
}
