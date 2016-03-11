using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Script attached to FoodController object that 
/// controls behavior such as food spawning, and food movement.
/// </summary>

public class FoodController : MonoBehaviour {

    /// <summary>
    /// Enumerator of food movement modes
    /// </summary>
    public enum Movements { Static, Horizontal, Vertical, Random }; 

    /// <summary>
    /// Array of food names
    /// </summary>
    public static readonly string[] FOOD_NAMES = new string[]
        {"Normal", "Cheese", "Carrot", "Oat", "Apple", "Anchovy", "Bread", "Seed",
         "Bad", "Peanut", "Orange", "Garlic", "Chocolate", "Poison"};
    /// <summary>
    /// Dictionary of ObjectPool script components attached to FoodController and keyed by the food name
    /// </summary>
    private Dictionary<string, ObjectPool> objPoolsDict = new Dictionary<string,ObjectPool>();
    /// <summary>
    /// Current food movement mode
    /// </summary>
    private Movements foodMovement = Movements.Random;
    /// <summary>
    /// Dictionary of max number of food for each type that can be on the screen
    /// </summary>
    private Dictionary<string, int> maxFoodCounts = new Dictionary<string, int>
    {
        // Good foods
        {"Normal" , 15 },
        {"Cheese" , 8 },
        {"Carrot" , 10 },
        {"Oat" , 5 },
        {"Apple" , 9 },
        {"Anchovy" , 3 },
        {"Bread" , 3 },
        {"Seed" , 2 },

        // Bad foods
        {"Bad", 10 },
        {"Peanut", 7 },
        {"Orange", 5 },
        {"Garlic", 3 },
        {"Chocolate", 2 },
        {"Poison", 1 }
    };
    private int totalMaxFoodCount; //!< The total number of food that can be on the screen
    /// <summary>
    /// Dictionary of spawning probability weights for each type of food
    /// </summary>
    private Dictionary<string, float> foodSpawnWeights = new Dictionary<string, float>
    {
         // Good foods
        {"Normal" , 6f},
        {"Cheese" , 2.5f },
        {"Carrot" , 3.5f},
        {"Oat" , 2f },
        {"Apple" , 3f },
        {"Anchovy" , 1.5f },
        {"Bread" , 1f },
        {"Seed" , 0.8f },

        // Bad foods
        {"Bad", 4f },
        {"Peanut", 2f },
        {"Orange", 1.5f },
        {"Garlic", 1.2f },
        {"Chocolate", 0.8f },
        {"Poison", 0.2f }
    };
    private float totalFoodSpawnWeight; //!< Sum of all food spawn probability weights

    private float minSpawnTime = 1f; //!< Minimum time to wait between food spawning
    private float maxSpawnTime = 3.0f; //!< Maximum time to wait between food spawning
    private int foodDirection = 1; //!< Change food direction for Horizontal and Vertical Movement
    private Vector2[] randDirections; //!< Array of random directions For Random Movement
    

	// Use this for initialization
	void Start () {
        ObjectPool[] objPools = gameObject.GetComponents<ObjectPool>();
        foreach(ObjectPool objPool in objPools)
        {
            Debug.Log(objPool.gameObject);
            objPoolsDict.Add(objPool.obj.GetComponent<Food>().type, objPool); 
        }
        Debug.Log(objPoolsDict);
        totalMaxFoodCount = maxFoodCounts.Sum(x => x.Value);
        totalFoodSpawnWeight = foodSpawnWeights.Sum(x => x.Value);
        StartCoroutine(SpawnFood());
        StartCoroutine(changeFoodDirection());

	}
	
	// Update is called once per frame
	void Update () {
    }

    void FixedUpdate()
    {

        /// 
        /// Add force to move food based on food movement mode
        ///
        switch (foodMovement)
        {
            case Movements.Horizontal:
                foreach(Transform food in transform)
                {
                    Rigidbody2D foodBody = food.gameObject.GetComponent<Rigidbody2D>();
                    foodBody.AddForce(new Vector3(foodBody.drag*foodDirection, 0, 0));
                }
                break;

            case Movements.Vertical:
                foreach(Transform food in transform)
                {
                    Rigidbody2D foodBody = food.gameObject.GetComponent<Rigidbody2D>();
                    foodBody.AddForce(new Vector3(0, foodBody.drag*foodDirection, 0));
                }
                break;

            case Movements.Random:
                int i = 0;
                foreach(Transform food in transform)
                {
                    Rigidbody2D foodBody = food.gameObject.GetComponent<Rigidbody2D>();
                    foodBody.AddForce(randDirections[i]);
                    i++;
                }
                break;

        }
    }

