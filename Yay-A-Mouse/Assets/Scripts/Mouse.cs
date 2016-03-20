using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

// The mouse doesn't need physics, just a Collision2D component
// so we can detect when food/bad stuff touch it
/// <summary>
/// Script class that controls behaviour of the mouse.
/// Keeps track of mouses status such as weight, happiness, level, 
/// and attributes that can be set by player abilities:
/// (immunity, leptinDeficiency, fearless)
/// </summary>
public class Mouse: MonoBehaviour {
    private RaycastHit hit;

    // HETTY - Variables for generated array
    List<string> playerSwipedCombo = new List<string>();
    List<string> currentCombinationToSwipe = new List<string>();

    // Hetty - Playing Mode - Normal/Frenzy - Get From
    string mode;
    bool NormalMode = true;
    bool FrenzyMode = false;

    //Hetty - GUI - Text to be shown on screen
    //public Text scoreDisplay;
    //public Text streakCountDisplay;
    //public Text frenzyModeTitle;

    public int countStreak;     // Count number of combinations that player get correct                                     
    public int sequenceFed;     // To count the number of correct food fed
    public int orderNumber;


    // Hetty - GUI - Images
    public Image SeqBox1;
    public Image SeqBox2;
    public Image SeqBox3;

    // Hetty - Timer
    float time = 30.0f;
    private float timeout;
    public System.DateTime startTime;              // Need to be global

    //Mouse sprites
    private SpriteRenderer spriteRenderer;
    public Sprite[] LevelSprites = new Sprite[11];

    //Mouse status
    private int weight = 0; //!< Acts as the player score. Increases/decreases when mouse eats good/bad food
    private int happiness = 0; //!< Acts as mana for player abilities. Increased by player stroking. 
    private int level = 0; //!< Mouse level increases at certain weight thresholds. Affects game difficulty and player abilities
    private bool immunity = false; //!< Immunity to bad foods. Can be set by player ability. (cover all bad foods or only some?)
    private int leptinDeficiency = 1; //!< Ability to gain weight; acts as score multiplier to food points. Can be set by player ability.
    private bool fearless = false; //!< immunity to being scared by cats, ants etc. Can be set by player ability.
    private bool offScreen = false; //!< Whether the mouse has been scared off-screen.
    private bool stroked = false; //!< For detecting if a stroke has started
    private bool stroking = false; //!< For detecting if mouse has been stroked

    // Weight and Happiness Levels
    private readonly int[] WeightLevels = new int[] {0,50,150,300,500,750,1000,1300,1700,2100,2500,3000 };
    private readonly int[] HappinessLevels = new int[] { 0, 20, 40, 60, 80, 100 }; //!< Happiness levels at which to change happiness indicator

    //For scaling transform
    //Default size is at full grown size
    private int defaultSize = 1280; //width in pixels
    private Vector3 defaultScale = new Vector3(1,1,1);
    private float scale; 

    //For rotation
    private float finalAngle1;
    private float finalAngle2;
    private float easeK;
    private int maxTurnSteps = 3; // value to reset turnSteps to 
    private int turnSteps; // to make sure transform continues turning in the changed direction

	// Use this for initialization
	void Start () {
        //Components
        spriteRenderer = GetComponent<SpriteRenderer>();

        //For scaling
        scale = (100 + (float)weight) / WeightLevels[WeightLevels.Length -1];
        transform.localScale = defaultScale* scale;
        
        //Rotation parameters
        //easeK and maxturnSteps might need hand-tuning
        easeK = 2f;
        turnSteps = maxTurnSteps;
        finalAngle1 = -90f;
        finalAngle2 = -1 * finalAngle1;

        // Hetty
        currentCombinationToSwipe = Permutation.getfoodCombo();
        Debug.Log("Feed Hamster Start() : " + currentCombinationToSwipe[0] + currentCombinationToSwipe[1] + currentCombinationToSwipe[2]);

        NormalMode = true;

        if (NormalMode == true)
        {
            // Disable frenzyTapping
            GameObject.FindGameObjectWithTag("Mouse").GetComponent<NormalMode>().enabled = true;
            GameObject.FindGameObjectWithTag("Mouse").GetComponent<FrenzyMode>().enabled = false;

            // Intialise Variables
            countStreak = 0;
            orderNumber = 0;

            Debug.Log("countStreak:" + countStreak);

            // Starting Combination - Show on the screen
            SeqBox1.sprite = Resources.Load<Sprite>(currentCombinationToSwipe[0]) as Sprite;
            SeqBox2.sprite = Resources.Load<Sprite>(currentCombinationToSwipe[1]) as Sprite;
            SeqBox3.sprite = Resources.Load<Sprite>(currentCombinationToSwipe[2]) as Sprite;
        }

    }
	
