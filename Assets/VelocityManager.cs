using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(VelocityManager))]
public class VelocityManagerEditor : SensorManagerEditor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        VelocityManager VelocityManager = (VelocityManager)target;
    }
}
#endif

public class VelocityManager : SensorManager
{
    void Start()
    {
        name = "Velocity";
        tag = "Velocity";
    }
}
