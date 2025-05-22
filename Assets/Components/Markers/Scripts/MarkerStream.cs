using System.Collections;
using System.Collections.Generic;
using RosMessageTypes.Visualization;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
 

public class MarkerStream : SensorStream
{
    // This class is used to manage the visualization of markers in Unity.
    private static int nextId = 0;
    private int _id;
    public string topicName = "visualization_marker";

    private ROSConnection _ros;

    public enum MarkerType
    {
        Arrow,
        Cube,
        Sphere,
        Cylinder,
        Line_strip,
        Line_list,
        Cube_list,
        Sphere_list,
        Points,
        Text_view_facing,
        Mesh_resource,
        Triangle_list
    }

    public enum MarkerAction
    {
        Add_modify,     // 0
        Deprecated,    // 1
        Delete,        // 2
        Delete_all,   // 3
    }

    void Awake()
    {
        _id = nextId++;
        // Initialize ROS connection
        _ros = ROSConnection.GetOrCreateInstance();
    }
    // Start is called before the first frame update
    void Start()
    {
        // If we are the first instance subscribe to visualization_marker
        if (topicName != null && _id == 0)
        {
            // Subscribe to the marker topic
            _ros.Subscribe<MarkerMsg>(topicName, OnMarker);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    bool Validate(MarkerMsg msg)
    {
        if (msg.id == (int)MarkerType.Text_view_facing || msg.id == (int)MarkerType.Mesh_resource || msg.id == (int)MarkerType.Triangle_list)
        {
            // Possibly log unsupported
            return false;
        }


        return true;
    }

    void OnMarker(MarkerMsg msg)
    {
        // Handle the received marker message
        // Get the marker type name from the enum
        string markerTypeName = System.Enum.GetName(typeof(MarkerType), msg.type);
        Debug.Log($"Received marker with ID: {msg.id}, Type: {markerTypeName}");

        if (!Validate(msg))
        {
            return;
        }

        
    }

    // Implementation of abstract method ToggleTrack
    public override void ToggleTrack(int trackId)
    {
        // Add your logic here
        Debug.Log($"Toggling track with ID: {trackId}");
    }

    // Implementation of abstract method Serialize
    public override string Serialize()
    {
        // Add your serialization logic here
        Debug.Log("Serializing MarkerStream");
        return "{}"; // Example return value
    }

    // Implementation of abstract method Deserialize
    public override void Deserialize(string data)
    {
        // Add your deserialization logic here
        Debug.Log($"Deserializing MarkerStream with data: {data}");
    }
}
