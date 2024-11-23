using System;
using System.Collections.Generic;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.UI;

public class JournalManager
{
    private Transform scrollViewContent;
    private GameObject entryPrefab;
    private GameObject itemPrefab;

    public JournalManager(Transform scrollViewContent, GameObject itemPrefab, GameObject entryPrefab)
    {
        this.scrollViewContent = scrollViewContent;
        this.entryPrefab = entryPrefab;
        this.itemPrefab = itemPrefab;
    }

    public void FetchJournalEntries()
    {
        FirebaseManager.Instance.GetAllJournalEntries(
            onSuccess: (entries) =>
            {
                Clear(); // Clear existing entries before adding new ones
                Debug.Log($"Retrieved {entries.Count} journal entries.");
                
                foreach (var entry in entries)
                {
                    DisplayEntry(entry);
                }

                // Force layout rebuild after all entries are added
                ForceLayoutRebuild(scrollViewContent);
            },
            onFailure: (error) =>
            {
                Clear(); // Clear the UI even if the fetch fails
                Debug.LogError($"Error fetching journal entries: {error}");
            }
        );
    }

    public void DisplayEntry(JournalEntry entry)
    {
        // Parse and format the timestamp
        DateTime parsedTimestamp = DateTime.Parse(entry.timestamp);
        string formattedTimestamp = parsedTimestamp.ToString("MMM dd, yyyy - hh:mm tt"); // Example: Nov 23, 2024 - 05:00 PM

        GameObject newEntry = UnityEngine.Object.Instantiate(entryPrefab, scrollViewContent);
        newEntry.GetComponent<EntryObjectScript>().SetEntryData(formattedTimestamp, ((int)entry.totalCalories).ToString());

        // Add food items to the entry
        if (entry.foodItems != null && entry.foodItems.Count > 0)
        {
            foreach (var foodItem in entry.foodItems.Values)
            {
                GameObject newItem = UnityEngine.Object.Instantiate(itemPrefab, newEntry.transform);
                newItem.GetComponent<ItemObjectScript>().SetItemData(foodItem.name, foodItem.quantity.ToString(), ((int)foodItem.calories).ToString());
                Debug.Log($"JournalManager retrieved: {foodItem.quantity} {foodItem.name}");
            }
        }
        else
        {
            Debug.LogWarning("No food items retrieved for this entry.");
        }
    }


    // Helper method to force layout rebuild
    private void ForceLayoutRebuild(Transform contentTransform)
    {
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentTransform.GetComponent<RectTransform>());
    }


    public void Clear()
    {
        foreach (Transform child in scrollViewContent)
        {
            GameObject.Destroy(child.gameObject); // Destroy each child
        }

        // Optional: Force Unity to update the layout after clearing
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(scrollViewContent.GetComponent<RectTransform>());
    }


}
