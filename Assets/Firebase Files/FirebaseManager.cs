using System;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;


public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager Instance { get; private set; }
    private DatabaseReference dbReference;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                dbReference = FirebaseDatabase.DefaultInstance.RootReference;
                Debug.Log("Firebase is ready.");
            }
            else
            {
                Debug.LogError($"Could not resolve all Firebase dependencies: {task.Result}");
            }
        });
    }

    public void SaveJournalEntry(JournalEntry entry)
    {
        string userId = AuthHandler.Instance?.user?.UserId;
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("User ID is null or empty! Ensure the user is authenticated.");
            return;
        }

        string key = dbReference.Child("users").Child(userId).Child("journalEntries").Push().Key;
        string json = JsonConvert.SerializeObject(entry); // Use Newtonsoft.Json for serialization

        Debug.Log($"Serialized JournalEntry with Newtonsoft.Json: {json}");

        dbReference.Child("users").Child(userId).Child("journalEntries").Child(key).SetRawJsonValueAsync(json).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("Journal entry saved successfully.");
            }
            else
            {
                Debug.LogError($"Failed to save journal entry: {task.Exception}");
            }
        });
    }

    public void GetAllJournalEntries(Action<List<JournalEntry>> onSuccess, Action<string> onFailure)
    {
        string userId = AuthHandler.Instance?.user?.UserId;
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("User ID is null or empty! Ensure the user is authenticated.");
            onFailure?.Invoke("User not authenticated.");
            return;
        }

        dbReference.Child("users").Child(userId).Child("journalEntries").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                if (task.Result.Exists)
                {
                    List<JournalEntry> entries = new List<JournalEntry>();

                    foreach (var childSnapshot in task.Result.Children)
                    {
                        // Get raw JSON value for the entry
                        string json = childSnapshot.GetRawJsonValue();
                        Debug.Log($"Raw JSON from Firebase: {json}");

                        // Deserialize the entry
                        JournalEntry entry = JsonConvert.DeserializeObject<JournalEntry>(json);
                        
                        if (entry.foodItems != null)
                        {
                            Debug.Log($"Deserialized {entry.foodItems.Count} food items for entry.");
                        }
                        else
                        {
                            Debug.LogWarning("foodItems field is null after deserialization.");
                        }

                        entries.Add(entry);
                    }

                    onSuccess?.Invoke(entries);
                }
                else
                {
                    Debug.LogWarning("No journal entries found.");
                    onSuccess?.Invoke(new List<JournalEntry>());
                }
            }
            else
            {
                Debug.LogError($"Failed to retrieve journal entries: {task.Exception}");
                onFailure?.Invoke(task.Exception?.ToString());
            }
        });
    }


    public void LogOut()
    {
        Debug.Log("Logout button pressed.");
        if (AuthHandler.Instance != null)
        {
            Debug.Log("AuthHandler found.");
            AuthHandler.Instance.Logout();  // Directly call the Logout method on the Singleton
        }
        else
        {
            Debug.LogError("AuthHandler instance not found.");
            SceneManager.LoadScene("Authentication");
            // You could show a fallback UI or try to reload the scene if needed
        }
    }
        
}