	// Update is called once per frame
	void Update () {
        //Scale mouse size based on weight
        scale = (100 + (float)weight) / WeightLevels[WeightLevels.Length - 1];
        transform.localScale = defaultScale* scale;
        //Rotate mouse
        StartCoroutine(RotateWithEasing(finalAngle1, finalAngle2, transform));
        // Update mouse level
        checkLevel();
        // Update mouse happiness
        updateHappiness();


        normalMode();

    }

    // Detect Click on Object
    void normalMode()
    {
        Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(pos, Vector2.zero);
        if (Input.GetMouseButtonDown(0) && hit != null && hit.collider != null)
        {
            if (hit.collider.tag == "Mouse")
            {
                //score.text = (int.Parse(score.text) + 1) + "";
                //Debug.Log(score.text);
                weight = weight + 10;
                // Increase weight of mouse
                Debug.Log("hit");
            }
        }

        //Text score = GetComponentInChildren<FeedHamster>().scoreDisplay; // if you need a variable of it or method you can access it like this
    }
    
    // Hetty
    void frenzyMode()
    {
        Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(pos, Vector2.zero);
        if (Input.GetMouseButtonDown(0) && hit != null && hit.collider != null)
        {
            if (hit.collider.tag == "Mouse")
            {
                //score.text = (int.Parse(score.text) + 1) + "";
                //Debug.Log(score.text);
                weight = weight * 2;
                // Increase weight of mouse
                Debug.Log("hit");
            }
        }

    }

    /// <summary>
    /// Checks and updates mouse level and changes sprite accordingly
    /// </summary>
    void checkLevel()
    {
        for(int i = 1; i< WeightLevels.Length; i++)
        {
            if( WeightLevels[i-1] <= weight && weight < WeightLevels[i] && level != i-1)
            {
                level = i;
                spriteRenderer.sprite = LevelSprites[i];
                break;
            }
        }if (FrenzyMode == true) {
            // Deactivate Food
        }
   }

