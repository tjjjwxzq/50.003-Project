using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Linq;

// The mouse doesn't need physics, just a Collision2D component
// so we can detect when food/bad stuff touch it
/// <summary>
/// Script class that controls behaviour of the mouse.
/// Keeps track of mouses status such as weight, happiness, level, 
/// and attributes that can be set by player abilities:
/// (immunity, leptinDeficiency, fearless)
/// </summary>
public class Mouse: MonoBehaviour {

    // Reference to levelcontroller
    private LevelController levelController;

    // Coroutines
    private IEnumerator rotationCoroutine;

    //Mouse sprites
    private SpriteRenderer spriteRenderer;
    private SpriteRenderer shadowSpriteRenderer;
    public Sprite[] LevelSprites;
    public Sprite[] ShadowSprites;
    private PolygonCollider2D[] polygonColliders; // colliders corresponding to each level sprite

    //Happiness status sprites
    private Image mouseHappinessImage;
    private Image mouseHappinessFill;
    public Sprite[] HappinessSprites = new Sprite[5];

    //Mouse status
    private int weight = 0; //!< Acts as the player score. Increases/decreases when mouse eats good/bad food
    private int happiness = 0; //!< Acts as mana for player abilities. Increased by player stroking. 
    private int level = 0; //!< Mouse level increases at certain weight thresholds. Affects game difficulty and player abilities
    private bool immunity = false; //!< Immunity to bad foods. Can be set by player ability. (cover all bad foods or only some?)
    private int leptinDeficiency = 1; //!< Ability to gain weight; acts as score multiplier to food points. Can be set by player ability.
    private bool fearless = false; //!< immunity to being scared by cats, ants etc. Can be set by player ability.
    private bool offScreen = false; //!< Whether the mouse should be scared off-screen.
    private bool alreadyOffScreen = false; // whether the mouse has been scared off-screen
    private bool rotateBack = false; // trigger mouse rotate back
    private bool rotatingBack = false; // whether rotating corouting is running
    private bool runBack = false; // trigger mouse run back at the end of rotate back
    private bool runningBack = false; // whether running back coroutine is running
    private bool returnToScreen = false; // to track whether mouse is in the midst of returning to the screen

    // For swiping
    private bool stroked = false; //!< For detecting if a stroke has started
    private Vector2 swipeFirstPos;
    private Vector2 swipeLastPos;
    private float SwipeDistance = Screen.height * 5f / 100f;

    // Weight and Happiness Levels
    private readonly int[] weightLevels = new int[] {0,50,150,300,500,750,1000,1300,1700,2100,2500,3000 };
    private readonly int[] happinessLevels = new int[] { 0, 20, 40, 60, 80, 100 }; //!< Happiness levels at which to change happiness indicator

    // For level up animation
    private GameObject levelUpObject;

    //For scaling transform
    //Default size is at full grown size
    private int defaultSize = 1280; //width in pixels
    private Vector3 defaultScale = new Vector3(1,1,1);
    private float scale;

    //For rotation
    private Quaternion currentRotation; // for rotating back before running back to center
    private bool correctRotation = false;
    private bool rotationCorrected = false; // flag set if rotation has been corrected
    private float finalAngle1;
    private float finalAngle2;
    private float easeK;
    private int maxTurnSteps = 3; // value to reset turnSteps to 
    private int turnSteps; // to make sure transform continues turning in the changed direction

    private float timer = 3;

