using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI; // Required for UI elements
using TMPro;

public class QueryScript : MonoBehaviour
{
    public TMP_InputField inputField;  // UI Input Field for fruit name
    public TMP_Text outputText;        // UI Text to display the result

    private string csvPath;
    private float initialShowTime = 5.0f;
    private float showTime;

    // Fruit class to store nutritional information
    public class Fruit
    {
        public string name;
        public int calories;
        public float carbohydrates;
        public float protein;
        public float fat;

        public Fruit(string name, int calories, float carbohydrates, float protein, float fat)
        {
            this.name = name;
            this.calories = calories;
            this.carbohydrates = carbohydrates;
            this.protein = protein;
            this.fat = fat;
        }

        // Method to convert the Fruit object to a formatted string
        public override string ToString()
        {
            return $"Name: {name}\nCalories: {calories}\nCarbohydrates: {carbohydrates} g\nProtein: {protein} g\nFat: {fat} g";
        }
    }

    void Start()
    {
        // Define the path to the CSV file
        csvPath = Path.Combine(Application.streamingAssetsPath, "fruits_nutrition.csv");
    }

    private void Update()
    {
        showTime -= Time.deltaTime;
        if (showTime <= 0)
        {
            ClearText();
        }
    }

    private void ClearText()
    {
        outputText.text = string.Empty;
    }

    // Function to query a fruit from the CSV file by its name and return a formatted string
    public void QueryFruitAndDisplay()
    {
        string fruitName = inputField.text; // Get the input from the UI
        Fruit fruit = GetFruitByName(fruitName);

        if (fruit != null)
        {
            // Display the fruit's nutritional info as a string
            outputText.text = fruit.ToString();
        }
        else
        {
            outputText.text = "Fruit not found in the database.";
        }
    }

    public void QueryFruitAndDisplay(string fruitName)
    {
        fruitName = fruitName.ToLower().Trim();
        Fruit fruit = GetFruitByName(fruitName);

        if (fruit != null)
        {
            // Display the fruit's nutritional info as a string
            outputText.text = fruit.ToString();
        }
        else
        {
            outputText.text = "Fruit not found in the database.";
        }
    }

    // Function to query a fruit from the CSV file by its name
    Fruit GetFruitByName(string name)
    {
        if (File.Exists(csvPath))
        {
            using (StreamReader reader = new StreamReader(csvPath))
            {
                string line;
                bool isFirstLine = true; // Skip header

                while ((line = reader.ReadLine()) != null)
                {
                    if (isFirstLine)
                    {
                        isFirstLine = false;
                        continue;
                    }

                    string[] values = line.Split(',');

                    if (values.Length == 5)
                    {
                        string fruitName = values[0];

                        if (fruitName.Equals(name, StringComparison.OrdinalIgnoreCase))
                        {
                            int calories = int.Parse(values[1]);
                            float carbohydrates = float.Parse(values[2]);
                            float protein = float.Parse(values[3]);
                            float fat = float.Parse(values[4]);

                            return new Fruit(fruitName, calories, carbohydrates, protein, fat);
                        }
                    }
                }
            }
        }
        else
        {
            Debug.LogError("CSV file not found at path: " + csvPath);
        }

        return null;
    }
}
