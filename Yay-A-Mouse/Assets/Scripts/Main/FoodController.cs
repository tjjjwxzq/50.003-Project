using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
# if UNITY_EDITOR
using UnityEditor;
# endif

/// <summary>
/// Script attached to FoodController object that 
/// controls behavior such as food spawning, and food movement.
/// </summary>

public class FoodController : MonoBehaviour {

    public enum Movements { Static, Horizontal, Vertical, Random }; // enumerator of food movements

    private string[] foodNames;// Array of food names
    public Sprite[] FoodSprites; //<! Array of food sprites, to be assigned in the inspector
    private Dictionary<string,Sprite> foodSpritesDict = new Dictionary<string,Sprite>(); //<! array of food sprites
    /// Dictionary of ObjectPool script components attached to FoodController and keyed by the food name
    private Dictionary<string, ObjectPool> foodPoolsDict = new Dictionary<string,ObjectPool>();
    private Movements foodMovement = Movements.Random;//!< Current food movement mode
    private Dictionary<string, int> foodValues = new Dictionary<string, int> //!< Dictionary of food values
    {
        // Good foods
        {"Normal" , 5},
        {"Cheese" , 10 },
        {"Carrot" , 7 },
        {"Oat" , 15 },
        {"Apple" , 8 },
        {"Anchovy" , 12 },
        {"Bread" , 18 },
        {"Seed" , 20 },

        // Bad foods
        {"Bad", -5},
        {"Peanut", -7 },
        {"Orange", -10 },
        {"Garlic", -15 },
        {"Chocolate", -20 },
        {"Poison", -50 }
    };

    /// Dictionary of max number of food for each type that can be on the screen
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

    /// Dictionary of spawning probability weights for each type of food
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
    private float totalFoodSpawnWeight; // Sum of all food spawn probability weights


    private IEnumerator spawnCoroutine;
    private IEnumerator changeDirectionsCoroutine;
    private float minSpawnTime = 0.3f; // Minimum time to wait between food spawning
    private float maxSpawnTime = 2.0f; // Maximum time to wait between food spawning
    private float minChangeTime = 0.5f; // Minimum time to wait between change of food directions
    private float maxChangeTime = 15f; // Maximum time to wait between change of food directions
    private int foodDirection = 1; // Change food direction for Horizontal and Vertical Movement
    private List<Vector2> randDirections = new List<Vector2>(); // Array of random directions For Random Movement

    // Public properties
    // clone data structures to prevent instance escape
    public string[] FoodNames
    {
        get { return (string[]) foodNames.Clone(); }
    }

    public Dictionary<string, int> FoodValues
    {
        get { return new Dictionary<string,int>(foodValues); }
    }

    public Dictionary<string, int> MaxFoodCounts
    {
        get { return new Dictionary<string,int>(maxFoodCounts); }
    }

    public Dictionary<string, float> FoodSpawnWeights
    {
        get { return new Dictionary<string,float>(foodSpawnWeights); }
    }

    public Dictionary<string, ObjectPool> FoodPools
    {
        get { return new Dictionary<string, ObjectPool>(foodPoolsDict); }
    }

    public Dictionary<string,Sprite> FoodSpritesDict
    {
        get { return new Dictionary<string, Sprite>(foodSpritesDict); }
    }
    
    // Ensure that certain members are initialized before
    // they are referenced in the initialization code
    // of other scripts
    void Awake()
    {
        // Get array of food names from sprites assigned in inspector
        foodNames = new string[FoodSprites.Length];
        for(int i = 0; i < FoodSprites.Length; i ++)
        {
            foodNames[i] = FoodSprites[i].name;
        }

        // Add entries to foodSpriteDict
        for(int i = 0; i < FoodSprites.Length; i ++)
        {
            foodSpritesDict.Add(foodNames[i], FoodSprites[i]);
        }

        // Dynamically add food object pools
        foreach(KeyValuePair<string,int> entry in foodValues)
        {
            // Add object pooler
            ObjectPool foodPool = gameObject.AddComponent<ObjectPool>();
            foodPoolsDict.Add(entry.Key, foodPool);

            // Create Prefabs dynamically
            # if UNITY_EDITOR
            // Create pool object
            GameObject food = new GameObject();
            food.tag = "Food"; // ensure its tagged as food so mouse object an detect collision
            // Spawn off-screen
            float posX = CameraController.MinXUnits - 2;
            float posY = CameraController.MinYUnits - 2;
            food.transform.position = new Vector2(posX, posY);
            // Add Sprite, Rigidbody and Collider
            SpriteRenderer foodSprite = food.AddComponent<SpriteRenderer>();
            foodSprite.sprite = foodSpritesDict[entry.Key];
            Rigidbody2D foodRigidBody = food.AddComponent<Rigidbody2D>();
            foodRigidBody.gravityScale = 0; // no gravity
            foodRigidBody.drag = 3; // set linear drag
            food.AddComponent<PolygonCollider2D>();

            // Create and add food script component
            Food.CreateFood(food, entry.Value, entry.Key);

            //string prefabPath = "Assets" + Path.DirectorySeparatorChar + "Resources" + Path.DirectorySeparatorChar
            //                + "Prefabs" + Path.DirectorySeparatorChar + entry.Key + ".prefab";
            // not sure whether Unity uses forward slashes for pathnames by default
            string prefabPath = "Assets/Resources/Prefabs/" + entry.Key + ".prefab";
            PrefabUtility.CreatePrefab(prefabPath, food);
            Destroy(food);
            #endif

            // Assign food object to object pool
            foodPool.PoolObject = Resources.Load("Prefabs/"+entry.Key) as GameObject;
        }

        totalMaxFoodCount = maxFoodCounts.Sum(x => x.Value);
        totalFoodSpawnWeight = foodSpawnWeights.Sum(x => x.Value);

    }

