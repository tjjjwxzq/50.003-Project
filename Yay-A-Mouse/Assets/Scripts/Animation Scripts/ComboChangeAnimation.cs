using UnityEngine;
using System.Collections;

public class ComboChangeAnimation : MonoBehaviour
{

    public void OnComboChanged()
    {
        gameObject.GetComponent<Animator>().SetTrigger("ComboChange");
        gameObject.SetActive(false);
    }

}	
