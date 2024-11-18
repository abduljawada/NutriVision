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

    public int ItemCount { get; private set; } = 0;


    public void SelectFood(FoodData foodData)
    {
        selectedFood = foodData;
    }
    
    public void AddFood(FoodData foodData)
    {
        var existingFood = foodList.FirstOrDefault(food => food.Name == foodData.Name);

        if (existingFood != null)
        {
            existingFood.Calories += foodData.Calories;
            existingFood.Protein += foodData.Protein;
            existingFood.Carbs += foodData.Carbs;
            existingFood.Fats += foodData.Fats;
            existingFood.Quantity += foodData.Quantity;
        }
        else
        {
            foodList.Add(foodData);
        }

        UpdateItemCount();
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

            UpdateItemCount();
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

    public int getItemCount()
    {
        int totalCount = 0;

        foreach(FoodData food in foodList)
        {
            totalCount += food.Quantity;
        }
        
        return totalCount;
    }

    public void UpdateItemCount()
    {
        ItemCount = getItemCount();
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
