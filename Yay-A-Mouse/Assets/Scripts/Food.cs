using UnityEngine;
using System.Collections;

public class Food: MonoBehaviour {

    private Rigidbody2D rigidbody;
    private float speedScale = 0.3f; //hand-tune this or linear drag; right now linear drag of 4 seems ok
    public int value = 5; //nutritional value, equivalent to 5 units of mouse weight (aka 5 points)
    private bool moveable = false; // moveable if it has been touched

	// Use this for initialization
	void Start () {
        rigidbody = GetComponent<Rigidbody2D>();
	
	}
	
	// Update is called once per frame
	void Update () {
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

            }

        }
        
	
	}

    void detectTouch(Vector3 pos)
    {
        Vector3 wpos = Camera.main.ScreenToWorldPoint(pos);
        Vector2 touchPos = new Vector2(wpos.x, wpos.y);
        Collider2D colObj = Physics2D.OverlapPoint(touchPos);

        if(colObj != null)
        {
            Debug.Log("I was touched!");
            moveable = true;
        }

    }
}
