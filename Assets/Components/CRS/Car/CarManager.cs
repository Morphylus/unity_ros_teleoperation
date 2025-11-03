using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(CarManager))]
public class CarManagerEditor : SensorManagerEditor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        CarManager carManager = (CarManager)target;
    }
}
#endif

public class CarManager : SensorManager
{
}
