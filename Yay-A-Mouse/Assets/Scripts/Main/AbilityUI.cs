﻿using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.UI;

public class AbilityUI : MonoBehaviour
{
    // Ability sprites
    public Sprite[] abilitySprites;

    // Ability button prefab
    public GameObject AbilityButton;

    private AbilityController abilityController;
    private GameObject canvas;
    private GameObject buttonTemplate;
    private Player player;
    private List<Ability> playerAbilities;
    private Mouse mouse;

    private IDictionary<AbilityName, GameObject> abilityButtons;
    private IDictionary<AbilityName, Sprite> abilitySpritesDict;

    // Use this for initialization
    void Start()
    {
        GameObject playerObj = GameObject.Find("Player");

        if (playerObj == null)
        {
            Debug.LogWarning("Couldn't find instance of player, instantiating.");
            GameObject playerPrefab = (GameObject)Resources.Load("Prefabs\\Player");
            playerObj = Instantiate(playerPrefab);

            player = playerObj.GetComponent<Player>();
            player.PAbilities = new Abilities(1, 1, 1, 1, 0, 0, 0);
            abilityController = playerObj.GetComponent<AbilityController>();
        }
        else
        {
            abilityController = GameObject.Find("Player").GetComponent<AbilityController>();
            player = GameObject.Find("Player").GetComponent<Player>();
        }

        playerAbilities = player.getAbilities();
        mouse = GameObject.Find("Mouse").GetComponent<Mouse>();

        buttonTemplate = (GameObject)Resources.Load("Prefabs\\Button");
        canvas = GameObject.Find("Canvas").transform.Find("Abilities").gameObject;

        // Get the height of each button for positioning
        RectTransform buttonRect = AbilityButton.GetComponent<RectTransform>();
        float offset = buttonRect.rect.height * buttonRect.localScale.x * 1.1f;

        abilityButtons = new Dictionary<AbilityName, GameObject>(7);    
        abilitySpritesDict = new Dictionary<AbilityName, Sprite>();

        abilitySprites = Resources.LoadAll<Sprite>("Abilities");

        foreach (Sprite sprite in abilitySprites)
        {
            abilitySpritesDict.Add((AbilityName)Enum.Parse(typeof(AbilityName), sprite.name), sprite);
        }

        float yOffset = -playerAbilities.Count / 2 * offset;
        foreach (Ability ability in playerAbilities)
        {
            abilityButtons[ability.Name] = InstantiateButton(yOffset, ability.Name);
            abilityButtons[ability.Name].name = ability.Name.ToString();
            var ability1 = ability.Name;
            abilityButtons[ability.Name].GetComponent<Button>().onClick.AddListener(() => abilityController.ActivateAbility(ability1));
            yOffset += offset;
        }
    }

    // Update is called once per frame
    void Update()
    {
        foreach (Ability ability in playerAbilities)
        {
            abilityButtons[ability.Name].GetComponent<Button>().interactable = mouse.Happiness >= player.PAbilities[ability.Name].Cost;
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

        button.transform.Find("AbilityLevel").gameObject.GetComponent<Text>().text = player.PAbilities[ability].Level.ToString();

        return button;
    }
}
