using UnityEngine;
using UnityEngine.UI;

public class MainMenuButton : MonoBehaviour {

    Text buttonText;

    // Use this for initialization
    void Start () {
        buttonText = transform.FindChild("Text").GetComponent<Text>();
    }
}
