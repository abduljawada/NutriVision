using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine.SceneManagement;

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

    public void SaveJournalEntry(Dictionary<string, object> journalEntry)
    {
        string userId = AuthHandler.Instance?.user?.UserId;
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("User ID is null or empty! Ensure the user is authenticated.");
            return;
        }

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
