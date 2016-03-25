using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class AbilityUpgradeController : MonoBehaviour
{
    private Text selectedAbilityName;
    private Text selectedAbilityDescription;
    private Text selectedAbilityCurrentLevel;
    private Text selectedAbilityCurrentLevelDetails;
    private Text selectedAbilityNextLevel;
    private Text selectedAbilityNextLevelDetails;
    private Button improveSelectedAbilityButton;

    private Dictionary<AbilityName, GameObject> abilityIcons; 

    private Player player;

    private AbilityName selectedAbility;
    private int selectedAbilityLevel;

	// Use this for initialization
	void Start ()
	{
	    player = new Player(Abilities.EmptyAbilities);
	    selectedAbility = AbilityName.BeastlyBuffet;
	    selectedAbilityLevel = player.PAbilities[selectedAbility].Level;

	    selectedAbilityName = GameObject.Find("SelectedAbilityTitle").GetComponent<Text>();
	    selectedAbilityDescription = GameObject.Find("SelectedAbilityDescription").GetComponent<Text>();
	    selectedAbilityCurrentLevel = GameObject.Find("SelectedAbilityCurrentLevel").GetComponent<Text>();
	    selectedAbilityCurrentLevelDetails = GameObject.Find("SelectedAbilityCurrentLevelDetails").GetComponent<Text>();
	    selectedAbilityNextLevel = GameObject.Find("SelectedAbilityNextLevel").GetComponent<Text>();
	    selectedAbilityNextLevelDetails = GameObject.Find("SelectedAbilityNextLevelDetails").GetComponent<Text>();
	    improveSelectedAbilityButton = GameObject.Find("ImproveSelectedAbilityButton").GetComponent<Button>();

	    abilityIcons = new Dictionary<AbilityName, GameObject>(7)
        {
            {AbilityName.Fearless, GameObject.Find("Fearless") },
            {AbilityName.Immunity, GameObject.Find("Immunity") },
            {AbilityName.TreatsGalore, GameObject.Find("TreatsGalore") },
            {AbilityName.FatMouse, GameObject.Find("FatMouse") },
            {AbilityName.ScaryCat, GameObject.Find("ScaryCat") },
            {AbilityName.BeastlyBuffet, GameObject.Find("BeastlyBuffet") },
            {AbilityName.Thief, GameObject.Find("Thief") }
        };

        foreach (AbilityName ability in Enum.GetValues(typeof(AbilityName)))
        {
            abilityIcons[ability].GetComponentInChildren<Text>().text = player.PAbilities[ability].Level.ToString();
            // todo: can't get the button to work
            var ability1 = ability;
            abilityIcons[ability].GetComponent<Button>().onClick.AddListener(() => selectedAbility = ability1);
        }

        // even this doesn't work
        improveSelectedAbilityButton.onClick.AddListener(() => Debug.Log("HI"));

        // todo: proper listener for improveSelectedAbilityButton
        // todo: two points to assign
    }
	
	// Update is called once per frame
	void Update ()
	{
	    selectedAbilityName.text = selectedAbility.ToString();
	    selectedAbilityDescription.text = player.PAbilities[selectedAbility].Description;

        selectedAbilityCurrentLevel.text = "Current level " + player.PAbilities[selectedAbility].Level.ToString();
        if (player.PAbilities[selectedAbility].Level == 0)
	    {
	        selectedAbilityCurrentLevelDetails.enabled = false;
	    }
	    else
	    {
            selectedAbilityCurrentLevelDetails.text = player.PAbilities[selectedAbility].GetDetails();
        }
  
	    if (player.PAbilities[selectedAbility].Level == player.PAbilities[selectedAbility].MaxLevel)
	    {
	        selectedAbilityNextLevel.enabled = false;
	        selectedAbilityNextLevelDetails.enabled = false;
	    }
	    else
	    {
	        selectedAbilityNextLevel.text = "Next level " + (player.PAbilities[selectedAbility].Level + 1).ToString();
	        selectedAbilityNextLevelDetails.text = player.PAbilities[selectedAbility].GetDetails();
	    }
	}
}
