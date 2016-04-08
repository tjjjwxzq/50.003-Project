using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Networking;
using UnityEngine.UI;

public class AbilityUI : MonoBehaviour
{
    // Ability sprites
    public Sprite[] abilitySprites;

    private GameObject canvas;
    private GameObject abilityButtonPrefab;
    private GameObject plusButtonPrefab;

    private AbilityController abilityController;
    private Player localPlayer;
    private List<Ability> playerAbilities;
    private Mouse mouse;

    private IDictionary<AbilityName, GameObject> abilityButtons = new Dictionary<AbilityName, GameObject>();
    private IDictionary<AbilityName, GameObject> abilityUpgradeButtons = new Dictionary<AbilityName, GameObject>();
    private IDictionary<AbilityName, Sprite> abilitySpritesDict = new Dictionary<AbilityName, Sprite>();

    // Use this for initialization
    void Start()
    {
        var playerObj = GameObject.FindGameObjectsWithTag("Player").First(o => o.GetComponent<Player>().isLocal);
        localPlayer = playerObj.GetComponent<Player>();
        abilityController = playerObj.GetComponent<AbilityController>();
        playerAbilities = localPlayer.getAbilities();
        Debug.Log("Ability UI player" + localPlayer);
        Debug.Log("Ability UI playerAbilities" + playerAbilities);
        foreach (Ability ability in playerAbilities)
        {
            Debug.Log(ability.Name);
        }

        mouse = GameObject.Find("Mouse").GetComponent<Mouse>();

        abilityButtonPrefab = (GameObject)Resources.Load("Prefabs\\Button");
        plusButtonPrefab = Resources.Load<GameObject>("Prefabs\\PlusButton");
        canvas = GameObject.Find("Canvas").transform.Find("Abilities").gameObject;

        // Get ability icon sprites
        foreach (Sprite sprite in abilitySprites)
        {
            abilitySpritesDict[(AbilityName)Enum.Parse(typeof(AbilityName), sprite.name)] = sprite;
        }

        // Get the height of each button for positioning
        RectTransform buttonRect = abilityButtonPrefab.GetComponent<RectTransform>();
        float offset = buttonRect.rect.height * buttonRect.localScale.x * 1.1f;

        float yOffset = -playerAbilities.Count / 2 * offset;
        foreach (Ability ability in playerAbilities)
        {
            InstantiateButton(yOffset, ability.Name);
            Debug.Log("instantiated button");
            abilityButtons[ability.Name].name = ability.Name.ToString();
            yOffset += offset;
        }
    }

    // Update is called once per frame
    void Update()
    {
        foreach (Ability ability in playerAbilities)
        {
            abilityButtons[ability.Name].GetComponent<Button>().interactable = mouse.Happiness >= localPlayer.PAbilities[ability.Name].Cost;
            abilityUpgradeButtons[ability.Name].SetActive(abilityController.abilityPoints > 0);
            abilityButtons[ability.Name].GetComponentInChildren<Text>().text = localPlayer.PAbilities[ability.Name].Level.ToString();
        }
    }

    private void InstantiateButton(float yOffset, AbilityName ability)
    {
        var button = Instantiate(abilityButtonPrefab) as GameObject;
        var plusButton = Instantiate(plusButtonPrefab) as GameObject;

        // Set sprite, parent object and rectTransform anchors
        button.GetComponent<Image>().sprite = abilitySpritesDict[ability];
        button.transform.SetParent(canvas.transform, false);
        plusButton.transform.SetParent(canvas.transform, false);
        RectTransform buttonRectTransform = button.GetComponent<RectTransform>();

        Vector2 pos = buttonRectTransform.anchoredPosition;
        buttonRectTransform.anchoredPosition = new Vector2(pos.x, pos.y - yOffset);
        plusButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(pos.x + 20, pos.y - 20 - yOffset);

        button.GetComponentInChildren<Text>().text = localPlayer.PAbilities[ability].Level.ToString();
        button.GetComponent<Button>().onClick.AddListener(() => abilityController.ActivateAbility(ability));

        plusButton.GetComponent<Button>().onClick.AddListener(() => abilityController.ImproveAbility(ability));

        abilityButtons[ability] = button;
        abilityUpgradeButtons[ability] = plusButton;
    }
}
