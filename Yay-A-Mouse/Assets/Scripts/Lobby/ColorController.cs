using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// One instance of this component exists in the lobby
/// scene on each client, and is used to store
/// the list of colors and used colors, since 
/// these should be globablly accessible from
/// the scripts of each LobbyPlayer game object
/// </summary>
public class ColorController : MonoBehaviour {

    // List of possible player colors
    public static Color[] Colors = { Color.red, Color.green, Color.blue, Color.yellow};
    public static List<Color> UsedColors = new List<Color>();

}
