using UnityEngine;
using System.Collections;

// The mouse doesn't need physics, just a Collision2D component
// so we can detect when food/bad stuff touch it
/// <summary>
/// Script class that controls behaviour of the mouse.
/// Keeps track of mouses status such as weight, happiness, level, 
/// and attributes that can be set by player abilities:
/// (immunity, leptinDeficiency, fearless)
/// </summary>
public class Mouse: MonoBehaviour {

    //Mouse sprites
    private SpriteRenderer spriteRenderer;
    public Sprite spriteLevel0;
    public Sprite spriteLevel1;
    public Sprite spriteLevel2;
    public Sprite spriteLevel3;
    public Sprite spriteLevel4;

    //Mouse status
    private int weight = 0; //!< Acts as the player score. Increases/decreases when mouse eats good/bad food
    private int happiness = 0; //!< Acts as mana for player abilities. Increased by player stroking. 
    private int maxHappiness = 100; //!< Maximum happiness level
    private bool stroked = false; //!< For detecting if a stroke has started
    private bool stroking = false; //!< For detecting if mouse has been stroked
    private int level = 0; //!< Mouse level increases at certain weight thresholds. Affects game difficulty and player abilities
    private static int weightLevel1 = 50;
    private static int weightLevel2 = 150;
    private static int weightLevel3 = 300;
    private static int weightLevel4 = 500;
    private static int weightFinal = 1000;
    private bool immunity = false; //!< Immunity to bad foods. Can be set by player ability. (cover all bad foods or only some?)
    private int leptinDeficiency = 1; //!< Ability to gain weight; acts as score multiplier to food points. Can be set by player ability.
    private bool fearless = false; //!< immunity to being scared by cats, ants etc. Can be set by player ability.

    //For scaling transform
    //Default size is at full grown size
    private static int defaultSize = 1280; //width in pixels
    private static Vector3 defaultScale = new Vector3(1,1,1);
    private float scale; 

    //For rotation
    private float finalAngle1;
    private float finalAngle2;
    private float easeK;
    private static int maxturnSteps = 3; // value to reset turnSteps to 
    private int turnSteps; // to make sure transform continues turning in the changed direction

	// Use this for initialization
	void Start () {
        //Components
        spriteRenderer = GetComponent<SpriteRenderer>();

        //For scaling
        scale = (100 + (float)weight) / weightFinal;
        transform.localScale = defaultScale* scale;
        
        //Rotation parameters
        //easeK and maxturnSteps might need hand-tuning
        easeK = 2f;
        turnSteps = maxturnSteps;
        finalAngle1 = -90f;
        finalAngle2 = -1 * finalAngle1;
	
	}
	
	// Update is called once per frame
	void Update () {
        //Scale mouse size based on weight
        scale = (100 + (float)weight) / weightFinal;
        transform.localScale = defaultScale* scale;
        //Rotate mouse
        StartCoroutine(RotateWithEasing(finalAngle1, finalAngle2, transform));
        // Update mouse level
        checkLevel();
        // Update mouse happiness
        updateHappiness();
        
	}

    /// <summary>
    /// Checks and updates mouse level and changes sprite accordingly
    /// </summary>
    void checkLevel()
    {
        if (weight < weightLevel1 && level != 0)
        {
            level = 0;
            spriteRenderer.sprite = spriteLevel0;
        }
        if (weightLevel1 <= weight && weight < weightLevel2 && level != 1 )
        {
            level = 1;
            spriteRenderer.sprite = spriteLevel1;
        }
        if (weight >= weightLevel2 && weight < weightLevel3 && level != 2)
        {
            level = 2;
            spriteRenderer.sprite = spriteLevel2;
        }
        if (weight >= weightLevel3 && weight < weightLevel4 && level != 3)
        {
            level = 3;
            spriteRenderer.sprite = spriteLevel3;
        }
        if (weight >= weightLevel4 && weight < weightFinal && level != 4)
        {
            level = 4;
            spriteRenderer.sprite = spriteLevel4;
        }
        if (weight >= weightFinal && level != -1)
            level = -1; // indicates end game
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
            if (stroking && happiness < maxHappiness)
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
                    if(stroking && happiness < maxHappiness)
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
            turnSteps = maxturnSteps;
            easeK *= -1;
        }

        turnSteps--;

        float rotationSpeed = diff * easeK;
        trans.Rotate(0, 0, rotationSpeed * Time.deltaTime);
        yield return null;
        
    }

    // Check for collision with food
    void OnCollisionEnter2D(Collision2D collision)
    {
        // Food nom nom
        if(collision.gameObject.tag == "Food") //Use tag so we can different kinds of 'food' game objects tagged as food
        {
            Food food = collision.gameObject.GetComponent<Food>();
            // include score multiplier only for good food
            weight += food.value > 0 ? food.value * leptinDeficiency: food.value;
            collision.gameObject.GetComponent<PoolMember>().Deactivate();
            Debug.Log("Nom nom");
        } 
    }

    //Attribute getters

    /// <summary>
    /// Gets mouse level
    /// </summary>
    /// <returns></returns>
    public int getLevel() { return level; }

    /// <summary>
    /// Gets mouse happiness
    /// </summary>
    /// <returns></returns>
    public int getHappiness() { return happiness; }

    // Attribute setters

    /// <summary>
    /// Sets mouse immunity.
    /// Can be called by Player Ability code
    /// </summary>
    /// <param name="immunity">Boolean value to set immunity to</param>
    public void setImmunity(bool immunity) { this.immunity = immunity; }

    /// <summary>
    /// Sets mouse leptinDeficiency (score multiplier)
    /// Can be called by Player Ability code
    /// </summary>
    /// <param name="leptinDeficiency">Value to set leptinDeficiency to</param>
    public void setGrowthAbility(int leptinDeficiency) { this.leptinDeficiency = leptinDeficiency; }

    /// <summary>
    /// Sets mouse fearless(ness)
    /// Can be called by Player Ability code
    /// </summary>
    /// <param name="fearless">Boolean value to set fearless to</param>
    public void setFearlessness(bool fearless) { this.fearless = fearless; } 

}
