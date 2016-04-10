using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Linq;

public class HowToPlayController : MonoBehaviour {

    private int currentSectionIndex;
    private GameObject[] sections;
    private Animator[] sectionAnimators;

    void Start()
    {
        sections = GameObject.FindGameObjectsWithTag("Section");
        sectionAnimators = sections.Select(s => s.GetComponent<Animator>()).ToArray();
        currentSectionIndex = sections.Length - 1;
        sectionAnimators[currentSectionIndex].SetTrigger("Enter");

        // Destroy network manager in case it persists
        GameObject lobbyManager = GameObject.Find("NetworkLobbyManager");
        if (lobbyManager != null)
            Destroy(lobbyManager);

    }

    /// <summary>
    /// Callback when next section button is pressed
    /// </summary>
    public void OnNextButton()
    {
        sectionAnimators[currentSectionIndex].SetTrigger("Exit");
        currentSectionIndex--;
        Invoke("AnimateEntry", 0.5f);
    }

    // Invoke entry 1 second after exit
    private void AnimateEntry()
    {
        sectionAnimators[currentSectionIndex].SetTrigger("Enter");
    }

    /// <summary>
    /// Callback when OK button is pressed
    /// </summary>
    public void OnOKButton()
    {
        SceneManager.LoadScene("Lobby");
    }

}
