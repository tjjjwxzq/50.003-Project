using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Networking;

public class AbilityControls : NetworkBehaviour
{
    // Ability sprites
    public Sprite[] abilitySprites;

    // Ability button prefab
    public GameObject AbilityButton;

    private AbilityController abilityController;
    private GameObject canvas;
    private GameObject buttonTemplate;
    private Player player;
    private Abilities playerAbilities;
    private Mouse mouse;

    private IDictionary<AbilityName, GameObject> abilityButtons;
    private IDictionary<AbilityName, Sprite> abilitySpritesDict;

    // Use this for initialization
    void Start()
    {
        abilityController = GameObject.Find("AbilityController").GetComponent<AbilityController>();
        player = Player.MockPlayer;
        playerAbilities = player.Abilities;
        mouse = GameObject.Find("Mouse").GetComponent<Mouse>();

        buttonTemplate = (GameObject)Resources.Load("Prefabs\\Button");
        canvas = GameObject.Find("Canvas").transform.Find("Abilities").gameObject;

        float offset = AbilityButton.GetComponent<RectTransform>().rect.height;

        abilityButtons = new Dictionary<AbilityName, GameObject>(7);
        abilitySpritesDict = new Dictionary<AbilityName, Sprite>();

        foreach(Sprite sprite in abilitySprites)
        {
            abilitySpritesDict.Add((AbilityName) Enum.Parse(typeof(AbilityName), sprite.name), sprite);
        }

        float yOffset =  - playerAbilities.Length/2 * offset;
        foreach (AbilityName ability in playerAbilities)
        {
            abilityButtons[ability] = InstantiateButton(yOffset, ability);
            abilityButtons[ability].name = ability.ToString();
            var ability1 = ability;
            abilityButtons[ability].GetComponent<Button>().onClick.AddListener(() => abilityController.ActivateAbility(ability1));
            RectTransform buttonRect = abilityButtons[ability].GetComponent<RectTransform>();
            yOffset += buttonRect.rect.height * buttonRect.localScale.x * 1.1f;
        }
    }

    // Update is called once per frame
    void Update()
    {
        foreach (AbilityName ability in Enum.GetValues(typeof(AbilityName)))
        {
            abilityButtons[ability].GetComponent<Button>().interactable = mouse.Happiness >= player.Abilities[ability].Cost;
        }
    }

    private GameObject InstantiateButton(float yOffset, AbilityName ability)
    {
        var button = Instantiate(buttonTemplate) as GameObject;
        // Set sprite, parent object and rectTransform anchors
        button.GetComponent<Image>().sprite = abilitySpritesDict[ability];
        button.transform.SetParent(canvas.transform, false);
        RectTransform buttonRectTransform = button.GetComponent<RectTransform>();

        if (yOffset != 0)
        {
            Vector2 pos = buttonRectTransform.anchoredPosition;
            buttonRectTransform.anchoredPosition = new Vector2(pos.x, pos.y - yOffset);
            
        }

        button.transform.Find("AbilityLevel").gameObject.GetComponent<Text>().text = player.Abilities[ability].Level.ToString();

        return button;
    }
}
