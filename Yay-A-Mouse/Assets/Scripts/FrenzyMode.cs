using UnityEngine;
using UnityEngine.UI;
using System.Collections;

// DeactivateController : deactivate food
// ActivateController : activate food
// In frenzy mode, when there's tapping, shoot objects into the mouse
// Add into JQ's level controller 

public class FrenzyMode: MonoBehaviour
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
        // bool inFrenzyMode = GetComponentInChildren<Score>().inFrenzyMode;


        // Input.GetMouseButtonDown(0): For anywhere in the screen. Tag "mouse" if you hit the mouse only
        if (Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                if (hit.collider.tag == "Mouse")
                {
                    Debug.Log("Frenzy");
                    // Set the mouse weight to increase
                    //score.text = (int.Parse(score.text) + 10) + "";
                    //Debug.Log(score.text);
                }
            }
        }
    }


}
