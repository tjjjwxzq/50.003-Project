﻿using System;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class AbilitySelectionController : MonoBehaviour
{
    public Sprite cancelSelectionSprite;
    public Sprite confirmSelectionSprite;
    public Sprite overlayIcon;
    public Sprite[] abilityIconSprites;
    private Dictionary<AbilityName, Sprite> abilityIconSpritesDict = new Dictionary<AbilityName, Sprite>();
    private GameObject abilityIconPrefab;
    private Dictionary<AbilityName, GameObject> abilityIcons;
    public Player player; // local player object, to be set by local player object in OnStartLocalPlayer

    private GameObject introText; // intro text shown in selection ui
    private GameObject abilityIconUI; // container for ability icons
    private GameObject abilityDetailUI; // container for ability details
    private GameObject readyButtonObject;  // object for ready button
    private GameObject readyAndWaitingObject; // object for ready and waiting text
    private GameObject alreadyChosenText; // text to show if player has already chosen two abilities

    private GameObject[] selfHelpAbilityButtons; // self help ability icon button game objects
    private GameObject[] sabotageAbilityButtons; // sabotage ability icon button game objects

    // Ability detail UI components
    private AbilityName selectedAbility;
    private Image selectedAbilityIcon;
    private Text selectedAbilityTitle;
    private Text selectedAbilityDescription;
    private Button chooseAbilityButton;
    private Button backToSelectionButton;
    private GameObject selectedAbilityLevelDetails;
    private Text selectedAbilityLevelHeader;
    private Text selectedAbilityLevelHappiness;
    private Text selectedAbilityLevelDuration;
    private Text selectedAbilityLevelDetail;
    private int currentLevelIndex; // which level details to display

    // Ready countdownUI
    public GameObject countdownUI; // for lobby manager to access when activating countdown

    private bool ready; // if player has chosen two abilities

    // Ability details data
    private Dictionary<AbilityName, string> abilityTitles = new Dictionary<AbilityName, string>
    {
        {AbilityName.Immunity, "Immunity" },
        {AbilityName.TreatsGalore, "Treats Galore" },
        {AbilityName.Fearless, "Fearless" },
        {AbilityName.FatMouse, "Fat Mouse" },
        {AbilityName.Thief, "Thief" },
        {AbilityName.ScaryCat, "Scary Cat" },
        {AbilityName.BeastlyBuffet, "Beastly Buffet" }
    };

    private Dictionary<AbilityName, string> abilityDescription = new Dictionary<AbilityName, string>
    {
        {AbilityName.Immunity, "Your mouse becomes immune to bad food for some time" },
        {AbilityName.TreatsGalore, "Spawn more good food to feed your mouse" },
        {AbilityName.Fearless, "Your mouse becomes fearless in the face of a scary cat" },
        {AbilityName.FatMouse, "Your mouse gains weight more easily" },
        {AbilityName.Thief, "Steal good food from one of you opponents"},
        {AbilityName.ScaryCat, "Send a cat over to scare off your opponent's mouse" },
        {AbilityName.BeastlyBuffet, "Spawn more bad food on your opponent's screen" }
    };

    // Ability level details
    private struct AbilityLevel
    {
        public string level;
        public string happiness;
        public string duration;
        public string description;

        public AbilityLevel(string level, string happiness, string duration, string description)
        {
            this.level = level;
            this.happiness = happiness;
            this.duration = duration;
            this.description = description;
        }
    }

    // Change this to extract static fields from the Ability subclasses
    private Dictionary<AbilityName, AbilityLevel[]> abilityLevelDetails = new Dictionary<AbilityName, AbilityLevel[]>
    {
        {AbilityName.Immunity, new AbilityLevel[]
        {
            new AbilityLevel("Level 1", "30", "10", "Immunity to all bad foods"),
            new AbilityLevel("Level 2", "30", "30", "Immunity to all bad foods"),
        } },
        {AbilityName.TreatsGalore, new AbilityLevel[]
        {
            new AbilityLevel("1", "80", "15", "Immunity to all bad foods"),
            new AbilityLevel("2", "30", "30", "Immunity to all bad foods"),
        } },
        {AbilityName.Fearless, new AbilityLevel[]
        {
            new AbilityLevel("1", "80", "15", "Immunity to all bad foods"),
            new AbilityLevel("2", "30", "30", "Immunity to all bad foods"),
        } },
        {AbilityName.FatMouse, new AbilityLevel[]
        {
            new AbilityLevel("1", "80", "15", "Immunity to all bad foods"),
            new AbilityLevel("2", "30", "30", "Immunity to all bad foods"),
        } },
        {AbilityName.Thief, new AbilityLevel[]
        {
            new AbilityLevel("1", "80", "15", "Immunity to all bad foods"),
            new AbilityLevel("2", "30", "30", "Immunity to all bad foods"),
        } },
        {AbilityName.ScaryCat, new AbilityLevel[]
        {
            new AbilityLevel("1", "80", "15", "Immunity to all bad foods"),
            new AbilityLevel("2", "30", "30", "Immunity to all bad foods"),
        } },
        {AbilityName.BeastlyBuffet, new AbilityLevel[]
        {
            new AbilityLevel("1", "80", "15", "Immunity to all bad foods"),
            new AbilityLevel("2", "30", "30", "Immunity to all bad foods"),
        } },
    };


    // Use this for initialization
    void Start()
    {
        introText = GameObject.Find("IntroText");
        abilityIconUI = GameObject.Find("AbilityIconButtons");
        abilityDetailUI = GameObject.Find("AbilityDetailUI");
        readyButtonObject = GameObject.Find("ReadyButton");
        readyAndWaitingObject = GameObject.Find("ReadyAndWaiting");

        selectedAbilityIcon = GameObject.Find("SelectedAbilityIcon").GetComponent<Image>();
        selectedAbilityTitle = GameObject.Find("SelectedAbilityTitle").GetComponent<Text>();
        selectedAbilityDescription = GameObject.Find("SelectedAbilityDescription").GetComponent<Text>();
        chooseAbilityButton = GameObject.Find("ChooseAbilityButton").GetComponent<Button>();
        backToSelectionButton = GameObject.Find("BackButton").GetComponent<Button>();
        alreadyChosenText = GameObject.Find("AbilitiesAlreadyChosen");
        selectedAbilityLevelDetails = GameObject.Find("SelectedAbilityLevelDetails");
        selectedAbilityLevelHeader = GameObject.Find("LevelHeader").GetComponent<Text>();
        selectedAbilityLevelHappiness = GameObject.Find("HappinessText").GetComponent<Text>();
        selectedAbilityLevelDuration = GameObject.Find("DurationText").GetComponent<Text>();
        selectedAbilityLevelDetail = GameObject.Find("SelectedAbilityLevelDetail").GetComponent<Text>();

        countdownUI = GameObject.Find("CountdownUI");

        // Deactivate ability details and ready button and countdown UI
        abilityDetailUI.SetActive(false);
        readyButtonObject.SetActive(false);
        readyAndWaitingObject.SetActive(false);
        countdownUI.SetActive(false);

        // Initialize object and sprite dictionaries

        foreach (Sprite abilityIcon in abilityIconSprites)
        {
            abilityIconSpritesDict[(AbilityName)Enum.Parse(typeof(AbilityName), abilityIcon.name)] = abilityIcon;
        }

        abilityIconPrefab = Resources.Load("Prefabs/AbilitySelectionIcon") as GameObject;
        abilityIcons = new Dictionary<AbilityName, GameObject>();

        // Set up ability icons
        Array abilityNames = Enum.GetValues(typeof(AbilityName));
        foreach (AbilityName abilityName in abilityNames)
        {
            abilityIcons[abilityName] = Instantiate(abilityIconPrefab);
            abilityIcons[abilityName].name = abilityName.ToString(); // set object name
            abilityIcons[abilityName].GetComponent<Button>().onClick.AddListener(OnAbilityDetail); // set button callback
            abilityIcons[abilityName].GetComponent<Image>().sprite = abilityIconSpritesDict[abilityName]; // set sprite
            abilityIcons[abilityName].GetComponentInChildren<Text>().text = abilityName.ToString(); // change this later; names need spaces
            abilityIcons[abilityName].transform.SetParent(abilityIconUI.transform); // make sure to set parent
            if (Enum.IsDefined(typeof(SelfHelpAbilities), abilityName.ToString()))
            {
                abilityIcons[abilityName].tag = "Self_help";
            }
            else
            {
                abilityIcons[abilityName].tag = "Sabotage";
            }
        }

        selfHelpAbilityButtons = GameObject.FindGameObjectsWithTag("Self_help");
        sabotageAbilityButtons = GameObject.FindGameObjectsWithTag("Sabotage");

        // position ability icons nicely
        float yPos = 54f; // edit in scene view and update here
        RectTransform rectTransform = selfHelpAbilityButtons[0].GetComponent<RectTransform>();
        float xOffset = rectTransform.rect.width * 0.2f * 1.2f; //scale of 0.2
        Vector2 startPos = new Vector2(-xOffset * (selfHelpAbilityButtons.Length / 2 - 0.5f), yPos);

        for (int i = 0; i < selfHelpAbilityButtons.Length; i++)
        {
            rectTransform = selfHelpAbilityButtons[i].GetComponent<RectTransform>();
            rectTransform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
            rectTransform.anchoredPosition = startPos + new Vector2(xOffset * i, 0);
        }

        yPos = -99;
        startPos = new Vector2(-xOffset * (sabotageAbilityButtons.Length / 2), yPos);
        for (int i = 0; i < sabotageAbilityButtons.Length; i++)
        {
            rectTransform = sabotageAbilityButtons[i].GetComponent<RectTransform>();
            rectTransform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
            rectTransform.anchoredPosition = startPos + new Vector2(xOffset * i, 0);
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log("Plyaer is null" + (player == null));
        // When player has chosen two abilities, show ready button
        if (player != null)
        {
            int numAbilities = player.getAbilities().Count;
            if (!ready && numAbilities == 2)
            {
                readyButtonObject.SetActive(true);
                ready = true;
            }

            if (ready && numAbilities < 2)
            {
                readyButtonObject.SetActive(false);
                ready = false;
            }
        }
    }

    // Callback when ability icons are clicked
    public void OnAbilityDetail()
    {
        // Get selected ability
        GameObject selectedAbilityObj = EventSystem.current.currentSelectedGameObject;
        selectedAbility = (AbilityName)Enum.Parse(typeof(AbilityName), selectedAbilityObj.name);

        // Check if player already has selecte ability and set choose ability button sprite
    if (player.hasAbility(selectedAbility))
        {
            alreadyChosenText.SetActive(false);
            chooseAbilityButton.gameObject.SetActive(true);
            (chooseAbilityButton.targetGraphic as Image).sprite = cancelSelectionSprite;
        }
        else
        {
            // Check if player already has two abilities and disable choose ability button
            if (player.getAbilities().Count == 2)
            {
                alreadyChosenText.SetActive(true);
                chooseAbilityButton.gameObject.SetActive(false);
            }
            else
            {
                alreadyChosenText.SetActive(false);
                chooseAbilityButton.gameObject.SetActive(true);
            }

            (chooseAbilityButton.targetGraphic as Image).sprite = confirmSelectionSprite;
        }

        // Change UI
        introText.SetActive(false);
        abilityIconUI.SetActive(false);
        readyButtonObject.SetActive(false);

        // Set detail UI to selected ability
        selectedAbilityIcon.sprite = abilityIconSpritesDict[selectedAbility];
        selectedAbilityTitle.text = abilityTitles[selectedAbility];
        selectedAbilityDescription.text = abilityDescription[selectedAbility];

        selectedAbilityLevelHeader.text = abilityLevelDetails[selectedAbility][0].level;
        selectedAbilityLevelHappiness.text = abilityLevelDetails[selectedAbility][0].happiness;
        selectedAbilityLevelDuration.text = abilityLevelDetails[selectedAbility][0].duration;
        selectedAbilityLevelDetail.text = abilityLevelDetails[selectedAbility][0].description;

        currentLevelIndex = 0;

        abilityDetailUI.SetActive(true);

    }

    // Callback when next level button is pressed
    public void OnNextAbilityLevel()
    {
        // Change level details, increment current level
        currentLevelIndex = currentLevelIndex == abilityLevelDetails[selectedAbility].Length -1 ? 0 : currentLevelIndex + 1;
        selectedAbilityLevelHeader.text = abilityLevelDetails[selectedAbility][currentLevelIndex].level;
        selectedAbilityLevelHappiness.text = abilityLevelDetails[selectedAbility][currentLevelIndex].happiness;
        selectedAbilityLevelDuration.text = abilityLevelDetails[selectedAbility][currentLevelIndex].duration;
        selectedAbilityLevelDetail.text = abilityLevelDetails[selectedAbility][currentLevelIndex].description;
    }

    // Callback when ability is chosen
    public void OnAbilityChosen()
    {
        // Check if player already has selected ability
        if (player.hasAbility(selectedAbility))
        {
            // Then player is cancelling the selection
            player.removeAbility(selectedAbility);

            // Remove overlay on ability icon in selection ui
            abilityIcons[selectedAbility].transform.Find("Overlay").gameObject.GetComponent<Image>().enabled = false;
        }
        else
        {
            // Add selected ability to player's list of abilities
            player.addAbility(selectedAbility);

            // Add overlay on ability icon in selection ui
            abilityIcons[selectedAbility].transform.Find("Overlay").gameObject.GetComponent<Image>().enabled = true;
        }

        // Go back to selection ui 
        introText.SetActive(true);
        abilityIconUI.SetActive(true);
        abilityDetailUI.SetActive(false);
        readyButtonObject.SetActive(ready);
    }

    // Callback when back button is pressed from ability detail screen
    public void OnBackButton()
    {
        abilityDetailUI.SetActive(false);
        introText.SetActive(true);
        abilityIconUI.SetActive(true);
        readyButtonObject.SetActive(ready);
    }

    // Callback when ready button is pressed
    public void OnReadyButton()
    {
        Debug.Log("Readying");
        player.CmdReadyToPlay(true);
        readyButtonObject.SetActive(false);
        readyAndWaitingObject.SetActive(true);
        //LobbyManager.singleton.ServerChangeScene("Main");
    }


    /// <summary>
    /// Called by LobbyManager when ready countdown starts
    /// </summary>
    public void StartCountdown()
    {
        abilityDetailUI.SetActive(false);
        abilityIconUI.SetActive(false);
        readyButtonObject.SetActive(false);
        readyAndWaitingObject.SetActive(false);
        introText.SetActive(false);

        countdownUI.SetActive(true);
    }
}
