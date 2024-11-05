using System;
using System.Collections.Generic;

public class FoodManager
{
    private FoodData selectedFood;
    private List<FoodData> foodList = new List<FoodData>();

    public void SelectFood(FoodData foodData)
    {
        selectedFood = foodData;
    }
    
    public void AddFood(FoodData foodData)
    {
        foodList.Add(foodData);
    }

    public void RemoveFood(string foodName)
    {
        FoodData foodToRemove = foodList.Find(food => food.Name == foodName);
        if (foodToRemove != null)
        {
            foodList.Remove(foodToRemove);
        }
    }

    public (float totalCalories, float totalProtein, float totalCarbs, float totalFats) GetTotalNutritionalValues()
    {
        float totalCalories = 0;
        float totalProtein = 0;
        float totalCarbs = 0;
        float totalFats = 0;

        foreach (FoodData food in foodList)
        {
            totalCalories += food.Calories;
            totalProtein += food.Protein;
            totalCarbs += food.Carbs;
            totalFats += food.Fats;
        }

        return (totalCalories, totalProtein, totalCarbs, totalFats);
    }

    public FoodData getSelected()
    {
        return selectedFood;
    }

    public List<FoodData> GetFoodList()
    {
        return foodList;
    }
}
