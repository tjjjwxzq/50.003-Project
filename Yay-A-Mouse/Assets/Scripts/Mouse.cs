using UnityEngine;
using System.Collections;

// The mouse doesn't need physics, just a Collision2D component
// so we can detect when food/bad stuff touch it
public class Mouse: MonoBehaviour {

    //Mouse status
    private int weight = 0; // In effect the score. Poor skinny mouse. Ranges to 1000
    private int happiness = 0; // Acts as mana of sorts? Charge by player stroking?
    private int level = 0; // Level increases at certain weight thresholds
    private static int weightLevel1 = 50;
    private static int weightLevel2 = 150;
    private static int weightLevel3 = 300;
    private static int weightLevel4 = 500;
    private static int weightFinal = 1000;

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
        //For scaling
        scale = (100 + (float)weight) / weightFinal;
        transform.localScale = defaultScale* scale;
        Debug.Log("defaultScale is " + defaultScale.x + " " + defaultScale.y);
        Debug.Log("localScale is " + transform.localScale.x + " " + transform.localScale.y);
        
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
	}

    // coroutine that rotates specified transform starting counterclockwise from its origin 
    // between two specified angles with linear easing
    // angle1 must be negative and angle2 positive
    // To track the state of easeK it cannot be passed as a parameter 
    // or its value will reset each time
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

    // Check for collision with food/bad objects
    void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("Colliding");
        // Food nom nom
        if(collision.gameObject.tag == "Food") //Use tag so we can different kinds of 'food' game objects tagged as food
        {
            Food food = collision.gameObject.GetComponent<Food>();
            weight += food.value;
            Debug.Log("Nom nom");
        } 
    }
}
