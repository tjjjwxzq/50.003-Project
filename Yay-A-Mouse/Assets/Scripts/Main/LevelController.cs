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
public class LevelController : MonoBehaviour
{
    public AudioClip SoundInGameBGM;
    public AudioClip SoundCat;
    public AudioClip SoundFrenzyMode;
    public AudioClip SoundCombo;
    public AudioClip SoundWin;
    public AudioClip SoundLose;


    private AudioSource audio;

    // End game sprites
    public Sprite YouWonSprite;
    public Sprite YouLostSprite;

    // Get this info from the number of player objects on the client
    private bool isUISet = false;
    private int numOpponents;
    public int NumPlayers = -1; // set once Player.NumPlayers is a valid value synchronized with the server

    // Normal or frenzy mode
    public enum GameMode { Normal, Frenzy };
    private GameMode mode = GameMode.Frenzy;

    // Reference to food controller and mouse and object pool scripts 
    private FoodController foodController;
    private Mouse mouse;
    private ObjectPool foodPool; // food pool for frenzy feeding
    private AbilityController abilityController;

    // Food combo state
    private string[] eligibleFoods;
    private string[] foodCombo = new string[3]; // array of food names
    private const float minUpdateTime = 20f;
    private const float maxUpdateTime = 40f;
    private const float WeightedFoodCountMin = 0.5f; // minimum weighted food count (max count * spawn weight) before food type has a chance to be included in combo
    private const int ScoreBonus = 20;
    private const float ComboAnimTime = 3f;
    private float comboAnimCountDown = 0;
    private int numCombos = 0;     // length of combo streak
    private int sequenceFed = 0;     // count the number of correct food fed

    // Frenzy mode 
    private bool changedMode = false; // to ensure set up only happens once, when switching modes
    private const int FrenzyComboCount = 1; // number of (not necessarily consecutive) combos before entering frenzy mode
    private const int FrenzyModeTime = 10; // number of seconds frenzy mode lasts
    private float frenzyTimer = FrenzyModeTime; // timer to countdown frenzy mode

    private GameObject frenzyText; // activate when entering frenzy mode
    private GameObject frenzyBackground; // activate when entering frenzy mode
    private GameObject frenzyFed; // aniamted whenever mouse eats food in frenzy mode
    private Animator frenzyMouseAnimator; // animated when entering and leaving frenzy mode


    // Scary cat animation
    private GameObject scaryCatShock;
    private GameObject scaryCat;

    // Non local Player objects
    private GameObject[] playerObjects;
    private GameObject[] nonLocalPlayerObjects;

    // Container UI game objects
    // Combo UI
    public Sprite comboBGNormal;
    public Sprite comboBGHighlight;
    private GameObject comboUI;
    private GameObject comboText; // "COMBO!" text that pops up when combo is achieved
    private Animator comboTextAnimator;
    private GameObject comboChange;
    private Animator comboChangeAnimator;
    private GameObject comboMouse; // animated star when combo is achieved
    private Image[] comboBackgrounds = new Image[3];
    private Image[] comboImages = new Image[3];
    private Animator[] comboAnimators = new Animator[3];

    // Player avatars
    private GameObject playerAvatarUI;
    private GameObject playerAvatar; // player avatar prefab
    private GameObject[] playerAvatars; // player avatars in UI
    private Color[] playerColors; // player avatar colors
    private string[] playerScores; // player scores
    private Text[] playerScoresText; // player scores text components
    private string[] playerNames; // player names
    private Text[] playerNamesText; // player names text components


    // End Game
    private GameObject endGame;


