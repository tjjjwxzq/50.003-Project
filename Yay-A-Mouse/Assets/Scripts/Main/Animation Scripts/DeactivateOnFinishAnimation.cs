using UnityEngine;
using System.Collections;

public class DeactivateOnFinishAnimation: MonoBehaviour
{
    public void OnAnimationEnd()
    {
        gameObject.SetActive(false);
    }
}

	