    /// <summary>
    /// Updates mouse happiness when stroked
    /// Works for both touch and mouse input
    /// </summary>
    void updateHappiness()
    {
        // For mouse input
        if (Input.GetMouseButtonDown(0))
        {
            detectTouch(Input.mousePosition);

        }
        if (Input.GetMouseButton(0))
        {
            if (stroked)
            {

                Vector2 direction = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
                //Debug.Log(direction.magnitude*10);
                if (direction.magnitude != 0)
                    stroking = true;
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (stroking && happiness < HappinessLevels[HappinessLevels.Length - 1])
                happiness++;
            stroked = false;
        }

        // For touch input
        if(Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0); //might we consider multiple touches?
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    detectTouch(Input.GetTouch(0).position);
                    break;

                case TouchPhase.Moved:
                    if (stroked)
                    {
                        float magnitude = touch.deltaPosition.magnitude;
                        if (magnitude != 0)
                            stroking = true;
                    }
                    break;

                case TouchPhase.Ended:
                    if(stroking && happiness < HappinessLevels[HappinessLevels.Length -1])
                        happiness ++;
                    stroking = false;
                    break;

            }

        }


    }


    /// <summary>
    /// Detects whether mouse has been touched
    /// If yes sets stroked to true to begin stroke detection
    /// </summary>
    /// <param name="pos">Position of touch</param>
    void detectTouch(Vector3 pos)
    {
        Vector3 wpos = Camera.main.ScreenToWorldPoint(pos);
        Vector2 touchPos = new Vector2(wpos.x, wpos.y);
        Collider2D colObj = Physics2D.OverlapPoint(touchPos);

        if (colObj == GetComponent<Collider2D>())
        {
            stroked = true;
        }

    }



    /// <summary>
    /// coroutine that rotates specified transform starting counterclockwise from its origin 
    /// between two specified angles with linear easing
    /// angle1 must be negative and angle2 positive
    /// To track the state of easeK it cannot be passed as a parameter 
    /// or its value will reset each time
    /// </summary>
    /// <param name="angle1">Smaller angle; must be negative</param>
    /// <param name="angle2">Larger angle; must be positive</param>
    /// <param name="trans">The transform to rotate</param>
    /// <returns></returns>
    IEnumerator RotateWithEasing(float angle1, float angle2, Transform trans)
    {
        if(angle1>0 || angle2 < 0)
        {
            Debug.Log("Wrong angle input");
            yield return null;
        }
            
        float angDisplacement = trans.eulerAngles.z > 180 ? trans.eulerAngles.z - 360 : trans.eulerAngles.z;
        float angAccDir = Mathf.Sign(angDisplacement);
        float diff;
        //Get correct easing difference based on sign of displacement
        if (angAccDir == 1f)
            diff = Mathf.Abs(angDisplacement - angle2);
        else
            diff = Mathf.Abs(angDisplacement - angle1);

        //Change direction
        //If easeK is too large the rotation may jump past the target
        //the threshold to check should thus be proportional to easeK
        //but might need hand-tuning
        if (diff <= Mathf.Abs(easeK) && turnSteps < 0)
        {
            turnSteps = maxTurnSteps;
            easeK *= -1;
        }

        turnSteps--;

        float rotationSpeed = diff * easeK;
        trans.Rotate(0, 0, rotationSpeed * Time.deltaTime);
        yield return null;
        
    }
    // Hetty - On Tapping

    // Hetty - Add food check here
    // Check for collision with food
    void OnCollisionEnter2D(Collision2D collision)
    {
        // Food nom nom
        if(collision.gameObject.tag == "Food") //Use tag so we can different kinds of 'food' game objects tagged as food
        {


            Food food = collision.gameObject.GetComponent<Food>();

            playerSwipedCombo.Add(food.name);                // Getting the name of object
            Debug.Log("Food Fed:" + food.name);

            if (playerSwipedCombo.Count == 3)
            {

                // Check sequence then change

                // Compare playerSwipedCombo with comboGenerated[ordernumber][i];
                for (int i = 0; i < playerSwipedCombo.Count; i++)
                {
                    if (playerSwipedCombo[i].Equals(currentCombinationToSwipe[i]))
                    {
                        sequenceFed = sequenceFed + 1;

                    }

                }

                // Get another sequence
                currentCombinationToSwipe = Permutation.getfoodCombo();

                // Increase order number so that you can display the next combo
                orderNumber = orderNumber + 1;

                // Change combo picture
                SeqBox1.sprite = Resources.Load<Sprite>(currentCombinationToSwipe[0]) as Sprite;
                SeqBox2.sprite = Resources.Load<Sprite>(currentCombinationToSwipe[1]) as Sprite;
                SeqBox3.sprite = Resources.Load<Sprite>(currentCombinationToSwipe[2]) as Sprite;


                // Clear array that holds user food input
                playerSwipedCombo.Clear();

                // Check if player feed correctly
                if (sequenceFed == 3)
                {
                    sequenceFed = 0;
                    countStreak = countStreak + 1;

                    // Display Total Count Streak on the screen
                    //streakCountDisplay.text = countStreak + "";


                    // Check if use qualify to enter frenzy mode
                    if (countStreak == 3)
                    {
                        //frenzyModeTitle.text = "Frenzy Mode!";
                        countStreak = 0;
                        // Start Timer
                        startTime = System.DateTime.Now;

                        Debug.Log("In FRENZYYY");

                        /*while (FrenzyMode = true) {

                            FrenzyMode = GameObject.Find("Mouse").GetComponent<FrenzyModeScript>();
                            FrenzyModeScript goToFrenzyMode = gameObject.GetComponent<FrenzyModeScript>();
                            goToFrenzyMode.insideFrenzyMode();
                        }*/

                        countStreak = 0;




                        Debug.Log("Back in normal mode");
                    }
                }
                else {
                    countStreak = 0;
                    //streakCountDisplay.text = countStreak + "";
                }


            }

            // include score multiplier only for good food
            weight += food.value > 0 ? food.value * leptinDeficiency: food.value;
            collision.gameObject.GetComponent<PoolMember>().Deactivate();
            Debug.Log("Nom nom");
        } 
    }

    // Properties

    /// <summary>
    /// Property to get mouse weight. Read only. 
    /// </summary>
    public int Weight
    {
        get { return weight; }
    }

    /// <summary>
    ///  Property to get mouse level. Read only.
    /// </summary>
    public int Level
    {
        get { return level; }
    }

    public int Happiness
    {
        get { return happiness; }
        set { happiness = value; }
    }

    /// <summary>
    /// Property to get and set mouse immunity
    /// Can be called by Player Ability code
    /// </summary>
    public bool Immunity
    {
        get { return immunity; }
        set { immunity = value; }
    }

    /// <summary>
    /// Property to get and set mouse growth ability (leptin deficiency)
    /// Can be called by Player Ability code
    /// </summary>
    public int GrowthAbility
    {
        get { return leptinDeficiency; }
        set { leptinDeficiency = value; }
    }

    /// <summary>
    /// Property to get and set mouse fearless(ness)
    /// Can be called by Player Ability code
    /// </summary>
    public bool Fearless
    {
        get { return fearless; }
        set { fearless = value; }
    }

    /// <summary>
    /// Property to get and set whether the mouse is off-screen
    /// Can be called by Player Ability code
    /// </summary>
    public bool Offscreen
    {
        get { return offScreen; }
        set { offScreen = value; }
    }

}
