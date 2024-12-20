using System.IO;
using UnityEngine;

public class CSVQuery
{
    private string csvFilePath;

    public CSVQuery(string filePath)
    {
        csvFilePath = filePath;
    }

    public FoodData QueryFoodData(int foodIndex)
    {
        using (StreamReader sr = new StreamReader(csvFilePath))
        {
            string line;
            sr.ReadLine(); // Skip headers
            int lineNum = 0;

            while ((line = sr.ReadLine()) != null)
            {
                string[] values = line.Split(',');
                if (lineNum == foodIndex)
                {
                    return new FoodData
                    {
                        Name = values[0],
                        Calories = float.Parse(values[1]),
                        Carbs = float.Parse(values[2]),
                        Protein = float.Parse(values[3]),
                        Fats = float.Parse(values[4])
                    };
                }
                lineNum++;
            }
        }
        return null; // Return null if food not found
    }
}