	// Use this for initialization
	void Start () {
        //Get level controller
        levelController = GameObject.Find("LevelController").GetComponent<LevelController>();

        // Level up
        levelUpObject = GameObject.Find("LevelUp");
        levelUpObject.SetActive(false);

        // Move this out later
        // Get local player
	   /* var localPlayerObj = GameObject.FindGameObjectsWithTag("Player").First(obj => obj.GetComponent<Player>().isLocalPlayer);
        localPlayerObj.GetComponent<Player>().AttachToMouse();
        localPlayerObj.GetComponent<AbilityController>().AttachToMouse();*/

        //Components
        spriteRenderer = GetComponent<SpriteRenderer>();
        shadowSpriteRenderer = GameObject.Find("Shadow").GetComponent<SpriteRenderer>();
        polygonColliders = GetComponents<PolygonCollider2D>();
        mouseHappinessImage = GameObject.Find("MouseStatus").GetComponent<Image>();
        mouseHappinessFill = GameObject.Find("Fill").GetComponent<Image>();

        // Deactivate all the but the first collider
        for (int i = 1; i < polygonColliders.Length; i++)
            polygonColliders[i].enabled = false;

        //For scaling
        float weightRatio = (float) weight / weightLevels[weightLevels.Length - 1];
        scale = -0.8f * weightRatio * weightRatio + 1.6f * weightRatio + 0.2f;
        transform.localScale = defaultScale* scale;
        
        //Rotation parameters
        //easeK and maxturnSteps might need hand-tuning
        easeK = 2f;
        turnSteps = maxTurnSteps;
        finalAngle1 = -90f;
        finalAngle2 = -1 * finalAngle1;

        //Rotate mouse
        rotationCoroutine = rotateWithEasing(finalAngle1, finalAngle2, transform);
        StartCoroutine(rotationCoroutine);

	}
	
	// Update is called once per frame
	void Update () {
        //Scale mouse size based on weight
        float weightRatio = (float) weight / weightLevels[weightLevels.Length - 1];
        scale = -0.8f * weightRatio * weightRatio + 1.6f * weightRatio + 0.2f;
        transform.localScale = defaultScale* scale;
        // Update mouse level and sprite
        checkLevel();
        // Update mouse happiness
        updateHappiness();
        // Update mouse happiness sprite
        checkHappiness();

        // Correct mouse rotation if needed (during frenzy mode)
        if (correctRotation)
            CorrectMouseRotation();

        // Check if mouse should be scared off screen
        checkOffScreen();

        /*
         //For testing off screen animation
        timer -= Time.deltaTime;
        if (timer < 0)
        {
            Debug.Log("Triggering offscreen");
            offScreen = !offScreen;
            timer = 3;
            levelController.ScaryCatAnimation();
        }*/
	}


    /// <summary>
    /// Checks and updates mouse level and changes sprite accordingly
    /// </summary>
    void checkLevel()
    {
        // There are 10 levels in total
        // But the first and last element of weightLevels
        // give the starting weight and the final weight
        // so we exclude them
        for(int i = 1; i< weightLevels.Length - 1; i++)
        {
            if( weightLevels[i-1] <= weight && weight < weightLevels[i] && level != i-1)
            {
                // update polygon collider and sprite
                polygonColliders[level].enabled = false;
                level = i-1;
                polygonColliders[level].enabled = true;
                spriteRenderer.sprite = LevelSprites[level];
                shadowSpriteRenderer.sprite = ShadowSprites[level];

                // Play level up animation
                levelUpObject.SetActive(true);
                break;
            }
        }

   }

    void checkHappiness()
    {
        // Update meter fill sprite
        mouseHappinessFill.rectTransform.localScale = new Vector2(1f, ((float)happiness) / happinessLevels[happinessLevels.Length - 1]);

        // Change mouse happiness sprites
       for(int i = 1; i < happinessLevels.Length; i++)
        {
            if( happinessLevels[i-1] <= happiness && happiness < happinessLevels[i] && 
                mouseHappinessImage.sprite != HappinessSprites[i-1])
            {
                mouseHappinessImage.sprite = HappinessSprites[i - 1];
                mouseHappinessImage.SetNativeSize();
            }
        }

       // Set animation
       Animator mouseHappinessAnimator = mouseHappinessImage.GetComponent<Animator>();

       if(happiness == happinessLevels[happinessLevels.Length - 1] && !mouseHappinessAnimator.enabled)
        {
            mouseHappinessAnimator.enabled = true;
        }
       else if(happiness < happinessLevels[happinessLevels.Length - 1] && mouseHappinessAnimator.enabled)
        {
            mouseHappinessAnimator.enabled = false;
        }

    }