    // Use this for initialization
    void Start()
    {
        audio = GetComponent<AudioSource>();
        audio.PlayOneShot(SoundInGameBGM);

        // Get food controller and mouse references
        foodController = GameObject.Find("FoodController").GetComponent<FoodController>();
        mouse = GameObject.Find("Mouse").GetComponent<Mouse>();
        foodPool = GetComponent<ObjectPool>();
        foodPool.PoolObject = Resources.Load("Prefabs/Normal") as GameObject;

        abilityController =
            GameObject.FindGameObjectsWithTag("Player")
                .First(o => o.GetComponent<AbilityController>().isLocalPlayer)
                .GetComponent<AbilityController>();

        abilityController.AttachToFoodController()
            .AttachToMouse()
            .AttachToLevelController()
            .AttachToAbilityUi()
            .AttachToMessageBox();

        // Get Combo UI
        comboUI = GameObject.Find("Combo");
        comboText = GameObject.Find("ComboText");
        comboTextAnimator = comboText.GetComponent<Animator>();
        comboChange = GameObject.Find("ComboChange");
        comboChangeAnimator = comboChange.GetComponent<Animator>();
        comboMouse = GameObject.Find("ComboMouse");
        comboText.SetActive(false);
        comboMouse.SetActive(false);
        for (int i = 0; i < comboImages.Length; i++)
        {

            Transform comboChild = comboUI.transform.GetChild(i);
            comboImages[i] = comboChild.GetChild(0).GetComponent<Image>();
            comboBackgrounds[i] = comboChild.GetComponent<Image>();
            comboAnimators[i] = comboChild.GetComponent<Animator>();
            comboAnimators[i].enabled = false; // disable or it will override sprite renderer
        }

        // Get players UI
        playerAvatarUI = GameObject.Find("PlayerUI");

        // Get Frenzy UI
        frenzyText = GameObject.Find("FeedingFrenzy");
        frenzyBackground = GameObject.Find("FrenzyBackground");
        frenzyFed = GameObject.Find("FrenzyFed");
        frenzyMouseAnimator = GameObject.Find("FrenzyMouse").GetComponent<Animator>();
        frenzyText.SetActive(false);
        frenzyBackground.SetActive(false);
        frenzyFed.SetActive(false);

        // Get Scary Cat UI
        scaryCatShock = GameObject.Find("Shock");
        scaryCat = GameObject.Find("Cat");
        scaryCatShock.SetActive(false);
        scaryCat.SetActive(false);

        // Get EndGame UI
        endGame = GameObject.Find("EndGame");
        endGame.SetActive(false);

        // Initialization for food combo
        // All treats are eligible
        eligibleFoods = foodController.FoodValues.Where(kvp => kvp.Value > 0).Select(x => x.Key).ToArray<string>();
        /*eligibleFoods = foodController.MaxFoodCounts.Where(kvp =>
        kvp.Value * foodController.FoodSpawnWeights[kvp.Key] / foodController.TotalFoodSpawnWeight >= WeightedFoodCountMin
            && foodController.FoodValues[kvp.Key] > 0).
            Select(x => x.Key).ToArray<string>();*/
        foreach (string food in eligibleFoods)
            Debug.Log("Eligible foods are " + food);

        // Start updating food combo
        StartCoroutine(primer());
        StartCoroutine(updateFoodCombo());


    }

    void FixedUpdate()
    {
        // Check for collision of food 
        // spawned during frenzy mode with mouse
        // Check regardless of game mode in case frenzy mode
        // ends while there are still fired food on screen
        foreach (Transform food in transform)
        {
            // check for collision with mouse
            if (mouse.MouseCollider2D.OverlapPoint(food.position))
            {
                Debug.Log("Colliding with mouse");
                mouse.Weight += food.GetComponent<Food>().NutritionalValue * mouse.GrowthAbility;
                food.GetComponent<PoolMember>().Deactivate();
                food.position = new Vector2(0, CameraController.MinYUnits);

                // Animate frenzy fed
                frenzyFed.SetActive(false);
                frenzyFed.SetActive(true);
            }
        }


    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log("Level controller num players" + NumPlayers);
        Debug.Log("Player objects are null" + (playerObjects == null));
        if (playerObjects == null || playerObjects.Length < NumPlayers)
        {
            playerObjects = GameObject.FindGameObjectsWithTag("Player");
            Debug.Log("Finding players");
            foreach (GameObject player in playerObjects)
            {
                player.GetComponent<Player>().AttachToMouse();
            }
        }

        if (!isUISet && playerObjects.Length == NumPlayers)
        {
            foreach (GameObject player in playerObjects)
                Debug.Log("Players are " + player);
            // Filter out local player
            nonLocalPlayerObjects = playerObjects.Where(p => !p.GetComponent<Player>().isLocalPlayer).ToArray();
            foreach (GameObject player in nonLocalPlayerObjects)
                Debug.Log("Non local players " + player);
            numOpponents = nonLocalPlayerObjects.Length;

            Debug.Log("Start to set UI");
            setupPlayerAvatarUI();
            isUISet = true;
        }

        updatePlayerScores();
        checkEndGame();

        checkComboStreak();
        checkGameMode();

        if (mode == GameMode.Frenzy)
        {
            if (changedMode)
            {
                Debug.Log("ENTERING FRENZY");
                enterFrenzy();
                changedMode = false;
            }
            runFrenzy();
        }
        else if (mode == GameMode.Normal)
        {
            if (changedMode)
            {
                enterNormal();
                changedMode = false;
            }
        }
        // Count down combo animation
        if (comboAnimCountDown > 0)
        {
            comboAnimCountDown -= Time.deltaTime;
        }

        // Check if combo bgs are animating and stop them if needed
        if (comboUI.activeInHierarchy && comboAnimators[0].enabled && comboAnimCountDown < 0)
        {
            foreach (Animator animator in comboAnimators)
            {
                Debug.Log("Stopping animation");
                animator.SetBool("ComboAnimation", false);
                animator.enabled = false;
            }

            // Reset background
            foreach (Image comboBG in comboBackgrounds)
            {
                Debug.Log("Ressting BG after combo hit");
                comboBG.sprite = comboBGNormal;
            }
        }


    }