    // For randomly getting the name of a foodtype to spawn
    // to be called in SpawnFood()
    // Probabilities are weighted
    /// <summary>
    /// Randomly gets the name of a food type to spawn
    /// Probabilities are weighted
    /// To be called in ::SpawnFood()
    /// </summary>
    /// <returns></returns>
    string RandomizeFoodType()
    {
        while (true)
        {
            string foodName = FOOD_NAMES[Random.Range(0, FOOD_NAMES.Length)];
            float prob = Random.value * totalFoodSpawnWeight;
            if (prob <= foodSpawnWeights[foodName])
            {
                //Debug.Log("Weighted food name is " + foodName);
                return foodName;
            }
        }

    }

    /// <summary>
    /// Coroutine that spawns food on screen
    /// Spawns one food object each time it is executed, from a randomly selected type
    /// </summary>
    /// <returns></returns>
    IEnumerator SpawnFood()
    {
        for (;;)
        {
            string foodName="";
            // Randomize which food type to spawn
            while (!objPoolsDict.ContainsKey(foodName))
                foodName = RandomizeFoodType();
               

            if( objPoolsDict[foodName].getActiveObjects() < maxFoodCounts[foodName])
            {
                GameObject food = objPoolsDict[foodName].getObj();

                // Spawn at a random position
                // make sure it doesn't overlap with
                // over food objects or the mouse
                float x;
                float y;
                while(true){
                    // Make ranges of spawn position with different probabilities
                    //  magnitude < orthoSize/3: P = 0.2
                    //  magnitude < 2* orthoSize/3: P = 0.3
                    //  magnitude < orthosize: P = 0.5
                    x = Random.Range(CameraController.minXUnits, CameraController.maxXUnits);
                    y = Random.Range(CameraController.minYUnits, CameraController.maxYUnits);
                    Vector2 foodPos = new Vector2(x, y);
                   Collider2D colObj = Physics2D.OverlapPoint(foodPos);
                    if (colObj == null)
                    {
                        float prob = Random.value;
                        if (prob <= 0.2 && foodPos.magnitude < Camera.main.orthographicSize/3f)
                        {
                            break;
                        }
                        else if(prob > 0.2 && prob <= 0.5 && foodPos.magnitude <= 2*Camera.main.orthographicSize/3f
                            && foodPos.magnitude > Camera.main.orthographicSize / 3f)
                        {
                            break;
                        }
                        else if(prob > 0.5 && foodPos.magnitude > 2 * Camera.main.orthographicSize / 3f)
                        {
                            break;
                        }
 
                    }
                }

                food.GetComponent<Rigidbody2D>().position = new Vector3(x,y,0);
            }

            yield return new WaitForSeconds(Random.Range(minSpawnTime, maxSpawnTime));
        }
    }

    /// <summary>
    /// Coroutine that changes the direction of food movement every random time interval within a range
    /// </summary>
    /// <returns></returns>
    IEnumerator changeFoodDirection()
    {
        // Customize the waittime range later
        // Where would be a good place to store
        // these values?
        // It would depend on how long that movement mode
        // lasts as well
        for (;;)
        {
            // For Horizontal and Vertical Movements
            foodDirection *= -1;

            // For Random Movement
            randDirections = new Vector2[totalMaxFoodCount];
            int i = 0;
            foreach(Transform food in transform)
            {
                float drag = food.gameObject.GetComponent<Rigidbody2D>().drag;
                randDirections[i] = Random.insideUnitCircle * drag;
                i++;
            }
            float waittime = Random.Range(5f, 20f);
            yield return new WaitForSeconds(3);
        }
        
    }

    /// <summary>
    /// Sets the max food count for the specified food type
    /// Can be called by Player Ability code
    /// </summary>
    /// <param name="name">Name of food</param>
    /// <param name="count">Max count to set to</param>
    public void setMaxFoodCount(string name, int count)
    {
        maxFoodCounts[name] = count;
    }

    /// <summary>
    /// Sets the food spawn probability weight for the specified food type
    /// Can be called by Player Ability code
    /// </summary>
    /// <param name="name"></param>
    /// <param name="weight"></param>
    public void setFoodSpawnWeight(string name, float weight)
    {
        foodSpawnWeights[name] = weight;
    }

    /// <summary>
    /// Changes food spawning difficulty
    /// Can be called by LevelController to change difficulty of the game
    /// </summary>
    /// <param name="level"></param>
    public void setDifficulty(int level)
    {
        foodMovement += level;
    }

    // Attribute setters 
    public void setMovement(Movements movement)
    {
        foodMovement = movement;
    }


}
