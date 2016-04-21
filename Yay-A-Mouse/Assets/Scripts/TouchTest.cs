using UnityEngine;
using System.Collections;

/// <summary>
/// Script for testing touch input
/// </summary>
public class TouchTest : MonoBehaviour {

    private GUIStyle GUIStyle = new GUIStyle();
	// Use this for initialization
	void Start () {
        Debug.Log("initializing");
        Debug.Log("Orthographics size is "+Camera.main.orthographicSize.ToString());
        Debug.Log("Screen resolution is " + Screen.currentResolution.ToString());
        Debug.Log("Screen height is " + Screen.height);
        Debug.Log("Screen width is " + Screen.width);
        Debug.Log("Screen dpi is " + Screen.dpi);

        GUIStyle.fontSize = 30;

        
	
	}
	
	// Update is called once per frame
	void Update () {
        
	}

    void OnGUI()
    {
        foreach(Touch touch in Input.touches)
        {
            string message = "";
            message += "ID: " + touch.fingerId + "\n";
            message += "Phase: " + touch.phase.ToString() + "\n";
            message += "TapCount: " + touch.tapCount + "\n";
            message += "PosX: " + touch.position.x + "\n";
            message += "PosY: " + touch.position.y + "\n";

            int num = touch.fingerId;
            GUI.Label(new Rect(130*num, 100, 500, 500), message, GUIStyle);

        }
    }
}
