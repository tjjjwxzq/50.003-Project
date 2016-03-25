using System;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class AbilityUpgradeController : MonoBehaviour
{
    private Text selectedAbilityName;
    private Text selectedAbilityDescription;
    private Text selectedAbilityCurrentLevel;
    private Text selectedAbilityNextLevel;
    private Button improveSelectedAbilityButton;

    private Player player;

    private AbilityName selectedAbility;
    private int selectedAbilityLevel;

	// Use this for initialization
	void Start ()
	{
        // todo: create ability icons programatically and add onclick handlers

	    selectedAbilityName = GameObject.Find("SelectedAbilityTitle").GetComponent<Text>();
	    selectedAbilityDescription = GameObject.Find("SelectedAbilityDescription").GetComponent<Text>();
	    selectedAbilityCurrentLevel = GameObject.Find("SelectedAbilityCurrentLevel").GetComponent<Text>();
	    selectedAbilityNextLevel = GameObject.Find("SelectedAbilityNextLevel").GetComponent<Text>();
	    improveSelectedAbilityButton = GameObject.Find("ImproveSelectedAbilityButton").GetComponent<Button>();

	    player = new Player(Abilities.EmptyAbilities);
        selectedAbility = AbilityName.Fearless;
	    selectedAbilityLevel = player.PAbilities[selectedAbility].Level;
	}
	
	// Update is called once per frame
	void Update ()
	{
        // todo: create data structure to hold strings

	    selectedAbilityName.text = selectedAbility.ToString();
	    selectedAbilityDescription.text = "SELECTED ABILITY DESC";
	    selectedAbilityCurrentLevel.text = player.PAbilities[selectedAbility].Level.ToString();
	    selectedAbilityNextLevel.text = (player.PAbilities[selectedAbility].Level + 1).ToString();
	}
}
