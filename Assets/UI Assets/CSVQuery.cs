using System.IO;
using UnityEngine;

public class CSVQuery
{
    private string csvFilePath;

    public CSVQuery(string filePath)
    {
        csvFilePath = filePath;
    }

    public FoodData QueryFoodData(string foodName)
    {
        using (StreamReader sr = new StreamReader(csvFilePath))
        {
            string line;
            sr.ReadLine(); // Skip headers

            while ((line = sr.ReadLine()) != null)
            {
                string[] values = line.Split(',');
                if (values[0].Trim().Equals(foodName, System.StringComparison.OrdinalIgnoreCase))
                {
                    return new FoodData
                    {
                        Name = values[0],
                        Calories = float.Parse(values[1]),
                        Protein = float.Parse(values[2]),
                        Carbs = float.Parse(values[3]),
                        Fats = float.Parse(values[4])
                    };
                }
            }
        }
        return null; // Return null if food not found
    }
}
