using UnityEngine;
using System.Collections;

/// <summary>
/// Script to be calle on the end of the combo text animation
/// </summary>
public class ComboTextAnimation : MonoBehaviour {

    /// <summary>
    /// End of combo text animation handler
    /// </summary>
    public void OnComboTextEnd()
    {
        gameObject.GetComponent<Animator>().SetTrigger("ComboText");
        gameObject.SetActive(false);
    }
 
}
