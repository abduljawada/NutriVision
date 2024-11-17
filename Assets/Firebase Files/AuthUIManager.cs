using UnityEngine;

public class AuthUIManager : MonoBehaviour
{
    public GameObject loginPanel;
    public GameObject registrationPanel;

    // Set initial visibility
    void Start()
    {
        loginPanel.SetActive(true);          // Show the login panel initially
        registrationPanel.SetActive(false);  // Hide the registration panel initially
    }

    // Toggle panels
    public void ShowPanel(GameObject panelToShow, GameObject panelToHide)
    {
        panelToShow.SetActive(true);
        panelToHide.SetActive(false);
    }

    // Button methods
    public void BackButton()
    {
        ShowPanel(loginPanel, registrationPanel);
    }

    public void RegistrationButton()
    {
        ShowPanel(registrationPanel, loginPanel);
    }

    public void RegisterButton()
    {
        // Add functionality for registering a user here
    }
}
