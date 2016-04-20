using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class NormalMode : MonoBehaviour
{

    private RaycastHit hit;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        //Text score = GetComponentInChildren<FeedHamster>().scoreDisplay; // if you need a variable of it or method you can access it like this


        // Input.GetMouseButtonDown(0): For anywhere in the screen. Tag "mouse" if you hit the mouse only
        if (Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                if (hit.collider.tag == "Mouse")
                {
                    //score.text = (int.Parse(score.text) + 1) + "";
                    //Debug.Log(score.text);
                    Debug.Log("hit");
                }
            }
        }
    }


}
