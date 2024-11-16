using UnityEngine;
using TMPro;
using System.IO;

public class UIManager : MonoBehaviour
{
    [SerializeField] private string csvFileName = "fruits_nutrition.csv";

    // Change Text to TextMeshProUGUI
    [SerializeField] private TextMeshProUGUI foodNameText;
    [SerializeField] private TextMeshProUGUI caloriesText;
    [SerializeField] private TextMeshProUGUI proteinText;
    [SerializeField] private TextMeshProUGUI carbsText;
    [SerializeField] private TextMeshProUGUI fatsText;

    // UI elements for total nutritional info
    [SerializeField] private TextMeshProUGUI totalCaloriesText;
    [SerializeField] private TextMeshProUGUI totalProteinText;
    [SerializeField] private TextMeshProUGUI totalCarbsText;
    [SerializeField] private TextMeshProUGUI totalFatsText;

    [SerializeField] private TextMeshProUGUI foodListText;

    private CSVQuery csvQuery;
    private FoodManager foodManager;

    private void Start()
    {
        csvQuery = new CSVQuery(Path.Combine(Application.streamingAssetsPath, csvFileName));
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
            Debug.LogWarning(foodName + " is not found in CSV.");
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
