using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(ROSManager))]
public class ROSManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ROSManager myScript = (ROSManager)target;
        if (GUILayout.Button("ZTE Mode"))
        {
            myScript.ZTE();
        }
        if (GUILayout.Button("Local Mode"))
        {
            myScript.Local();
        }
        if (GUILayout.Button("Toggle Viz"))
        {
            myScript.ToggleViz();
        }
    }
}
#endif

[ExecuteInEditMode]
public class ROSManager : MonoBehaviour
{
    private RosStatus _status;
    private GameObject _viz;
    public string zteIP = "192.168.0.101";
    public string localIP = "195.176.103.116";
    void OnEnable() {
        _status = GetComponentInChildren<RosStatus>();
        _viz = GameObject.Find("DefaultVisualizationSuite");
    }

    public void ToggleViz()
    {
        _viz.SetActive(!_viz.activeSelf);  
    
    }

    public void ZTE()
    {
        _status.defaultIP = zteIP;
    }

    public void Local()
    {
        _status.defaultIP = localIP;
    }
}