    /// <summary>
    /// Updates mouse happiness when stroked
    /// Works for both touch and mouse input
    /// </summary>
    void updateHappiness()
    {
        // For mouse input. Just clicking on the mouse will count as a stroke
        /*if (Input.GetMouseButtonDown(0))
        {
            stroked = detectTouch(Input.mousePosition);

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
            if (stroking && happiness < happinessLevels[happinessLevels.Length - 1])
                happiness++;
            stroked = false;
        }*/

        // For touch input
        if(Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0); 
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    stroked = detectTouch(Input.GetTouch(0).position);
                    if (stroked)
                        swipeFirstPos = Input.GetTouch(0).position;
                    break;

                case TouchPhase.Moved:
                    if (!stroked)
                    {
                        stroked = detectTouch(Input.GetTouch(0).position);
                        if (stroked)
                            swipeFirstPos = Input.GetTouch(0).position;
                    }
                    else
                    {
                        Debug.Log("Stroking");
                        swipeLastPos = Input.GetTouch(0).position;
                        Vector2 diff = swipeFirstPos - swipeLastPos;
                        Debug.Log("diff mag" + diff.magnitude);
                        Debug.Log("Swipe distance" + SwipeDistance);

                        if (diff.magnitude > SwipeDistance)
                        {
                            increaseHappiness();
                            stroked = false;
                        }
                    }
                    break;

                case TouchPhase.Ended:
                    if (stroked)
                    {
                        swipeLastPos = Input.GetTouch(0).position;
                        Vector2 diff = swipeFirstPos - swipeLastPos;

                        if (diff.magnitude > SwipeDistance)
                        {
                            increaseHappiness();
                        }
                    }
                    stroked = false;
                    break;

            }

        }


    }

    // Function to be callec by updateHappiness
    // to isolate how the happiness is incremented
    private void increaseHappiness()
    {
        if (happiness < happinessLevels[happinessLevels.Length - 1])
            happiness++;
    }

    /// <summary>
    /// To be called by level controller
    /// to see whether mouse has been tapped
    /// Fires food at mouse if mouse is tapped
    /// </summary>
    public bool detectTapping()
    {
         // For mouse input
        if (Input.GetMouseButtonDown(0))
        {
            return detectTouch(Input.mousePosition);
        }

        // For touch input
        if(Input.touchCount > 0)
        {
            // this needs to be changed
            Touch touch = Input.GetTouch(0); 
            switch (touch.phase)
            {
               case TouchPhase.Ended:
                    return true;
            }

        }

        return false;
    }


    /// <summary>
    /// Detects whether mouse has been touched
    /// If yes sets stroked to true to begin stroke detection
    /// </summary>
    /// <param name="pos">Position of touch</param>
    bool detectTouch(Vector3 pos)
    {
        Vector3 wpos = Camera.main.ScreenToWorldPoint(pos);
        Vector2 touchPos = new Vector2(wpos.x, wpos.y);
        Collider2D colObj = Physics2D.OverlapPoint(touchPos);

        if (colObj == polygonColliders[level])
        {
            return true;
        }

        return false;
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
    IEnumerator rotateWithEasing(float angle1, float angle2, Transform trans)
    {
        for (;;)
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
       
    }

    /// <summary>
    /// Stop mouse rotation
    /// </summary>
    public void StopMouseRotation()
    {
        correctRotation = true;
        StopCoroutine(rotationCoroutine);
    }

    // To make mouse face front during frenzy mode
    private void CorrectMouseRotation()
    {
        transform.rotation = Quaternion.Lerp(transform.rotation, new Quaternion(0,0,0,1f), Time.deltaTime * 5f);
        if (Quaternion.Angle(transform.rotation, Quaternion.identity) < 1)
        {
            correctRotation = false;
            rotationCorrected = true;
        }
    }

    /// <summary>
    /// Start mouse rotation
    /// </summary>
    public void StartMouseRotation()
    {
        StartCoroutine(rotationCoroutine);
    }

    // Checks for off screen handling
    private void checkOffScreen()
    {
        // Check if mouse should be sent off screen
        if (offScreen && !alreadyOffScreen && !returnToScreen) // make sure mouse is not in the midst of returning
        {
            mouseOffScreen();
            alreadyOffScreen = true;
        }

        // Check if mouse should come back
        if(!offScreen && alreadyOffScreen)
        {
            rotateBack = true;
            returnToScreen = true;
            alreadyOffScreen = false;
        }
 
        // Check if mouse should rotate back 
        if (rotateBack && !rotatingBack)
        {
            StartCoroutine(rotateBackToCenter());
            rotatingBack = true;
        }

        // Check if mouse should run back
        if (runBack && !runningBack)
        {
            StartCoroutine(runBackToCenter());
            runningBack = true;
        }

        if (rotationCorrected && returnToScreen)
        {
            StartCoroutine(rotationCoroutine);
            rotationCorrected = false;
            returnToScreen = false;
        }

    }

    // Handle when mouse is scared off the screen
    private void mouseOffScreen()
    {
        StopCoroutine(rotationCoroutine);
        StartCoroutine(runOffScreen());
    }

    // Handle mouse translation for running off screen
    IEnumerator runOffScreen()
    {
        currentRotation = transform.rotation;
        float zRad = (90 + currentRotation.eulerAngles.z) * Mathf.Deg2Rad ;
        Vector2 direction = Mathf.Tan(zRad)  > 0 ? - (new Vector2(1, Mathf.Tan(zRad))) : new Vector2(1,Mathf.Tan(zRad));
        while(spriteRenderer.bounds.max.x > CameraController.MinXUnits && spriteRenderer.bounds.min.x < CameraController.MaxXUnits
            && spriteRenderer.bounds.min.y < CameraController.MaxYUnits && spriteRenderer.bounds.max.y > CameraController.MinYUnits)
        {
            float speed = 20f;
            transform.Translate( direction.normalized * Time.deltaTime * speed, Space.World);

            yield return null;
        }

    }

    // Rotate back towards center
    IEnumerator rotateBackToCenter()
    {
        float finalZAngle = currentRotation.eulerAngles.z + 180f;
        Quaternion finalRotation = Quaternion.Euler(0, 0, finalZAngle);
        for (;;)
        {
            Debug.Log("Rotating");
            transform.rotation = Quaternion.Lerp(transform.rotation, finalRotation, 5f * Time.deltaTime);
            
            if (Quaternion.Angle(transform.rotation, finalRotation) < 1)
                break;
            yield return null;
        }

        runBack = true;
        rotateBack = false;
        rotatingBack = false;
    }

    // Run back towards center
    IEnumerator runBackToCenter()
    {
        for (;;)
        {
            transform.position = Vector2.Lerp(transform.position, Vector2.zero, 1f * Time.deltaTime);
            if((transform.position - Vector3.zero).magnitude < 0.1)
            {
                transform.position = Vector2.zero;
                break;
            }
            yield return null;
        }

        runBack = false;
        runningBack = false;
        correctRotation = true;
    }

    // Check for collision with food
    void OnCollisionEnter2D(Collision2D collision)
    {
        // Food nom nom
        if(collision.gameObject.tag == "Food")  // different food prefabs are tagged with food
        {
            Food food = collision.gameObject.GetComponent<Food>();
            // update combo sequence
            levelController.UpdateSequence(food.Type);
            // include score multiplier only for good food
            weight += food.NutritionalValue > 0 ? food.NutritionalValue * leptinDeficiency: food.NutritionalValue;
            if (weight < 0)
                weight = 0;
            if (weight > weightLevels[weightLevels.Length - 1])
                weight = weightLevels[weightLevels.Length - 1];
            collision.gameObject.GetComponent<PoolMember>().Deactivate();
        } 
    }

    // Properties

    /// <summary>
    /// Property to get mouse weight. Read only. 
    /// </summary>
    public int Weight
    {
        get { return weight; }
        set {
            weight = value;
            if (weight < 0)
                weight = 0;
        }
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
        set {
            happiness = value;
            if (happiness < 0)
                happiness = 0;
        }
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

    public PolygonCollider2D MouseCollider2D
    {
        get { return polygonColliders[level]; }
    }

}
