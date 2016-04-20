﻿using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class HowToPlayController : MonoBehaviour {

    private int currentSectionIndex;
    private GameObject[] sections;
    private Animator[] sectionAnimators;
    private Dictionary<string, Animator> sectionAnimatorsDict = new Dictionary<string, Animator>();
    private readonly string[] sectionNames = {"Introduction", "TastyTreats", "JunkFood", "Combo", "FrenzyMode", "Abilities", "ThatsAll"};

    void Start()
    {
        sections = GameObject.FindGameObjectsWithTag("Section");
        sectionAnimators = sections.Select(s => s.GetComponent<Animator>()).ToArray();
        foreach (Animator sectionAnimator in sectionAnimators)
        {
            sectionAnimatorsDict[sectionAnimator.gameObject.name] = sectionAnimator;
        }
        currentSectionIndex = 0;
        sectionAnimatorsDict[sectionNames[currentSectionIndex]].SetTrigger("Enter");

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
        if (sectionAnimatorsDict[sectionNames[currentSectionIndex]].GetCurrentAnimatorStateInfo(0).IsName("Default"))
        {
            sectionAnimatorsDict[sectionNames[currentSectionIndex]].SetTrigger("Exit");
            currentSectionIndex++;
            Invoke("AnimateEntry", 0.5f);
        }
    }

    // Invoke entry 1 second after exit
    private void AnimateEntry()
    {
        sectionAnimatorsDict[sectionNames[currentSectionIndex]].SetTrigger("Enter");
    }

    /// <summary>
    /// Callback when OK button is pressed
    /// </summary>
    public void OnOKButton()
    {
        SceneManager.LoadScene("Lobby");
    }

}
