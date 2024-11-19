using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;

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

    public void SaveJournalEntry(Dictionary<string, object> journalEntry, string userId)
    {
        string key = dbReference.Child("users").Child(userId).Child("journalEntries").Push().Key;
        dbReference.Child("users").Child(userId).Child("journalEntries").Child(key).SetValueAsync(journalEntry).ContinueWithOnMainThread(task =>
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
}