    private IEnumerator updateFoodCombo()
    {
        float waittime = Random.Range(minUpdateTime, maxUpdateTime);
        WaitForSeconds waitEnum = new WaitForSeconds(waittime);
        while (true)
        {
            Debug.Log("Updating food combo");
            for (int i = 0; i < foodCombo.Length; i++)
            {
                foodCombo[i] = eligibleFoods[Random.Range(0, eligibleFoods.Length)];
                comboImages[i].sprite = foodController.FoodSpritesDict[foodCombo[i]];
            }

            // Trigger combo change animation
            comboChange.SetActive(true);
            if (comboChange.activeInHierarchy)
                comboChangeAnimator.SetTrigger("ComboChange");

            yield return waitEnum;
        }
    }

    private IEnumerator primer()
    {
        // There's some weird bug with coroutines
        // where it seems to ignore waitforseconds 
        // a few times at a go, if its the first coroutine started
        // So use this dummy coroutine to prime the actual coroutine needed (updateFoodCombo)
        WaitForSeconds waitEnum = new WaitForSeconds(1);
        yield return waitEnum;

    }

    /// <summary>
    /// To update the number of combo streaks
    /// and add weight (points) to the mouse
    /// </summary>
    private void checkComboStreak()
    {
        if (sequenceFed == 3)
        {
            audio.PlayOneShot(SoundCombo);

            Debug.Log("Combo!");
            numCombos += 1;
            sequenceFed = 0;
            mouse.Weight += ScoreBonus;

            // Animate combo backgrounds
            foreach (Animator animator in comboAnimators)
            {
                animator.enabled = true;
                animator.SetBool("ComboAnimation", true);
            }

            // Animate combo text
            comboText.SetActive(true);
            comboTextAnimator.SetTrigger("ComboText");

            // Animate combo star
            comboMouse.SetActive(true);

            comboAnimCountDown = ComboAnimTime;

        }
    }

