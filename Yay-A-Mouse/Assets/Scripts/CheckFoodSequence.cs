using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;

public class CheckFoodSequence : MonoBehaviour {


    // Status of game
    //public NormalTap normalTap;
    //public FrenzyModeScript frenzyTap;

    // Playing Mode - Normal/Frenzy - Get From
    bool NormalMode = true;
    bool FrenzyMode = false;

    // GUI - Text to be shown on screen
    public Text scoreDisplay;
    public Text streakCountDisplay;
    public Text frenzyModeTitle;

    public int countStreak;     // Count number of combinations that player get correct                                     
    public int sequenceFed;     // To count the number of correct food fed
    public int orderNumber;


    // Variables for generated array
    List<string> playerSwipedCombo = new List<string>();
    List<string> currentCombinationToSwipe = new List<string>();
    List<string> tester = new List<string>();

    // GUI - Images
    public Image SeqBox1;
    public Image SeqBox2;
    public Image SeqBox3;



    // Timer
    float time = 30.0f;
    private float timeout;
    public System.DateTime startTime;              // Need to be global

    void Start()
    {
        // Get combination list
        currentCombinationToSwipe = Permutation.getfoodCombo();
        Debug.Log("Feed Hamster Start() : " + currentCombinationToSwipe[0] + currentCombinationToSwipe[1] + currentCombinationToSwipe[2]);

        NormalMode = true;

        if (NormalMode == true)
        {
            // Disable frenzyTapping
            GameObject.FindGameObjectWithTag("Mouse").GetComponent<NormalMode>().enabled = true;
            GameObject.FindGameObjectWithTag("Mouse").GetComponent<FrenzyMode>().enabled = false;

            // Intialise Variables
            countStreak = 0;
            orderNumber = 0;

            // Starting Combination - Show on the screen
            SeqBox1.sprite = Resources.Load<Sprite>(currentCombinationToSwipe[0]) as Sprite;
            SeqBox2.sprite = Resources.Load<Sprite>(currentCombinationToSwipe[1]) as Sprite;
            SeqBox3.sprite = Resources.Load<Sprite>(currentCombinationToSwipe[2]) as Sprite;
        }
    }

    /*void Update()
    {
        // Check for the modes here
        if (NormalMode == true)
        {
            GameObject.FindGameObjectWithTag("Mouse").GetComponent<NormalTap>().enabled = true;
            GameObject.FindGameObjectWithTag("Mouse").GetComponent<FrenzyMode>().enabled = false;
        }
        else if (FrenzyMode == true)
        {
            GameObject.FindGameObjectWithTag("Mouse").GetComponent<NormalTap>().enabled = false;
            GameObject.FindGameObjectWithTag("Mouse").GetComponent<FrenzyMode>().enabled = true;
        }
    }*/

    // When collide with object (hamster)
    void OnTriggerEnter(Collider collider)
    {
        playerSwipedCombo.Add(collider.gameObject.name);                // Getting the name of object
        Debug.Log("Food Fed:" + collider.gameObject.name);

        scoreDisplay.text = (int.Parse(scoreDisplay.text) + 1) + "";    // Convert to int. Add "" to convert back to string


        // If player fed 3 food to hamster
        if (playerSwipedCombo.Count == 3)
        {

            // Check sequence then change

            // Compare playerSwipedCombo with comboGenerated[ordernumber][i];
            for (int i = 0; i < playerSwipedCombo.Count; i++)
            {
                if (playerSwipedCombo[i].Equals(currentCombinationToSwipe[i]))
                {
                    sequenceFed = sequenceFed + 1;

                }

            }

            // Get another sequence
            currentCombinationToSwipe = Permutation.getfoodCombo();

            // Increase order number so that you can display the next combo
            orderNumber = orderNumber + 1;

            // Change combo picture
            SeqBox1.sprite = Resources.Load<Sprite>(currentCombinationToSwipe[0]) as Sprite;
            SeqBox2.sprite = Resources.Load<Sprite>(currentCombinationToSwipe[1]) as Sprite;
            SeqBox3.sprite = Resources.Load<Sprite>(currentCombinationToSwipe[2]) as Sprite;


            // Clear array that holds user food input
            playerSwipedCombo.Clear();

            // Check if player feed correctly
            if (sequenceFed == 3)
            {
                sequenceFed = 0;
                countStreak = countStreak + 1;

                // Display Total Count Streak on the screen
                streakCountDisplay.text = countStreak + "";


                // Check if use qualify to enter frenzy mode
                if (countStreak == 3)
                {
                    //frenzyModeTitle.text = "Frenzy Mode!";
                    countStreak = 0;
                    // Start Timer
                    startTime = System.DateTime.Now;

                    FrenzyMode = true;

                    Debug.Log("In FRENZYYY");

                    /*while (FrenzyMode = true) {
                        
                        FrenzyMode = GameObject.Find("Mouse").GetComponent<FrenzyModeScript>();
                        FrenzyModeScript goToFrenzyMode = gameObject.GetComponent<FrenzyModeScript>();
                        goToFrenzyMode.insideFrenzyMode();
                    }*/

                    countStreak = 0;




                    Debug.Log("Back in normal mode");
                }
            }
            else {
                countStreak = 0;
                streakCountDisplay.text = countStreak + "";
            }


        }
        //Debug.Log("CountStreak:" + countStreak);
        // If error in feeding hamster, countstreak will drop to 0

    }

    // When you call a function, it runs to completion before returning.


    /*void goToFrenzyOld()
    {



        Debug.Log("Inside gotoFrenzy");
        countStreak = 0;            // Reset count streak

        // After seconds of frenzy mode
        if (System.DateTime.Now.Subtract(startTime).Seconds < 2) {                                  // .seconds : To get the time in seconds
                                                                                                       // Set Playing Mode

            GameObject.FindGameObjectWithTag("Mouse").GetComponent<NormalTap>().enabled = false;
            GameObject.FindGameObjectWithTag("Mouse").GetComponent<FrenzyModeScript>().enabled = true;

            Debug.Log("End of streak");
        }
        
        GameObject.FindGameObjectWithTag("Mouse").GetComponent<NormalTap>().enabled = true;
        GameObject.FindGameObjectWithTag("Mouse").GetComponent<FrenzyModeScript>().enabled = false;

        //frenzyModeTitle.text = ""; // problem!


    }*/
}
