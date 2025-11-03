using System.Collections;
using System.Collections.Generic;
using RosMessageTypes.Crs;
using RosMessageTypes.Geometry;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using UnityEngine;
using UnityEngine.UIElements;

public class CarStream : SensorStream
{
    public GameObject carPrefab;
    private GameObject carInstance;
    public float carScale = 0.4f;

    void Awake()
    {
        _ros = ROSConnection.GetOrCreateInstance();
    }

    void Start()
    {
        _msgType = "crs_msgs/car_state_cart";

        _ros.Subscribe<Car_state_cartMsg>(topicName, OnCarState);

        if (carPrefab != null)
        {
            carInstance = Instantiate(carPrefab, Vector3.zero, Quaternion.identity);
            carInstance.transform.SetParent(transform);
            carInstance.name = "Car";

            carInstance.transform.localScale = Vector3.one * carScale;


            Renderer renderer = carInstance.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = Color.blue;
            }

        } else
        {
            Debug.LogError("Car prefab not provided");
        }
    }

    public override void OnTopicChange(string newTopic)
    {
        if (!string.IsNullOrEmpty(topicName))
        {
            _ros.Unsubscribe(topicName);
        }
    }

    public override void ToggleTrack(int mode)
    {
        _trackingState = mode;
    }

    private void OnCarState(Car_state_cartMsg msg)
    {
        if (carInstance == null) return;

        // Position
        PointMsg rosPosition = new(msg.x, msg.y, msg.z);
        Vector3 unityPosition = rosPosition.From<FLU>();
        carInstance.transform.position = unityPosition;

        // Rotation
        float yawDegrees = (float)msg.yaw * Mathf.Rad2Deg;
        carInstance.transform.rotation = Quaternion.Euler(0, -yawDegrees, 0);
    }

    void OnDestroy()
    {
        // Clean up
        if (carInstance != null)
        {
            Destroy(carInstance);
        }
        
        if (!string.IsNullOrEmpty(topicName))
        {
            _ros.Unsubscribe(topicName);
        }
    }
}
