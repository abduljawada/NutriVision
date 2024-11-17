using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase;
using Firebase.Auth;
using System.Threading.Tasks;
using System;
using System.ComponentModel;

public class AuthHandler : MonoBehaviour
{

    public TMP_Text message;
    public AuthUIManager authUI;

    [Header("Firebase")]
    public DependencyStatus dependencyStatus;
    public FirebaseAuth auth;
    public FirebaseUser user;

    [Space]
    [Header("Login")]
    public TMP_InputField emailLoginField;
    public TMP_InputField passwordLoginField;

    [Space]
    [Header("Registration")]
    public TMP_InputField firstNameRegisterField;
    public TMP_InputField lastNameRegisterField;
    public TMP_InputField emailRegisterField;
    public TMP_InputField passwordRegisterField;
    public TMP_InputField confirmPasswordRegisterField;

    void Awake()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            dependencyStatus = task.Result;

            if (dependencyStatus == DependencyStatus.Available)
            {
                InitializeFirebase();
            }
            else
            {
                Debug.LogError("Could not resolve all firebase dependencies: " +dependencyStatus);
            }
        });  
    }
    
    void InitializeFirebase() 
    {
        // Set the default instance object
        auth = FirebaseAuth.DefaultInstance;
        
        auth.StateChanged += AuthStateChanged;
        AuthStateChanged(this, null);
    }

    // Track state changes of the auth object
    void AuthStateChanged(object sender, System.EventArgs eventArgs)
    {
        if(auth.CurrentUser != user)
        {
            bool signedIn = user != auth.CurrentUser && auth.CurrentUser != null;

            if(!signedIn && user != null)
            {
                Debug.Log("Signed out " + user.UserId);
            }

            user = auth.CurrentUser;

            if(signedIn)
            {
                Debug.Log("Signed in " + user.UserId);
            }
        }
    }

    public void Login()
    {
        StartCoroutine(LoginAsync(emailLoginField.text, passwordLoginField.text));
    }

    private IEnumerator LoginAsync(string email, string password)
    {
        var loginTask = auth.SignInWithEmailAndPasswordAsync(email, password);

        yield return new WaitUntil(() => loginTask.IsCompleted);

        if(loginTask.Exception != null)
        {
            Debug.LogError(loginTask.Exception);

            FirebaseException firebaseException = loginTask.Exception.GetBaseException() as FirebaseException;
            AuthError authError = (AuthError)firebaseException.ErrorCode;

            string failedMessage = "Login Failed! ";

            switch(authError)
            {
                case AuthError.InvalidEmail:
                    failedMessage += "Email or password are invalid.";
                    break;
                case AuthError.WrongPassword:
                    failedMessage += "Email or password are invalid.";
                    break;
                case AuthError.MissingEmail:
                    failedMessage += "Email is missing.";
                    break;
                case AuthError.MissingPassword:
                    failedMessage += "Password is missing.";
                    break;
                default:
                    failedMessage = "Login Failed!";
                    break;
            }

            Debug.Log(failedMessage);
            message.SetText(failedMessage);
        }
        else
        {
            user = loginTask.Result.User;
            Debug.LogFormat("{0} succesfully logged in!", user.DisplayName);
            UnityEngine.SceneManagement.SceneManager.LoadScene("Main");

        }
    }

    public void Register()
    {
        StartCoroutine(RegisterAsync(firstNameRegisterField.text, lastNameRegisterField.text, 
                                    emailRegisterField.text, passwordRegisterField.text, confirmPasswordRegisterField.text));
    }

    private IEnumerator RegisterAsync(string fname, string lname, string email, string password, string confirmPassword)
    {
        if(fname == "" || lname == "" || email == "")
        {   
            string errorMessage = "One or more fields are empty.";
            Debug.LogError(errorMessage);
            message.SetText(errorMessage);
        }
        else if (passwordRegisterField.text != confirmPasswordRegisterField.text)
        {
            string errorMessage = "Passwords do not match.";
            Debug.LogError(errorMessage);
            message.SetText(errorMessage);
        }
        else
        {
            var registerTask = auth.CreateUserWithEmailAndPasswordAsync(email, password);

            yield return new WaitUntil(()=> registerTask.IsCompleted);

            if (registerTask.Exception != null)
            {
                Debug.LogError(registerTask.Exception);

                FirebaseException firebaseException = registerTask.Exception.GetBaseException() as FirebaseException;
                AuthError authError = (AuthError)firebaseException.ErrorCode;

                string failedMessage = "Registration Failed! ";
                switch (authError)
                {
                    case AuthError.InvalidEmail:
                        failedMessage += "Invalid email.";
                        break;
                    case AuthError.WrongPassword:
                        failedMessage += "Wrong password.";
                        break;
                    case AuthError.MissingEmail:
                        failedMessage += "Missing email.";
                        break;
                    case AuthError.MissingPassword:
                        failedMessage += "Missing password.";
                        break;
                    default:
                        failedMessage = "Registration Failed!";
                        break;
                }
                Debug.Log(failedMessage);
                message.SetText(failedMessage);
            }
            else
            {
                user = registerTask.Result.User;

                UserProfile userProfile = new UserProfile {DisplayName = fname + " " + lname};

                var updateProfileTask = user.UpdateUserProfileAsync(userProfile);

                yield return new WaitUntil(() => updateProfileTask.IsCompleted);

                if (updateProfileTask.Exception != null)
                {
                    //Delete the user if user update failed
                    user.DeleteAsync();

                    Debug.LogError(updateProfileTask.Exception);

                    FirebaseException firebaseException = updateProfileTask.Exception.GetBaseException() as FirebaseException;
                    AuthError authError = (AuthError)firebaseException.ErrorCode;

                    string failedMessage = "Profile update failed! ";
                    switch (authError)
                    {
                        case AuthError.InvalidEmail:
                            failedMessage += "Invalid email.";
                            break;
                        case AuthError.WrongPassword:
                            failedMessage += "Wrong password.";
                            break;
                        case AuthError.MissingEmail:
                            failedMessage += "Missing email.";
                            break;
                        case AuthError.MissingPassword:
                            failedMessage += "Missing password.";
                            break;
                        default:
                            failedMessage = "Registration Failed!";
                            break;
                    }
                    Debug.Log(failedMessage);
                    message.SetText(failedMessage);
                }
                else
                {
                    Debug.Log("Registration Sucessful! Welcome " + user.DisplayName);
                    message.SetText("Registration Sucessful! Welcome " + user.DisplayName);
                    authUI.BackButton();
                }

            }
        }
    }
}