    /// <summary>
    /// Checks and updates the game mode
    /// </summary>
    private void checkGameMode()
    {
        if (mode == GameMode.Normal && numCombos == FrenzyComboCount)
        {
            Debug.Log("SETTING FRENZY MODEJ");
            mode = GameMode.Frenzy;
            numCombos = 0;
            frenzyTimer = FrenzyModeTime;
            changedMode = true;
        }

        if (mode == GameMode.Frenzy)
        {
            if (frenzyTimer <= 0)
            {
                mode = GameMode.Normal;
                changedMode = true;
                Debug.Log("ENTERING NORMAl");
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
        audio.Stop();
        audio.volume = 0.5F;
        audio.PlayOneShot(SoundInGameBGM);

        // Reset food controller
        foodController.ActivateController();

        // Start mouse rotation
        mouse.StartMouseRotation();

        // Reset UI
        comboUI.SetActive(true);
        frenzyBackground.SetActive(false);
        frenzyMouseAnimator.SetTrigger("Exit");
    }

    /// <summary>
    /// Set up when entering frenzy mode
    /// </summary>
    private void enterFrenzy()
    {
        audio.Stop();
        audio.PlayOneShot(SoundFrenzyMode);

        // Disable physics on rigidbodies of food objects and spawning
        foodController.DeactivateController();

        // Stop mouse rotation
        mouse.StopMouseRotation();

        // Activate UI animation
        comboText.SetActive(false); // to prevent animation from playing after frenzy mode is exited
        comboChange.SetActive(false);// to prevent animation from playing after frenzy mode is exited 
        comboMouse.SetActive(false);
        comboUI.SetActive(false);
        frenzyText.SetActive(true);
        frenzyBackground.SetActive(true);
        frenzyMouseAnimator.SetTrigger("Entry");
    }


    /// <summary>
    /// Run frenzy mode. Detect taps on mouse and shoot food
    /// </summary>
    private void runFrenzy()
    {
        Debug.Log("Runing frenzy");
        if (mouse.detectTapping())
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

    /// <summary>
    /// Starts scary cat animation; to be called by the ability controller
    /// </summary>
    public void ScaryCatAnimation()
    {
        audio.PlayOneShot(SoundCat);
        scaryCatShock.SetActive(true);
        scaryCat.SetActive(true);
    }

    // Update player scores on UI
    private void updatePlayerScores()
    {
        for (int i = 0; i < numOpponents; i++)
        {
            playerScores[i] = nonLocalPlayerObjects[i].GetComponent<Player>().Score.ToString();
            Debug.Log("Updaing sscores " + playerScores[i]);
            playerScoresText[i].text = playerScores[i];
        }
    }

    // Set up player avatar UIs when game starts
    private void setupPlayerAvatarUI()
    {
        Debug.Log("Setting up player UI");
        Debug.Log("Num opponents is " + numOpponents);
        // Get player colors, names and scores 
        playerColors = new Color[numOpponents];
        playerNames = new string[numOpponents];
        playerNamesText = new Text[numOpponents];
        playerScores = new string[numOpponents];
        playerScoresText = new Text[numOpponents];
        for (int i = 0; i < numOpponents; i++)
        {
            Debug.Log("Getting player variables");
            Player playerScript = nonLocalPlayerObjects[i].GetComponent<Player>();
            playerColors[i] = playerScript.Color;
            Debug.Log("Setting non local player color " + playerColors[i]);
            playerNames[i] = playerScript.Name;
            playerScores[i] = playerScript.Score.ToString();

        }

        // Get player avatar prefab
        playerAvatar = Resources.Load("Prefabs/PlayerAvatar") as GameObject;
        float width = playerAvatar.GetComponent<RectTransform>().rect.width;
        playerAvatars = new GameObject[numOpponents];
        Vector2 startPos = new Vector2(numOpponents - 1, 0);

        var crosshairs = new List<GameObject>();
        for (int i = 0; i < playerAvatars.Length; i++)
        {
            Debug.Log("Initializing avatar");
            playerAvatars[i] = (GameObject)Instantiate(playerAvatar);
            playerAvatars[i].GetComponent<Image>().color = playerColors[i];
            var crosshair = playerAvatars[i].transform.Find("Targeted").gameObject;
            crosshair.SetActive(false);
            crosshairs.Add(crosshair);

            // Get text component and set name text
            playerNamesText[i] = playerAvatars[i].transform.Find("PlayerName").gameObject.GetComponent<Text>();
            playerNamesText[i].text = playerNames[i];

            // Get text component and set score text
            playerScoresText[i] = playerAvatars[i].transform.Find("PlayerScore").gameObject.GetComponent<Text>();
            playerScoresText[i].text = playerScores[i];

            playerAvatars[i].transform.SetParent(playerAvatarUI.transform, false);
            playerAvatars[i].GetComponent<RectTransform>().anchoredPosition = startPos + new Vector2(
                i * playerAvatars[i].transform.localScale.x * width * 1.25f, 0);
        }


        for (var i = 0; i < playerAvatars.Length; i++)
        {
            var avatar = playerAvatars[i];
            var button = avatar.AddComponent<Button>();
            var crosshair = avatar.transform.Find("Targeted").gameObject;
            var playerName = playerNames[i];
            button.onClick.AddListener(() =>
            {
                if (crosshair.activeSelf)
                {
                    abilityController.targetedPlayer = "";
                    crosshair.SetActive(false);
                }
                else
                {
                    crosshairs.ForEach(o => o.SetActive(false));
                    crosshair.SetActive(true);
                    abilityController.targetedPlayer = playerName;
                }
            });
        }



    }

    /// <summary>
    /// Checks to see if any player has won
    /// </summary>
    private void checkEndGame()
    {
        if (playerObjects != null)
        {
            bool gameEnd = false;
            foreach (GameObject player in playerObjects)
            {
                if (player.GetComponent<Player>().PlayerWon)
                {
                    gameEnd = true;
                    break;
                }
            }

            if (gameEnd)
            {
                Debug.Log("Ending game");
                // Deactivate food controller and play end game animation
                foodController.DeactivateController();

                bool localPlayerWon = true;
                foreach (GameObject player in nonLocalPlayerObjects)
                {
                    if (player.GetComponent<Player>().PlayerWon)
                    {
                        localPlayerWon = false;
                        break;
                    }
                }

                if (localPlayerWon)
                    endGame.GetComponent<Image>().sprite = YouWonSprite;
                else
                    endGame.GetComponent<Image>().sprite = YouLostSprite;

                endGame.SetActive(true);

            }
        }
    }





}
