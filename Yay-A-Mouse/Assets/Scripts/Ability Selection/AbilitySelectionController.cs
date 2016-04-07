using System;
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

    private bool ready; // if player has chosen two abilities


    // Use this for initialization
    void Start()
    {
        introText = GameObject.Find("IntroText");
        abilityIconUI = GameObject.Find("AbilityIconButtons");
        abilityDetailUI = GameObject.Find("AbilityDetailUI");
        readyButtonObject = GameObject.Find("ReadyButton");

        selectedAbilityIcon = GameObject.Find("SelectedAbilityIcon").GetComponent<Image>();
        selectedAbilityTitle = GameObject.Find("SelectedAbilityTitle").GetComponent<Text>();
        selectedAbilityDescription = GameObject.Find("SelectedAbilityDescription").GetComponent<Text>();
        chooseAbilityButton = GameObject.Find("ChooseAbilityButton").GetComponent<Button>();
        backToSelectionButton = GameObject.Find("BackButton").GetComponent<Button>();
        alreadyChosenText = GameObject.Find("AbilitiesAlreadyChosen");


        // Deactivate ability details and ready button
        abilityDetailUI.SetActive(false);
        readyButtonObject.SetActive(false);

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
        Debug.Log("Scale is " + rectTransform.localScale);
        Debug.Log("Xoffset is " + xOffset);
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
        /*
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
        */

        /*foreach (AbilityName ability in Enum.GetValues(typeof(AbilityName)))
        {
            abilityIcons[ability].GetComponentInChildren<Text>().text = player.PAbilities[ability].Level.ToString();
            // to do: can't get the button to work
            var ability1 = ability;
            abilityIcons[ability].GetComponent<Button>().onClick.AddListener(() => selectedAbility = ability1);
        }


        // to do: proper listener for improveSelectedAbilityButton
        // to do: two points to assign
        */
    }

    // Update is called once per frame
    void Update()
    {
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

        /*	    selectedAbilityName.text = selectedAbility.ToString();
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
                }*/
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
        // .... TODO, change description etc

        abilityDetailUI.SetActive(true);

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
        //LobbyManager.singleton.ServerChangeScene("Main");
    }

}
