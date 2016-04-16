using UnityEngine;
using System.Collections;

public class FeedingFrenzyAnimation : MonoBehaviour
{
    public void OnFeedFrenzyAnimationEnd()
    {
        gameObject.SetActive(false);
    }
}

	