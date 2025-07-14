using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Robotics.ROSTCPConnector;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(LidarManager))]
public class LidarManagerEditor : SensorManagerEditor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        LidarManager myScript = (LidarManager)target;
        // add text boxes for the topics
    }
}

#endif

public class LidarManager : SensorManager
{
    // public LidarStream lidarStreamer;
    // public LidarStream rgbdStreamer;

    // public TMPro.TextMeshProUGUI lidarTopic;
    // public TMPro.TextMeshProUGUI rgbdTopic;

    // public Dropdown topicDropdown;

    // private string _lidarTopic;
    // private string _rgbdTopic;

    // private bool _lidarClicked;

    private GridMapVisualizer _gridMapVisualizer;

    // public GameObject menu;

    private ROSConnection ros;


    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();

        _gridMapVisualizer = FindObjectOfType<GridMapVisualizer>();
        if (_gridMapVisualizer == null)
        {
            Debug.LogWarning("No GridMapVisualizer found in scene!");
        }
    }


    public void ToggleGridMap()
    {
        if(_gridMapVisualizer != null)
        {
            _gridMapVisualizer.gameObject.SetActive(!_gridMapVisualizer.gameObject.activeSelf);
        }
    }

}
