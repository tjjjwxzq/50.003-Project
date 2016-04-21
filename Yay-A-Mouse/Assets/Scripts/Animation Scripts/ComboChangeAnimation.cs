using UnityEngine;
using System.Collections;

/// <summary>
/// Script to be called on the end of the combo change animation
/// </summary>
public class ComboChangeAnimation : MonoBehaviour
{

    public void OnComboChanged()
    {
        gameObject.GetComponent<Animator>().SetTrigger("ComboChange");
        gameObject.SetActive(false);
    }

}	
