using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEditor;
using UnityEngine;

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
        // Find an existing food item in the list
        var existingFood = foodList.FirstOrDefault(food => food.Name == foodData.Name);

        if (existingFood != null)
        {
            // Increment the existing values
            existingFood.Calories += foodData.Calories;
            existingFood.Protein += foodData.Protein;
            existingFood.Carbs += foodData.Carbs;
            existingFood.Fats += foodData.Fats;
            existingFood.Quantity += foodData.Quantity;
        }
        else
        {
            // Add a new copy to the list
            foodList.Add(new FoodData
            {
                Name = foodData.Name,
                Calories = foodData.Calories,
                Protein = foodData.Protein,
                Carbs = foodData.Carbs,
                Fats = foodData.Fats,
                Quantity = foodData.Quantity
            });
        }
    }

    public void RemoveFood(string foodName)
    {
        var foodToRemove = foodList.Find(food => food.Name == foodName);
        if (foodToRemove != null)
        {   
            if (foodToRemove.Quantity == 1)
            {
                foodList.Remove(foodToRemove);
            }
            else
            {
                foodToRemove.Calories -= foodToRemove.Calories/foodToRemove.Quantity;
                foodToRemove.Protein -= foodToRemove.Protein/foodToRemove.Quantity;
                foodToRemove.Carbs -= foodToRemove.Carbs/foodToRemove.Quantity;
                foodToRemove.Fats -= foodToRemove.Fats/foodToRemove.Quantity;
                foodToRemove.Quantity--;
            }
        }
    }

    public (float totalCalories, float totalProtein, float totalCarbs, float totalFats, float totalQuantity) GetTotals()
    {
        float totalCalories = 0;
        float totalProtein = 0;
        float totalCarbs = 0;
        float totalFats = 0;
        float totalQuantity = 0;

        foreach (FoodData food in foodList)
        {
            totalCalories += food.Calories;
            totalProtein += food.Protein;
            totalCarbs += food.Carbs;
            totalFats += food.Fats;
            totalQuantity += food.Quantity;
        }

        return (totalCalories, totalProtein, totalCarbs, totalFats, totalQuantity);
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
