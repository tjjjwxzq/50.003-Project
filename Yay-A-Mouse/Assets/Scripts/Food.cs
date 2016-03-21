using UnityEngine;
using System.Collections;

public class Food: MonoBehaviour {

    private Rigidbody2D rigidbody;
    private SpriteRenderer spriteRenderer;
    private float speedScale = 0.3f; //factor to scale touch swipe speed by; hand-tune this or linear drag
    public int NutritionalValue; //<! Nutritional value, equivalent to units of mouse weight (aka points)
    public string Type; //<! Food type
    private bool moveable = false; // moveable if it has been touched

    // Factory method
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
        detectMouseSwipe();

        // Touch input
        detectTouchSwipe();

        // Check if food is out of screen; if so return it to object pool
        if(gameObject.GetComponent<PoolMember>() != null)
        {
            if(spriteRenderer.bounds.min.x > CameraController.MaxXUnits || spriteRenderer.bounds.max.x < CameraController.MinXUnits)
            {
                gameObject.GetComponent<PoolMember>().Deactivate();
            }
            
            if(spriteRenderer.bounds.min.y > CameraController.MaxYUnits || spriteRenderer.bounds.max.y < CameraController.MinYUnits)
            {
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
                        Vector2 direction = touch.deltaPosition.normalized;
                        float speed = touch.deltaPosition.magnitude*speedScale / touch.deltaTime;
                        rigidbody.AddForce(direction*speed, ForceMode2D.Force);
                    }
                    break;

                case TouchPhase.Ended:
                    moveable = false;
                    break;

            }

        }



    }

    void detectTouch(Vector3 pos)
    {
        Vector3 wpos = Camera.main.ScreenToWorldPoint(pos);
        Vector2 touchPos = new Vector2(wpos.x, wpos.y);
        Collider2D colObj = Physics2D.OverlapPoint(touchPos);

        if (colObj == GetComponent<Collider2D>())
        {
            moveable = true;
        }

    }

}
