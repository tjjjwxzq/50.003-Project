using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Script to be called on the end of the countdown text animation
/// </summary>
public class CountdownTextAnimation : MonoBehaviour
{
    private int numSeconds = 3;
    private Text countdownText;
    private Animator countdownAnimator;

    public void Start()
    {
        countdownText = gameObject.GetComponent<Text>();
        countdownAnimator = gameObject.GetComponent<Animator>();
    }
    // When animation ends after one second
    public void OnNextSecond()
    {
        if(numSeconds > 1)
        {
            numSeconds--;
            countdownText.text = numSeconds.ToString();
            countdownAnimator.Play("ReadyCountdown", 0, 0f);
        }
    }
}

