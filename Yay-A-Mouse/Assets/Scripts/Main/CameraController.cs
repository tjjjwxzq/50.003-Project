using UnityEngine;
using System.Collections;

/// <summary>
/// Script for controlling camera behavior
/// Stores global screen info such as the
/// default aspect ratio, pixels per unit,
/// and min. and max. x and y coordinates in Unity units
/// Camera viewport adapts to device screen size
/// and introduces pillarboxing/letterboxing to maintain
/// the correct aspect ratio
/// </summary>
public class CameraController : MonoBehaviour {

    private Camera camera;
    /// <summary>
    /// Default aspect ratio. Used to calculate whether to pillarbox or letterbox on different devices
    /// </summary>
    private static float defaultAspect = 2 / 3f;
    /// <summary>
    /// Pixels per unit, for referencing in other scripts
    /// </summary>
    public static float PixelsPerUnit;
    /// <summary>
    /// Smallest x coordinate, in Unity units, that fits into the screen
    /// </summary>
    public static float MinXUnits;
    /// <summary>
    /// Largest x coordinate, in Unity units, that fits into the screen
    /// </summary>
    public static float MaxXUnits;
    /// <summary>
    /// Smallest y coordinate, in Unity units, that fits into the screen
    /// </summary>
    public static float MinYUnits;
    /// <summary>
    /// Largest y coordinate, in Unity units, that fits into the screen
    /// </summary>
    public static float MaxYUnits;

	// Use this for initialization
	void Start () {

        PixelsPerUnit = Screen.height / (2f * Camera.main.orthographicSize);
        MinXUnits = -Screen.width / (2 * CameraController.PixelsPerUnit);
        MaxXUnits = Screen.width / (2 * CameraController.PixelsPerUnit);
        MinYUnits = -Screen.height / (2 * CameraController.PixelsPerUnit);
        MaxYUnits = Screen.height / (2 * CameraController.PixelsPerUnit);

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
