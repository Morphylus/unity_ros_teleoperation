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
    public Material carMaterial;

    // Trail settings
    public bool showTrail = true;
    private GameObject trailObject;
    private TrailRenderer trail;
    public float trailTime = 3f;
    public Color trailColor = Color.white;

    void Awake()
    {
        _ros = ROSConnection.GetOrCreateInstance();
    }

    void Start()
    {
        _msgType = "crs_msgs/car_state_cart";
        _ros.Subscribe<Car_state_cartMsg>(topicName, OnCarState);

        if (carMaterial == null)
        {
            carMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            carMaterial.SetColor("_BaseColor", Color.white);
        }

        if (carPrefab != null)
        {
            carInstance = Instantiate(carPrefab, Vector3.zero, Quaternion.identity);
            carInstance.transform.SetParent(transform);
            carInstance.name = "Car";
            carInstance.transform.localScale = Vector3.one * carScale;

            Renderer[] renderers = carInstance.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                renderer.material = carMaterial;
            }
        }

        // Create separate trail object
        trailObject = new GameObject("CarTrail");
        trailObject.transform.SetParent(transform);
        trail = trailObject.AddComponent<TrailRenderer>();
        trail.time = trailTime;
        trail.startWidth = 0.005f;
        trail.endWidth = 0.0025f;
        trail.material = new Material(Shader.Find("Sprites/Default"));
        trail.startColor = trailColor;
        trail.endColor = new Color(trailColor.r, trailColor.g, trailColor.b, 0);

        trailObject.SetActive(showTrail);
    }

    void Update()
    {
        // Toggle trail visibility
        if (trailObject != null)
        {
            trailObject.SetActive(showTrail);
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
        if (carInstance == null)
            return;

        // Position
        PointMsg rosPosition = new(msg.x, msg.y, msg.z);
        Vector3 unityPosition = rosPosition.From<FLU>();
        carInstance.transform.position = unityPosition;

        // Update trail position
        if (trailObject != null)
        {
            trailObject.transform.position = unityPosition;
        }

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

        if (trailObject != null)
        {
            Destroy(trailObject);
        }

        if (!string.IsNullOrEmpty(topicName))
        {
            _ros.Unsubscribe(topicName);
        }
    }
}
