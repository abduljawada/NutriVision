using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AuthUIManager : MonoBehaviour
{
    public static AuthUIManager Instance { get; private set; }  // Singleton instance

    public GameObject loginPanel;
    public GameObject registrationPanel;

    private void Awake()
    {
        // Ensure only one instance of AuthUIManager exists
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);  // Destroy duplicate instance
        }
    }

    // Set initial visibility
    void Start()
    {
        registrationPanel.SetActive(true);

        var auth = AuthHandler.Instance;

        if(auth != null)
        {
            if(auth.emailLoginField == null)
            {
                auth.emailLoginField = GameObject.Find("Login Email").GetComponent<TMP_InputField>();
                auth.passwordLoginField = GameObject.Find("Login Password").GetComponent<TMP_InputField>();
                auth.firstNameRegisterField = GameObject.Find("First Name").GetComponent<TMP_InputField>();
                auth.lastNameRegisterField = GameObject.Find("Last Name").GetComponent<TMP_InputField>();
                auth.emailRegisterField = GameObject.Find("Regist Email").GetComponent<TMP_InputField>();
                auth.passwordRegisterField = GameObject.Find("Regist Password").GetComponent<TMP_InputField>();
                auth.confirmPasswordRegisterField = GameObject.Find("Confirm Password").GetComponent<TMP_InputField>();
                auth.message = GameObject.Find("Error Message").GetComponent<TMP_Text>();
                Debug.Log("Input fields assigned... allegedly");
            }
        }
        else
        {
            Debug.LogError("Couldn't find AuthHandler class bruv.");
        }

        loginPanel.SetActive(true);          // Show the login panel initially
        registrationPanel.SetActive(false);  // Hide the registration panel initially
    }

    // Toggle panels
    public void ShowPanel(GameObject panelToShow, GameObject panelToHide)
    {
        panelToShow.SetActive(true);
        panelToHide.SetActive(false);
    }

    public void LoginButton()
    {
        AuthHandler.Instance.Login();
    }

    public void RegistButton()
    {
        AuthHandler.Instance.Register();
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

    
}
