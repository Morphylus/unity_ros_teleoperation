using System.Collections;
using System.Collections.Generic;
using RosMessageTypes.Crs;
using Unity.Robotics.ROSTCPConnector;
using UnityEngine;

public class ControlInputStream : SensorStream
{
    [Header("Visualization Settings")]
    public GameObject carObject; // Assign the car to follow
    public float arrowLength = 1.0f;
    public float arrowHeight = 0.5f; // Height above car
    public float barHeight = 1.0f; // Max bar height
    public float barOffset = 0.3f; // Distance from car
    
    private LineRenderer steeringLine;
    private LineRenderer torqueLine;
    private GameObject steeringObj;
    private GameObject torqueObj;
    private float currentSteering = 0f;
    private float currentTorque = 0f;

    void Awake()
    {
        _ros = ROSConnection.GetOrCreateInstance();
    }

    void Start()
    {
        Debug.Log("ControlInputStream Start() called");
        
        _msgType = "crs_msgs/car_input";
        topicName = "/car_1/control_input";
        
        _ros.Subscribe<Car_inputMsg>(topicName, OnControlInput);
        Debug.Log($"Subscribed to {topicName}");
        
        // Create steering visualization (arrow pointing in steering direction)
        CreateSteeringVisualization();
        Debug.Log("Created steering visualization");
        
        // Create throttle visualization (vertical bar)
        CreateThrottleVisualization();
        Debug.Log("Created throttle visualization");
        
        // Start coroutine to find car (in case it's not created yet)
        StartCoroutine(FindCarCoroutine());
    }
    
    private IEnumerator FindCarCoroutine()
    {
        int attempts = 0;
        while (carObject == null && attempts < 100) // Try for ~5 seconds
        {
            TryFindCar();
            if (carObject != null)
            {
                Debug.Log($"ControlInputStream: Auto-found car after {attempts} attempts!");
                yield break;
            }
            attempts++;
            yield return new WaitForSeconds(0.05f); // Wait 50ms between attempts
        }
        
        if (carObject == null)
        {
            Debug.LogWarning("ControlInputStream: Could not auto-find car. Please assign manually in Inspector.");
        }
    }
    
    private void TryFindCar()
    {
        if (carObject != null) return;
        
        // Method 1: Search by name in all loaded objects
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.name == "Car" && obj.scene.isLoaded && obj.hideFlags == HideFlags.None)
            {
                carObject = obj;
                return;
            }
        }
        
        // Method 2: Search through CarStream children
        CarStream[] carStreams = FindObjectsOfType<CarStream>();
        foreach (CarStream carStream in carStreams)
        {
            foreach (Transform child in carStream.transform)
            {
                if (child.name == "Car")
                {
                    carObject = child.gameObject;
                    return;
                }
            }
        }
    }

    private void CreateSteeringVisualization()
    {
        steeringObj = new GameObject("SteeringIndicator");
        steeringObj.transform.SetParent(transform);
        
        steeringLine = steeringObj.AddComponent<LineRenderer>();
        steeringLine.material = new Material(Shader.Find("Sprites/Default"));
        steeringLine.startColor = Color.red;
        steeringLine.endColor = Color.red;
        steeringLine.startWidth = 0.05f;
        steeringLine.endWidth = 0.025f;
        steeringLine.positionCount = 2;
    }

    private void CreateThrottleVisualization()
    {
        torqueObj = new GameObject("TorqueIndicator");
        torqueObj.transform.SetParent(transform);
        
        torqueLine = torqueObj.AddComponent<LineRenderer>();
        torqueLine.material = new Material(Shader.Find("Sprites/Default"));
        torqueLine.startColor = Color.green;
        torqueLine.endColor = Color.green;
        torqueLine.startWidth = 0.1f;
        torqueLine.endWidth = 0.05f;
        torqueLine.positionCount = 2;
    }

    private void OnControlInput(Car_inputMsg msg)
    {
        currentSteering = (float)msg.steer; // steering angle in radians
        currentTorque = (float)msg.torque; // torque value
        
        // Debug.Log($"Control Input - Steer: {currentSteering}, Torque: {currentTorque}");
    }

    void Update()
    {
        UpdateVisualization();
    }

    private void UpdateVisualization()
    {
        if (carObject == null)
        {
            // Don't spam console, just skip silently now
            return;
        }
        
        Vector3 carPosition = carObject.transform.position;
        Quaternion carRotation = carObject.transform.rotation;
        
        // Debug every 60 frames
        if (Time.frameCount % 60 == 0)
        {
            Debug.Log($"ControlInputStream: Updating vis at car pos {carPosition}, steer={currentSteering}, torque={currentTorque}");
        }
        
        // Update steering arrow - shows steering direction relative to car
        if (steeringLine != null)
        {
            Vector3 start = carPosition + Vector3.up * arrowHeight;
            
            // Steering angle relative to car's forward direction
            Vector3 steerDirection = carRotation * Quaternion.Euler(0, currentSteering * Mathf.Rad2Deg, 0) * Vector3.forward;
            Vector3 end = start + steerDirection * arrowLength;
            
            steeringLine.SetPosition(0, start);
            steeringLine.SetPosition(1, end);
        }
        
        // Update torque bar - vertical bar showing throttle amount
        if (torqueLine != null)
        {
            Vector3 basePos = carPosition + carRotation * (Vector3.right * barOffset);
            Vector3 start = basePos;
            Vector3 end = basePos + Vector3.up * (currentTorque * barHeight);
            
            torqueLine.SetPosition(0, start);
            torqueLine.SetPosition(1, end);
            
            // Color based on torque (green = positive, red = negative)
            Color torqueColor = currentTorque >= 0 ? Color.green : Color.red;
            torqueLine.startColor = torqueColor;
            torqueLine.endColor = torqueColor;
        }
    }

    public override void OnTopicChange(string newTopic)
    {
        if (!string.IsNullOrEmpty(topicName))
        {
            _ros.Unsubscribe(topicName);
        }
        
        topicName = newTopic;
        
        if (!string.IsNullOrEmpty(topicName) && topicName != "None")
        {
            _ros.Subscribe<Car_inputMsg>(topicName, OnControlInput);
        }
    }

    public override void ToggleTrack(int mode)
    {
        _trackingState = mode;
    }

    void OnDestroy()
    {
        if (!string.IsNullOrEmpty(topicName))
        {
            _ros.Unsubscribe(topicName);
        }
    }
}