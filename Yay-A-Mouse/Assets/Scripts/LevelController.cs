using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Handles game state, such as the player's combo streak,
/// whether or not the player is in frenzy feeding mode,
/// and the number of players in a game.
/// Handles state of combo and player avatar UI as well (?) 
/// </summary>
public class LevelController : MonoBehaviour {

    // Get this info from the number of player objects on the client
    private int numPlayers = 3;

    // Normal or frenzy mode
    public enum GameMode { Normal, Frenzy };
    private GameMode mode = GameMode.Normal;

    // Reference to food controller and mouse and object pool scripts 
    private FoodController foodController;
    private Mouse mouse;
    private ObjectPool foodPool; // food pool for frenzy feeding

    // Food combo state
    private string[] eligibleFoods;
    private string[] foodCombo = new string[3]; // array of food names
    private const float minUpdateTime = 10f;
    private const float maxUpdateTime = 20f;
    private const int FoodCountMin = 5; // minimum max food count before food type has a chance to be included in combo
    private const int ScoreBonus = 20;
    private int countStreak = 0;     // length of combo streak
    private int sequenceFed = 0;     // count the number of correct food fed

    // Frenzy mode 
    private bool changedMode = false; // to ensure set up only happens once, when switching modes
    private const int FrenzyComboCount = 5; // length of combo streak before entering frenzy mode
    private const int FrenzyModeTime = 10; // number of seconds frenzy mode lasts
    private float frenzyTimer = FrenzyModeTime; // timer to countdown frenzy mode

    // Container UI game objects
    public Sprite comboBGNormal;
    public Sprite comboBGHighlight;
    private GameObject comboUI;
    private Image[] comboBackgrounds = new Image[3];
    private Image[] comboImages = new Image[3];
    private GameObject playerAvatarUI;
    private GameObject playerAvatar; // player avatar prefab
    private GameObject[] playerAvatars; // player avatars in UI
    private Color[] playerColors; // player avatar colors
    private string[] playerScores; // player scores
    private Text[] playerScoresText; // player scores text components
    private string[] playerNames; // player names
    private Text[] playerNamesText; // player names text components


