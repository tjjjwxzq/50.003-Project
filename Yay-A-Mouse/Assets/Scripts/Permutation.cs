using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Permutation : MonoBehaviour
{

    // Other classes can access this variable
    public List<string> chosenComboPublic = new List<string>();
    string[] FoodNamesFromController;
    GameObject mouseObject;
    // Start is called exactly once in the lifetime of the scripts
    void Start()
    {
    }


    // To generate random number. Number will be used to select a combo from all the possible combinations
    public static int RandomNumber(int min, int max)
    {
        System.Random random = new System.Random();
        return random.Next(min, max);
    }


    // Compute all the different food combo
    public static List<string> getfoodCombo()
    {
        List<string> listOfAllFood = new List<string>();

        // Get FoodName from FoodController.cs

        /*string[] FoodNamesFromController = FoodController.FoodNames;

        for (int i = 0; i < FoodNamesFromController.Length; i++)
        {
            listOfAllFood.Add(FoodNamesFromController[i]);
            Debug.Log(listOfAllFood[i]);
        }*/

        listOfAllFood.Add("Normal"); listOfAllFood.Add("Cheese"); listOfAllFood.Add("Bad"); listOfAllFood.Add("Peanut");

        /*listOfAllFood.Add("Carrot");
        listOfAllFood.Add("Oat");listOfAllFood.Add("Apple"); listOfAllFood.Add("Anchovy");
        listOfAllFood.Add("Bread"); listOfAllFood.Add("Seed");listOfAllFood.Add("Orange"); listOfAllFood.Add("Garlic");
        listOfAllFood.Add("Chocolate");*/

        IList<IList<string>> perms = Permutations(listOfAllFood);           // perms is abstract

        List<List<string>> permutation = new List<List<string>>();
        List<string> chosenCombo = new List<string>();

        // Randomly choose one
        int chosenNumber = RandomNumber(0, perms.Count);

        for (int i = 0; i < perms.Count; i++)
        {

            List<string> eachcombo = new List<string>();        // Need to create new list. Else, you're just editing the same one which will result in overwriting issues

            for (int j = 0; j < perms[0].Count; j++)
            {
                String element = perms[i][j];
                eachcombo.Add(element);

            }
            //permutation.Add(eachcombo);
            permutation.Insert(i, eachcombo);

            //Debug.Log("Permutation in i loop" + i + ":" + permutation[i][0] + permutation[i][1] + permutation[i][2]);
        }


        /*for (int k = 0; k < permutation.Count; k++) {
            Debug.Log("Permutation in k loop" + k+ ":" + permutation[k][0] + permutation[k][1] + permutation[k][2]);
        }*/

        chosenCombo = permutation[chosenNumber];
        //Debug.Log("Chosen Permutation:" + permutation[chosenNumber][0] + permutation[chosenNumber][1] + permutation[chosenNumber][2]);
        return chosenCombo;
    }

    private static IList<IList<T>> Permutations<T>(IList<T> list)
    {
        List<IList<T>> perms = new List<IList<T>>();
        if (list.Count == 0)
            return perms; // Empty list.
        int factorial = 1;
        for (int i = 2; i <= list.Count; i++)
            factorial *= i;
        for (int v = 0; v < factorial; v++)
        {
            List<T> s = new List<T>(list);
            int k = v;
            for (int j = 2; j <= list.Count; j++)
            {
                int other = (k % j);
                T temp = s[j - 1];
                s[j - 1] = s[other];
                s[other] = temp;
                k = k / j;
            }
            perms.Add(s);
        }
        return perms;
    }


    // To test the data type of any variable

    /*To test in the script
    TypeTester t = new TypeTester();
    t.printType(permutation); // At this point, eachCombo is byte*/

    public class TypeTester
    {
        public void printType(List<string> x)
        {
            Debug.Log(x + " is an List<string>");
        }
        public void printType(List<List<string>> x)
        {
            Debug.Log(x + " is an List<List<string>>");
        }
    }
}
