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
    [SerializeField] private GameObject cameraPanel;
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

    [Space]
    [Header("Basket ScrollView")]
    
    public GameObject scrollViewElement;

    private CSVQuery csvQuery;
    public FoodManager foodManager;

    private BasketManager basketManager;
    private PanelManager panelManager;

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
    }

    private void Start()
    {
        csvQuery = new CSVQuery(Path.Combine(Application.streamingAssetsPath, csvFileName));
        foodManager = new FoodManager();
        basketManager = new BasketManager(scrollViewContent.transform, scrollViewElement);
        panelManager = new PanelManager(cameraPanel, basketPanel, journalPanel);
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

    private void DisplayFoodData(FoodData foodData)
    {
        var foodnameArabicFixer =  foodNameText.GetComponent<ArabicFixerTMPRO>();

        if (foodnameArabicFixer == null)
        {
            foodNameText.text = foodData.Name;
            caloriesText.text = $"{foodData.Calories}\nkCal";
            proteinText.text = $"Protein\t{foodData.Protein}g";
            carbsText.text = $"Carbs\t{foodData.Carbs}g";
            fatsText.text = $"Fats\t\t{foodData.Fats}g";
        }
        else
        {
            foodnameArabicFixer.fixedText = foodData.Name;
            caloriesText.GetComponent<ArabicFixerTMPRO>().fixedText = $"{foodData.Calories}\nسعرة";
            proteinText.GetComponent<ArabicFixerTMPRO>().fixedText  = $"بروتينات {foodData.Protein}ج";
            carbsText.GetComponent<ArabicFixerTMPRO>().fixedText = $"كربوهيدرات {foodData.Carbs}ج";
            fatsText.GetComponent<ArabicFixerTMPRO>().fixedText = $"دهون {foodData.Fats}ج";
        }
    }

    public void AddSelectedFood()
    {
        FoodData selectedFood = foodManager.getSelected();

        if(selectedFood != null)
        {
            foodManager.AddFood(selectedFood);
            basketManager.UpdateBasket();
            UpdateTotalUI();
        }
    }

    public void RemoveFood(string foodName) 
    {
        foodManager.RemoveFood(foodName);
        //basketManager.RemoveItem(foodName); //Not Implemented
        UpdateTotalUI();
    }

    public void OnAddJournalButtonClicked()
    {
        SaveEntryToFirebase();
        foodManager.ClearFoodList();
        basketManager.Clear();
        UpdateTotalUI();
    }

    private void UpdateTotalUI()
    {
        //Update total nutritional values
        var totals = foodManager.GetTotals();
        totalCaloriesText.text = $"{totals.totalCalories}\nkCal";
        totalProteinText.text = $"{totals.totalProtein}g";
        totalCarbsText.text = $"{totals.totalCarbs}g";
        totalFatsText.text = $"{totals.totalFats}g";
        totalQuantText.text = $"{totals.totalQuantity} Items";
    }

    private void SaveEntryToFirebase()
    {
        var totals = foodManager.GetTotals();
        List<Dictionary<string,object>> foodList = new List<Dictionary<string, object>>();

        foreach (var food in foodManager.GetFoodList())
        {
            foodList.Add(new Dictionary<string, object>
            {
                { "name", food.Name },
                { "quantity", food.Quantity },
                { "calories", food.Calories }
            });
        }

        Dictionary<string, object> journalEntry = new Dictionary<string, object>
        {
            {"timestamp", System.DateTime.UtcNow.ToString("o")},
            {"totalCalories", totals.totalCalories},
            {"totalProtien", totals.totalProtein},
            {"totalCarbs", totals.totalCarbs},
            {"totalFats", totals.totalFats},
            {"totalQuantity", totals.totalQuantity},
            {"foodItems", foodList}
        };

        FirebaseManager.Instance.SaveJournalEntry(journalEntry);
    }

    public void OnBasketNavButton() => panelManager.ToggleBasketPanel();
    public void OnJournalNavButton() => panelManager.ToggleJournalPanel();
    public void OnProfileNavButton() => FirebaseManager.Instance.LogOut();

}
