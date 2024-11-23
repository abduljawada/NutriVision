using System;
using System.Collections.Generic;

[Serializable]
public class FoodItem
{
    public string name;
    public int quantity;
    public float calories;
}

[Serializable]
public class JournalEntry
{
    public string timestamp;
    public float totalCalories;
    public float totalCarbs;
    public float totalFats;
    public float totalProtien;
    public int totalQuantity;
    public Dictionary<string, FoodItem> foodItems;
}