	// Use this for initialization
	void Start () {

        // Get food controller and mouse references
        foodController = GameObject.Find("FoodController").GetComponent<FoodController>();
        mouse = GameObject.Find("Mouse").GetComponent<Mouse>();
        foodPool = GetComponent<ObjectPool>();
        foodPool.PoolObject = Resources.Load("Prefabs/Normal") as GameObject;

        // Get Combo UI
        comboUI = GameObject.Find("Combo");
        for(int i = 0; i < comboImages.Length; i++)
        {
            comboImages[i] = comboUI.transform.GetChild(i).GetChild(0).GetComponent<Image>();
            comboBackgrounds[i] = comboUI.transform.GetChild(i).GetComponent<Image>();
        }

        // Get players UI
        playerAvatarUI = GameObject.Find("Players");

        // Get player colors, names and scores (from network manager?)
        playerColors = new Color[numPlayers];
        playerNames = new string[numPlayers];
        playerNamesText = new Text[numPlayers];
        playerScores = new string[numPlayers];
        playerScoresText = new Text[numPlayers];
        for(int i =0; i < playerColors.Length; i++)
        {
            playerColors[i] = new Color(100, 100, 100, 1);
            playerNames[i] = "Player " + i;
            playerScores[i] = "0";

        }


        // Get player avatar prefab
        playerAvatar = Resources.Load("Prefabs/PlayerAvatar") as GameObject;
        float width = playerAvatar.GetComponent<RectTransform>().rect.width;

        playerAvatars = new GameObject[numPlayers];

        for(int i = 0; i < playerAvatars.Length; i++)
        {
            playerAvatars[i] = (GameObject)Instantiate(playerAvatar);
            playerAvatars[i].GetComponent<Image>().color = playerColors[i];

            // Get text component and set name text
            playerNamesText[i] = playerAvatars[i].transform.Find("PlayerName").gameObject.GetComponent<Text>();
            playerNamesText[i].text = playerNames[i];

            // Get text component and set score text
            playerScoresText[i] = playerAvatars[i].transform.Find("PlayerScore").gameObject.GetComponent<Text>();
            playerScoresText[i].text = playerScores[i];

            playerAvatars[i].transform.SetParent(playerAvatarUI.transform, false);
            playerAvatars[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(
                (i-1) * playerAvatars[i].transform.localScale.x * width * 1.25f, 0);
        }

        // Initialization for food combo
        eligibleFoods = foodController.MaxFoodCounts.Where(kvp => kvp.Value >= FoodCountMin && foodController.FoodValues[kvp.Key] > 0).
            Select(x => x.Key).ToArray<string>();
        foreach (string food in eligibleFoods)
            Debug.Log(food);

        // Start updating food combo
        StartCoroutine(updateFoodCombo());


	}
	
	// Update is called once per frame
	void Update () {
        checkComboStreak();
        checkGameMode();

        if (mode == GameMode.Frenzy)
        {
            if (changedMode)
            {
                enterFrenzy();
                changedMode = false;
            }
            runFrenzy();
        }
        else if(mode == GameMode.Normal)
        {
            if (changedMode)
            {
                enterNormal();
                changedMode = false;
            }
        }

        // Check for collision of food with mouse
        // Check regardless of game mode in case frenzy mode
        // ends while there are still fired food on screen
        foreach(Transform food in transform)
        {
            // check for collision with mouse
            if(mouse.gameObject.GetComponent<PolygonCollider2D>().OverlapPoint(food.position))
            {
                mouse.Weight += food.GetComponent<Food>().NutritionalValue * mouse.GrowthAbility;
                food.GetComponent<PoolMember>().Deactivate();
                food.position = new Vector2(0, CameraController.MinYUnits);
            }
        }


	}

    private IEnumerator updateFoodCombo()
    {
        // Reset food combo
        for (;;)
        {
            for(int i = 0; i< foodCombo.Length; i++)
            {
                Debug.Log("Updating food combo");
                foodCombo[i] = eligibleFoods[Random.Range(0,eligibleFoods.Length)];
                comboImages[i].sprite = foodController.FoodSpritesDict[foodCombo[i]];
            }
        yield return new WaitForSeconds(Random.Range(minUpdateTime, maxUpdateTime));

        }

    }

    /// <summary>
    /// To update the number of combo streaks
    /// and add weight (points) to the mouse
    /// </summary>
    private void checkComboStreak()
    {
        if(sequenceFed == 3)
        {
            Debug.Log("Combo!");
            countStreak += 1;
            sequenceFed = 0;
            mouse.Weight += ScoreBonus;
        }
    }

    /// <summary>
    /// Checks and updates the game mode
    /// </summary>
    private void checkGameMode()
    {
        if(mode == GameMode.Normal && countStreak == FrenzyComboCount)
        {
            mode = GameMode.Frenzy;
            countStreak = 0;
            changedMode = true;
        }

        if(mode == GameMode.Frenzy)
        {
            if (frenzyTimer <= 0)
            {
                mode = GameMode.Normal;
                changedMode = true;
            }
            else
                frenzyTimer -= Time.deltaTime;
        }
    }

    /// <summary>
    /// Set up when entering normal mode
    /// </summary>
    private void enterNormal()
    {
        // Reset food controller
        foodController.ActivateController();
    }

    /// <summary>
    /// Set up when entering frenzy mode
    /// </summary>
    private void enterFrenzy()
    {
        // Disable physics on rigidbodies of food objects and spawning
        foodController.DeactivateController();
    }


    /// <summary>
    /// Run frenzy mode. Detect taps on mouse and shoot food
    /// </summary>
    private void runFrenzy()
    {
        if(mouse.detectTapping())
        {
            Debug.Log("Spawning food");
            GameObject food = foodPool.GetObj();
            food.transform.SetParent(transform);
            Rigidbody2D foodBody = food.GetComponent<Rigidbody2D>();
            foodBody.position = new Vector2(0, CameraController.MinYUnits);
            foodBody.isKinematic = true;
            foodBody.velocity = Vector2.up * 10;
        }


    }

    /// <summary>
    /// To be called by the mouse script
    /// when the mouse is fed, to determine
    /// if food fed matches with the combo sequence
    /// </summary>
    /// <param name="foodType"></param>
    public void UpdateSequence(string foodType)
    {
        if (foodType.Equals(foodCombo[sequenceFed], System.StringComparison.Ordinal))
        {
            comboBackgrounds[sequenceFed].sprite = comboBGHighlight;
            sequenceFed += 1;
            Debug.Log("Updating sequence" + sequenceFed);
        }
        else
        {
            // Reset sequence
            foreach (Image comboBG in comboBackgrounds)
                comboBG.sprite = comboBGNormal;
            sequenceFed = 0;
            Debug.Log("Reset sequence" + sequenceFed);
        }
    }

    

    
}
