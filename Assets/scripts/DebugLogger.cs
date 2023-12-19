using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(DebugLogger))]
public class DebugLoggerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        DebugLogger myScript = (DebugLogger)target;
        if(GUILayout.Button("Toggle Debug Mode"))
        {
            myScript.toggleDebug();
        }
    }
}
#endif

public class DebugLogger : MonoBehaviour
{
    public int qsize = 150;  // number of messages to keep
    public bool startActive = false;

    private TMPro.TextMeshProUGUI text;
    
    Queue myLogQueue = new Queue();

    public delegate void ToggleDebug();
    public ToggleDebug toggleDebug;

    // Captures Debug.Log output and displays it in a GUI Text for the User to see in the app

    void OnEnable() {
        text = GetComponentInChildren<TMPro.TextMeshProUGUI>();
        text.gameObject.SetActive(startActive);
        toggleDebug = ToggleDebugMode;
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable() {
        Application.logMessageReceived -= HandleLog;
    }

    void OnAwake(){
    }

    public void ToggleDebugMode(){
        text.gameObject.SetActive(!text.gameObject.activeSelf);
    }


    void HandleLog(string logString, string stackTrace, LogType type) {
        string colorTag = "<color=\"";
        switch (type)
        {
            case LogType.Error:
            case LogType.Exception:
            case LogType.Assert:
                colorTag += "red\">";
                break;
            case LogType.Warning:
                colorTag += "orange\">";
                break;
            case LogType.Log:
            default:
                colorTag += "black\">";
                break;
        }
        myLogQueue.Enqueue(colorTag + "[" + type + "] : " + logString + "</color>");
        if (type == LogType.Exception)
            myLogQueue.Enqueue(stackTrace);
        while (myLogQueue.Count > qsize)
            myLogQueue.Dequeue();

        text.SetText(string.Join("\n", myLogQueue.ToArray()));
    }


}
