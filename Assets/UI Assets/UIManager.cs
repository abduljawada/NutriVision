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
    [SerializeField] private GameObject cameraLabel;
    [SerializeField] private GameObject journalLabel;

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

    [Space]
    [Header("Basket ScrollView")]
    [SerializeField] private GameObject basketViewContent;
    public GameObject itemPrefab;

    [Space]
    [Header("Journal ScrollView")]
    [SerializeField] private GameObject journalViewContent;
    public GameObject entryPrefab;

    private CSVQuery csvQuery;
    public FoodManager foodManager;
    private BasketManager basketManager;
    private JournalManager journalManager;
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
        basketManager = new BasketManager(basketViewContent.transform, itemPrefab);
        journalManager = new JournalManager(journalViewContent.transform, itemPrefab, entryPrefab);
        panelManager = new PanelManager(basketPanel, journalPanel, cameraLabel, journalLabel);
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
        basketManager.UpdateBasket();
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
        // Step 1: Gather totals from foodManager
        var totals = foodManager.GetTotals(); // Ensure foodManager.GetTotals() returns an object with the required properties

        // Step 2: Gather food items from foodManager
        Dictionary<string, FoodItem> foodItemsMap = new Dictionary<string, FoodItem>();
        int counter = 1;
        foreach (var food in foodManager.GetFoodList()) // Ensure GetFoodList() returns a list of food items
        {
            foodItemsMap[$"food{counter}"] = new FoodItem
            {
                name = food.Name,
                quantity = food.Quantity,
                calories = food.Calories
            };
            counter++;
            Debug.Log($"UIManager added {food.Quantity} {food.Name} with {food.Calories} kCal");
        }
        Debug.Log($"foodItemsMap prepared: {JsonUtility.ToJson(foodItemsMap)}");

        // Step 3: Create a JournalEntry object
        JournalEntry journalEntry = new JournalEntry
        {
            timestamp = System.DateTime.UtcNow.ToString("o"), // ISO 8601 format for timestamp
            totalCalories = totals.totalCalories,
            totalProtien = totals.totalProtein,
            totalCarbs = totals.totalCarbs,
            totalFats = totals.totalFats,
            totalQuantity = (int)totals.totalQuantity,
            foodItems = foodItemsMap
        };
        Debug.Log($"JournalEntry prepared: {JsonUtility.ToJson(journalEntry)}");

        // Step 4: Save the JournalEntry to Firebase
        FirebaseManager.Instance.SaveJournalEntry(journalEntry);

        // Log success for debugging
        Debug.Log("Journal entry prepared and sent to Firebase.");
    }

    public void OnCameraNavButton()
    {
        panelManager.ShowCameraPanel();
    }

    public void OnJournalNavButton()
    {
        panelManager.ToggleJournalPanel();
        journalManager.FetchJournalEntries();
    }
    public void OnBasketNavButton() => panelManager.ToggleBasketPanel();
    public void OnProfileNavButton() => FirebaseManager.Instance.LogOut();
    

}
