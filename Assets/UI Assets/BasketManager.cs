using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BasketManager
 {
    private Dictionary<string, GameObject> basketItems = new Dictionary<string, GameObject>();
    private Transform scrollViewContent;
    private GameObject scrollViewElement;

    public BasketManager(Transform scrollViewContent, GameObject scrollViewElement)
    {
        this.scrollViewContent = scrollViewContent;
        this.scrollViewElement = scrollViewElement;

    }

        // Function to clear and repopulate the basket
    public void UpdateBasket()
    {
        // Step 1: Clear the basket by destroying all existing items
        Clear();

        var foodList = UIManager.Instance.foodManager.GetFoodList();

        // Step 2: Re-add all items from the foodManager's foodList to the basket
        foreach (var foodData in foodList)
        {
            AddItem(foodData, scrollViewElement);
        }
    }

    // Function to add a food item to the basket UI
    private void AddItem(FoodData foodData, GameObject scrollViewElement)
    {
        GameObject newItem = UnityEngine.Object.Instantiate(scrollViewElement, scrollViewContent);
        newItem.name = foodData.Name;

        // Get the ItemObjectScript on the new item and update labels
        ItemObjectScript itemScript = newItem.GetComponent<ItemObjectScript>();
        itemScript.SetItemData(foodData);

        // Optionally: Update your basketItems dictionary (if you're keeping track)
        basketItems[foodData.Name] = newItem;
    }


    public void RemoveItem(string foodName)
    {
        if (basketItems.ContainsKey(foodName))
        {
            GameObject item = basketItems[foodName];
            GameObject.Destroy(item);
            basketItems.Remove(foodName);
        }
    }

    public void Clear()
    {
        foreach (Transform child in scrollViewContent)
        {
            GameObject.Destroy(child.gameObject);
        }
    }
}
