using UnityEngine;
using TMPro; // Import TextMeshPro namespace

public class UIManager : MonoBehaviour
{
    public string csvFilePath;
    
    // Change Text to TextMeshProUGUI
    public TextMeshProUGUI foodNameText;
    public TextMeshProUGUI caloriesText;
    public TextMeshProUGUI proteinText;
    public TextMeshProUGUI carbsText;
    public TextMeshProUGUI fatsText;

    // UI elements for total nutritional info
    public TextMeshProUGUI totalCaloriesText;
    public TextMeshProUGUI totalProteinText;
    public TextMeshProUGUI totalCarbsText;
    public TextMeshProUGUI totalFatsText;

    public TextMeshProUGUI foodListText;

    private CSVQuery csvQuery;
    private FoodManager foodManager;

    private void Start()
    {
        csvQuery = new CSVQuery(csvFilePath);
        foodManager = new FoodManager();
    }

    public void OnFoodSelected(string foodName)
    {
        Debug.LogWarning("BAWAW");
        FoodData foodData = csvQuery.QueryFoodData(foodName);
        if (foodData != null)
        {
            foodManager.SelectFood(foodData);
            DisplayFoodData(foodData);
        }
        else
        {
            Debug.LogWarning("Food item not found in CSV.");
        }
    }

    public void AddSelectedFood()
    {
        foodManager.AddFood(foodManager.getSelected());
        foodListText.text += "\n> " + foodManager.getSelected().Name + "             " + foodManager.getSelected().Calories + "kCal";
        UpdateTotalNutritionalValues();
    } 

    private void DisplayFoodData(FoodData foodData)
    {
        foodNameText.text = foodData.Name;
        caloriesText.text = $"Calories: {foodData.Calories} kCal";
        proteinText.text = $"Protein: {foodData.Protein}g";
        carbsText.text = $"Carbs: {foodData.Carbs}g";
        fatsText.text = $"Fats: {foodData.Fats}g";
    }

    private void UpdateTotalNutritionalValues()
    {
        var totals = foodManager.GetTotalNutritionalValues();
        totalCaloriesText.text = $"Total Calories: {totals.totalCalories} kCal";
        totalProteinText.text = $"Total Protein: {totals.totalProtein}g";
        totalCarbsText.text = $"Total Carbs: {totals.totalCarbs}g";
        totalFatsText.text = $"Total Fats: {totals.totalFats}g";
    }

    public void RemoveFood(string foodName)
    {
        foodManager.RemoveFood(foodName);
        UpdateTotalNutritionalValues(); // Update totals after removing
    }
}
