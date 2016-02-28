using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

    private Camera camera;
    private static float defaultAspect = 2 / 3f; // default aspect ratio

	// Use this for initialization
	void Start () {
        camera = GetComponent<Camera>();
        float width = Screen.width;
        float height = Screen.height;
        float aspectRatio = Mathf.Round(width / height*100)/100f;
        float scaleheight = aspectRatio / defaultAspect; //scale height by this amount

        if(aspectRatio < defaultAspect) // require letterboxing(horizontal)
        {
            Rect rect = camera.rect;

            rect.width = 1f;
            rect.height = scaleheight;
            rect.x = 0;
            rect.y = (1 - scaleheight) / 2;
            Debug.Log("Letterboxing");

            camera.rect = rect;
        }
        else // require pillarboxing (vertical)
        {
            Rect rect = camera.rect;

            rect.width = 1 / scaleheight;
            rect.height = 1f;
            rect.x = (1 - 1 / scaleheight) / 2;
            rect.y = 0;

            Debug.Log("pillarboxing");
            camera.rect = rect;

        }
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
