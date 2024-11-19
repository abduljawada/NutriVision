using UnityEngine;
using TMPro;
using System.IO;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    [SerializeField] private string csvFileName = "fruits_nutrition.csv";

    [Space]
    [Header("UI Panels")]
    [SerializeField] private GameObject basketPanel;
    [SerializeField] private GameObject journalPanel;

    [Space]
    [Header("Detection UI")]
    // Change Text to TextMeshProUGUI
    [SerializeField] private TextMeshProUGUI foodNameText;
    [SerializeField] private TextMeshProUGUI caloriesText;
    [SerializeField] private TextMeshProUGUI proteinText;
    [SerializeField] private TextMeshProUGUI carbsText;
    [SerializeField] private TextMeshProUGUI fatsText;

    [Space]
    [Header("Basket UI")]
    // UI elements for total nutritional info
    [SerializeField] private TextMeshProUGUI totalCaloriesText;
    [SerializeField] private TextMeshProUGUI totalProteinText;
    [SerializeField] private TextMeshProUGUI totalCarbsText;
    [SerializeField] private TextMeshProUGUI totalFatsText;
    [SerializeField] private TextMeshProUGUI totalQuantText;

    [SerializeField] private GameObject scrollViewContent;
    
    [SerializeField] private GameObject scrollViewElement;

    private CSVQuery csvQuery;
    private FoodManager foodManager;

    private Dictionary<string, GameObject> basketItems = new Dictionary<string, GameObject>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        Debug.Log(Instance.gameObject.name);
    }

    private void Start()
    {
        csvQuery = new CSVQuery(Path.Combine(Application.streamingAssetsPath, csvFileName));
        foodManager = new FoodManager();
    }

    public void OnFoodSelected(int foodIndex)
    {
        FoodData foodData = csvQuery.QueryFoodData(foodIndex);
        if (foodData != null)
        {
            foodManager.SelectFood(foodData);
            DisplayFoodData(foodData);
        }
        else
        {
            Debug.LogWarning(foodIndex + " is not found in CSV.");
        }
    }

    public void AddSelectedFood()
    {
        FoodData selectedFood = foodManager.getSelected();

        if(selectedFood != null)
        {
            foodManager.AddFood(selectedFood);
            UpdateUI();
        }

        //foodManager.AddFood(selectedFood);
        //UpdateTotalNutritionalValues();
        //AddItemToBasket(selectedFood);
    }

    public void RemoveFood(string foodName)
    {
        foodManager.RemoveFood(foodName);
        UpdateUI();
    }

    private void UpdateUI()
    {
        //Update total nutritional values
        var totals = foodManager.GetTotals();
        totalCaloriesText.text = $"{totals.totalCalories}\nkCal";
        totalProteinText.text = $"{totals.totalProtein}g";
        totalCarbsText.text = $"{totals.totalCarbs}g";
        totalFatsText.text = $"{totals.totalFats}g";
        totalQuantText.text = $"{totals.totalQuantity} Items";

        //Update basket UI
        foreach (var item in basketItems.Values)
        {
            Destroy(item);
        }
        basketItems.Clear();

        foreach (var food in foodManager.GetFoodList())
        {
            AddItemToBasket(food);
        }
    }

    private void DisplayFoodData(FoodData foodData)
    {
        var foodnameArabicFixer =  foodNameText.GetComponent<ArabicFixerTMPRO>();

        if (foodnameArabicFixer == null)
        {
            foodNameText.text = foodData.Name;
            caloriesText.text = $"{foodData.Calories}\nkCal";
            proteinText.text = $"Protein: {foodData.Protein}g";
            carbsText.text = $"Carbs: {foodData.Carbs}g";
            fatsText.text = $"Fats: {foodData.Fats}g";
        }
        else
        {
            foodnameArabicFixer.fixedText = foodData.Name;
            caloriesText.GetComponent<ArabicFixerTMPRO>().fixedText = $"{foodData.Calories}\nكالوري";
            proteinText.GetComponent<ArabicFixerTMPRO>().fixedText  = $"بروتينات {foodData.Protein}ج";
            carbsText.GetComponent<ArabicFixerTMPRO>().fixedText = $"كربوهيدرات {foodData.Carbs}ج";
            fatsText.GetComponent<ArabicFixerTMPRO>().fixedText = $"دهون {foodData.Fats}ج";
        }
    }

    //public void OnLogoutButtonPressed()
    //{
    //    Debug.Log("Logout button pressed.");
    //    if (AuthHandler.Instance != null)
    //    {
    //        Debug.Log("AuthHandler found.");
    //        AuthHandler.Instance.Logout();  // Directly call the Logout method on the Singleton
    //    }
    //    else
    //    {
    //        Debug.LogError("AuthHandler instance not found.");
    //        // You could show a fallback UI or try to reload the scene if needed
    //    }
    //}

    public void OnBasketButton()
    {
        Debug.Log("khara");
        basketPanel.SetActive(!basketPanel.activeSelf);
    }

    public void OnAddButton()
    {

    }

    public void AddItemToBasket(FoodData foodData)
    {
        GameObject instance = Instantiate(scrollViewElement, scrollViewContent.transform);
        ItemObjectScript script = instance.GetComponent<ItemObjectScript>();

        if(script != null)
        {
            script.nameLabel.text = foodData.Name;
            script.countLabel.text = $"{foodData.Quantity}x";
            script.caloriesLabel.text = $"{foodData.Calories} kCal";
        }

        basketItems[foodData.Name] = instance;
    }

}
