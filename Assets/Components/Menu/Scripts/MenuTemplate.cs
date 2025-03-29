using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(MenuTemplate))]
public class MenuTemplateEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        MenuTemplate myScript = (MenuTemplate)target;
        if (GUILayout.Button("Setup Rows"))
        {
            myScript.SetupRows();
        }
        if (GUILayout.Button("Toggle Menu"))
        {
            myScript.ToggleMenu();
        }
    }
}
#endif

public class MenuTemplate : MonoBehaviour
{
    public GameObject menu;
    public string tagFilter = ""; // Only show sensors with this tag, leave empty to show all sensors

    public SensorManager[] managers;

    void Start()
    {
        SetupRows();
    }

    public void SetupRows()
    {
        Dictionary<string, int> groups = new Dictionary<string, int>();

        managers = FindObjectsOfType<SensorManager>();
        float offset = 0;
        foreach (SensorManager manager in managers)
        {
            string tag = manager.tag;
            if (!groups.ContainsKey(tag))
            {
                groups.Add(tag, 1);
            }
            else
            {
                groups[tag]++;
            }

            if (tagFilter != "" && tag != tagFilter)
            {
                continue;
            }

            // Make a child of this object's rect transform
            manager.transform.SetParent(menu.transform);
            manager.transform.localPosition = Vector3.zero;
            manager.transform.localRotation = Quaternion.identity;
            manager.transform.localScale = Vector3.one;

            // Move the manager above the last row
            manager.transform.localPosition += Vector3.up * offset;
            offset += manager.GetComponent<RectTransform>().sizeDelta.y + 0.1f;
        }

        string debugOutput = "Found " + groups.Count + " groups:\n";
        foreach (KeyValuePair<string, int> group in groups)
        {
            debugOutput += group.Key + ": " + group.Value + "\n";
        }
        Debug.Log(debugOutput);
    }

    public void ToggleMenu()
    {
        menu.SetActive(!menu.activeSelf);
    }
}