	void Start () {
        spawnCoroutine = SpawnFood();
        changeDirectionsCoroutine = ChangeFoodDirection();
        StartCoroutine(spawnCoroutine);
        StartCoroutine(changeDirectionsCoroutine);
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
            string foodName = foodNames[Random.Range(0, foodNames.Length)];
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
            while (!foodPoolsDict.ContainsKey(foodName))
                foodName = RandomizeFoodType();
               

            if( foodPoolsDict[foodName].ActiveObjects < maxFoodCounts[foodName])
            {

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
                    x = Random.Range(CameraController.MinXUnits, CameraController.MaxXUnits);
                    y = Random.Range(CameraController.MinYUnits, CameraController.MaxYUnits);
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

                GameObject food = foodPoolsDict[foodName].GetObj();

                // Place food object on screen and add to list of random directions for movement
                Rigidbody2D foodBody = food.gameObject.GetComponent<Rigidbody2D>();
                foodBody.position = new Vector3(x, y, 0);
                randDirections.Add(Random.insideUnitCircle * foodBody.drag);

            }

            yield return new WaitForSeconds(Random.Range(minSpawnTime, maxSpawnTime));
        }
    }

    /// <summary>
    /// Coroutine that changes the direction of food movement every random time interval within a range
    /// </summary>
    /// <returns></returns>
    IEnumerator ChangeFoodDirection()
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
            randDirections.Clear(); // clear list of random movements
            foreach(Transform food in transform)
            {
                float drag = food.gameObject.GetComponent<Rigidbody2D>().drag;
                randDirections.Add(Random.insideUnitCircle * drag);
            }

            yield return new WaitForSeconds( Random.Range(minChangeTime, maxChangeTime));
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
    /// Gets the max food count for the specified food type
    /// Can be called by Player Ability code
    /// </summary>
    /// <param name="name">Food type to get count for</param>
    /// <returns></returns>
    public int getMaxFoodCount(string name)
    {
        return maxFoodCounts[name];
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
    /// Gets the food spawn probability weight for the specified food type
    /// Can be called by Player Ability code
    /// </summary>
    /// <param name="name">Food type to get spawn probability weight for</param>
    /// <returns></returns>
    public float getFoodSpawnWeight(string name)
    {
        return foodSpawnWeights[name];
    }

    /// <summary>
    /// Changes food spawning difficulty
    /// Can be called by LevelController to change difficulty of the game
    /// </summary>
    /// <param name="level"></param>
    public void SetDifficulty(int level)
    {
        foodMovement += level;
    }

    /// <summary>
    /// Property to set food movement mode. (May not actually need this?) 
    /// </summary>
    public Movements FoodMovement 
    {
        set { foodMovement = value; }
    }

    /// <summary>
    /// Stops food spawning and change of food direction
    /// and freezes all food on screen in place
    /// </summary>
    public void DeactivateController()
    {
        StopCoroutine(spawnCoroutine);
        StopCoroutine(changeDirectionsCoroutine);

        foreach(Transform food in transform)
        {
            food.gameObject.GetComponent<Rigidbody2D>().isKinematic = true;
        }

    }

    /// <summary>
    /// Restarts food spawning and chaning of food direction
    /// </summary>
    public void ActivateController()
    {
        StartCoroutine(spawnCoroutine);
        StartCoroutine(changeDirectionsCoroutine);
    }


}
