using UnityEngine;
using System.Collections;

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
