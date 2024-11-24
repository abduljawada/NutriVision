using UnityEngine;
using TMPro;

public class DebugToScreen : MonoBehaviour
{
    [SerializeField] TMP_Text debugText;

    public void Log(string textToLog)
    {
        Debug.Log(textToLog);
        debugText.text = debugText.text + "\n" + textToLog;
    }


    public void LogError(string textToLog)
    {
        Debug.LogError(textToLog);
        debugText.text = debugText.text + "\nError!!! " + textToLog;
    }
}
