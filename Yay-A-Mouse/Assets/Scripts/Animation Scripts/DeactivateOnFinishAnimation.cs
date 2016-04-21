using UnityEngine;
using System.Collections;

/// <summary>
/// Script that deactivates Game Object on the end of its animation
/// </summary>
public class DeactivateOnFinishAnimation: MonoBehaviour
{
    public void OnAnimationEnd()
    {
        gameObject.SetActive(false);
    }
}

	