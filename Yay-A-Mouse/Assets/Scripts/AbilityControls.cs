using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts;
using UnityEngine.UI;

public class AbilityControls : MonoBehaviour
{
    private AbilityController abilityController;
    private GameObject canvas;
    private GameObject buttonTemplate;
    private Player player;
    private Mouse mouse;

    private IDictionary<AbilityName, GameObject> abilityButtons;

    // Use this for initialization
    void Start()
    {
        abilityController = GameObject.Find("AbilityController").GetComponent<AbilityController>();
        player = Player.MockPlayer;
        mouse = GameObject.Find("Mouse").GetComponent<Mouse>();

        buttonTemplate = (GameObject)Resources.Load("Prefabs\\Button");
        canvas = GameObject.Find("Canvas").transform.Find("Abilities").gameObject;

        abilityButtons = new Dictionary<AbilityName, GameObject>(7);

        var yOffset = 0;
        foreach (AbilityName ability in Enum.GetValues(typeof(AbilityName)))
        {
            abilityButtons[ability] = InstantiateButton(yOffset, ability);
            var ability1 = ability;
            abilityButtons[ability].GetComponent<Button>().onClick.AddListener(() => abilityController.ActivateAbility(ability1));
            yOffset += 70;
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

    private GameObject InstantiateButton(int yOffset, AbilityName ability)
    {
        var button = Instantiate(buttonTemplate) as GameObject;
        button.transform.SetParent(canvas.transform, false);
        if (yOffset != 0)
        {
            var pos = button.GetComponent<RectTransform>().anchoredPosition;
            button.GetComponent<RectTransform>().anchoredPosition = new Vector2(pos.x, pos.y - yOffset);
        }
        button.transform.Find("AbilityName").gameObject.GetComponent<Text>().text = ability.ToString();
        button.transform.Find("AbilityLevel").gameObject.GetComponent<Text>().text = player.Abilities[ability].Level.ToString();

        return button;
    }
}
