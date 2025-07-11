using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(SensorStream))]
public abstract class SensorStreamEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        SensorStream myScript = (SensorStream)target;
        if (GUILayout.Button("Clear"))
        {
            myScript.Clear();
        }
    }
}
#endif

public abstract class SensorStream : MonoBehaviour
{
    /// <summary>
    /// Reference back to the manager that spawned this sensor
    /// </summary>
    public SensorManager manager;

    /// <summary>
    /// Sets what tracking mode we should be in, changes based on the sensor
    /// </summary>
    public abstract void ToggleTrack(int mode);

    /// <summary>
    /// Converts the state of this sensor into a string so that it can be reinitialized later
    /// </summary>
    public abstract string Serialize();

    /// <summary>
    /// Converts a string into the state of this sensor
    /// </summary>
    public abstract void Deserialize(string data);

    /// <summary>
    /// Clears the sensor stream, removing it from the manager
    /// </summary>
    public void Clear()
    {
        manager.Remove(gameObject);
    }
}
