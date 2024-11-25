using TMPro;
using UnityEngine;

namespace DebugStuff
{
    public class ConsoleToGUI : MonoBehaviour
    {
        private TMP_Text consoleText => GetComponent<TMP_Text>();
        static string myLog = "";
        private string output;
        private string stack;

        [SerializeField] private float timeBtwClear = 5f;
        private float timeSinceClear = 0f;

        void Update()
        {
            timeSinceClear += Time.deltaTime;
            if (timeSinceClear >= timeBtwClear)
            {
                myLog = "";
                consoleText.text = myLog;
                timeSinceClear = 0f;
            }
        }

        void OnEnable()
        {
            Application.logMessageReceived += Log;
        }

        void OnDisable()
        {
            Application.logMessageReceived -= Log;
        }

        public void Log(string logString, string stackTrace, LogType type)
        {
            output = logString;
            stack = stackTrace;
            myLog = output + "\n" + myLog;
            if (myLog.Length > 5000)
            {
                myLog = myLog.Substring(0, 4000);
            }
            consoleText.text = myLog;
        }
    }
}