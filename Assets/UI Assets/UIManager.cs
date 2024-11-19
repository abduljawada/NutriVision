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
            proteinText.text = $"Protein\t{foodData.Protein}g";
            carbsText.text = $"Carbs\t{foodData.Carbs}g";
            fatsText.text = $"Fats\t\t{foodData.Fats}g";
        }
        else
        {
            foodnameArabicFixer.fixedText = foodData.Name;
            caloriesText.GetComponent<ArabicFixerTMPRO>().fixedText = $"{foodData.Calories}\nسعره";
            proteinText.GetComponent<ArabicFixerTMPRO>().fixedText  = $"بروتينات \t{foodData.Protein}ج";
            carbsText.GetComponent<ArabicFixerTMPRO>().fixedText = $"كربوهيدرات \t{foodData.Carbs}ج";
            fatsText.GetComponent<ArabicFixerTMPRO>().fixedText = $"دهون \t{foodData.Fats}ج";
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
        basketPanel.SetActive(!basketPanel.activeSelf);
    }

    public void OnAddJournalButtonClicked()
    {
        SaveJournalEntryToFirebase();
        ClearBasket();
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

    private void SaveJournalEntryToFirebase()
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

        string userId = AuthHandler.Instance?.user?.UserId;
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("User ID is null or empty! Ensure the user is authenticated.");
            return;
        }

        FirebaseManager.Instance.SaveJournalEntry(journalEntry, userId);
    }

    public void ClearBasket()
    {
        foodManager.ClearFoodList();
        UpdateUI();

        foreach (Transform child in scrollViewContent.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        basketItems.Clear();
    }

    public void OnJournalNavButton()
    {
        if(!journalPanel.activeSelf)
        {  
            journalPanel.SetActive(true);
            cameraPanel.SetActive(false);
            basketPanel.SetActive(false);
            toggleCameraIcon();
        }
        else
        {
            journalPanel.SetActive(false);
            cameraPanel.SetActive(true);
            toggleCameraIcon();
        }

        
    }

    private void toggleCameraIcon()
    {
        // Assume the icons are child objects under the cameraPanel
        Transform icon1 = cameraPanel.transform.GetChild(0); // First child icon
        Transform icon2 = cameraPanel.transform.GetChild(1); // Second child icon

        // Toggle visibility between icon1 and icon2
        bool isIcon1Active = icon1.gameObject.activeSelf;

        // Set icon1 to inactive and icon2 to active (or vice versa)
        icon1.gameObject.SetActive(!isIcon1Active); // If icon1 is active, deactivate it
        icon2.gameObject.SetActive(isIcon1Active);  // If icon1 was active, activate icon2
    }

}
