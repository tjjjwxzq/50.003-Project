﻿using UnityEngine;
using System.Collections;

/// <summary>
/// Component attached to Food GameObjects that handles
/// swipe detection
/// </summary>
public class Food: MonoBehaviour {

    private Rigidbody2D rigidbody;
    private SpriteRenderer spriteRenderer;
    private float speedScale = 0.5f; //factor to scale touch swipe speed by; hand-tune this or linear drag
    public int NutritionalValue; //<! Nutritional value, equivalent to units of mouse weight (aka points)
    public string Type; //<! Food type
    private bool moveable = false; // moveable if it has been tuoched

    private float SwipeDistance = Screen.height * 5f/ 100f;
    private Vector2 firstPos;
    private Vector2 lastPos;
    private const float MoveTime = 0.1f;
    private float moveTimeCountDown; // prevents food for moving if player swipes too slowly

    // Factory method
    /// <summary>
    /// Factory method to be called by the FoodController
    /// for programatically generating prefabs of each food type
    /// </summary>
    /// <param name="food">The food GameObject to create</param>
    /// <param name="nutritionalValue">The nutritional value of the food</param>
    /// <param name="type">The name of the food type</param>
    public static void CreateFood( GameObject food, int nutritionalValue, string type)
    {
        Food foodScript = food.AddComponent<Food>();
        foodScript.NutritionalValue = nutritionalValue;
        foodScript.Type = type;
    }

	// Use this for initialization
	void Start () {
        rigidbody = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

     }

    // Update is called once per frame
    void Update () {
        // Mouse input
        // activate this only for editor testing
        // touch input won't work properly when this is activated as well
        //detectMouseSwipe();
        // MAKE SURE TO TURN THIS OFF IS TOUCH INPUT IS SCREWED UP

        // Touch input
        detectTouchSwipe();

        // Check if food is out of screen; if so return it to object pool
        if(gameObject.GetComponent<PoolMember>() != null)
        {
            if(spriteRenderer.bounds.min.x > CameraController.MaxXUnits || spriteRenderer.bounds.max.x < CameraController.MinXUnits)
            {
                // set moveable to false
                // so it doesn't start as moveable when next activated
                moveable = false;
                gameObject.GetComponent<PoolMember>().Deactivate();
            }
            
            if(spriteRenderer.bounds.min.y > CameraController.MaxYUnits || spriteRenderer.bounds.max.y < CameraController.MinYUnits)
            {
                // set moveable to false
                // so it doesn't start as moveable when next activated
                moveable = false;
                gameObject.GetComponent<PoolMember>().Deactivate();
            }
     
        }

	
	}

    // For testing with mouse click and drag
    void detectMouseSwipe()
    {
        if (Input.GetMouseButtonDown(0))
            detectTouch(Input.mousePosition);
        if (Input.GetMouseButton(0))
        {
            if (moveable)
            {
                Vector2 direction = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
                rigidbody.AddForce(direction * 50);
            }
        }

        if(Input.GetMouseButtonUp(0))
            moveable = false;
    }

    // For touch swipe
    void detectTouchSwipe()
    {
        if(Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0); //might we consider multiple touches?
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    detectTouch(Input.GetTouch(0).position);
                    break;

                case TouchPhase.Moved:
                    if (moveable)
                    {
                        moveTimeCountDown -= Time.deltaTime;
                        if(moveTimeCountDown <= 0)
                        {
                            moveable = false;
                        }

                        lastPos = Input.GetTouch(0).position;
                        // Player did swipe
                        Vector2 diff = lastPos - firstPos;
                        if (diff.magnitude >= SwipeDistance)
                        {
                            float speed = touch.deltaPosition.magnitude * speedScale / touch.deltaTime;
                            rigidbody.AddForce(diff.normalized * speed, ForceMode2D.Force);
                            moveable = false;
                        }

                    }
                    else
                    {
                        // Allow more leeway and detect touch also when finger is moving
                        detectTouch(Input.GetTouch(0).position);
                    }
                    break;

                case TouchPhase.Ended:
                    Debug.Log("Touch ended");
                    if (moveable)
                    {
                        lastPos = Input.GetTouch(0).position;
                        // Player did swipe
                        Vector2 diff = lastPos - firstPos;
                        if (diff.magnitude >= SwipeDistance)
                        {
                            float speed = touch.deltaPosition.magnitude * speedScale/ touch.deltaTime;
                            rigidbody.AddForce(diff.normalized * speed, ForceMode2D.Force); 
                        }
                    }
                    moveable = false;
                   break;

            }

        }



    }

    void detectTouch(Vector2 touchPos)
    {
        Vector3 pos = Camera.main.ScreenToWorldPoint(touchPos);
        Collider2D colObj = Physics2D.OverlapPoint(pos);

        if (colObj == GetComponent<Collider2D>())
        {
            Debug.Log("Collinding food");
            moveable = true;
            firstPos = touchPos;
            lastPos = touchPos;
            moveTimeCountDown = MoveTime;
        }

    }

}
