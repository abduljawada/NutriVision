using UnityEngine;

public class AppleScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void DetectApple(){
        Debug.Log("Apple Detected");
        UIManager.Instance.OnFoodSelected("Apple");

    }

    public void DetectOrange(){
        Debug.Log("Orange Detected");
        UIManager.Instance.OnFoodSelected("Orange");

    }
}
