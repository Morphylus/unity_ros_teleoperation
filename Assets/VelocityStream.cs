using System.Collections;
using System.Collections.Generic;
using RosMessageTypes.Crs;
using Unity.Robotics.ROSTCPConnector;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VelocityStream : SensorStream
{
    [Header("ROS Settings")]
    public string velocityTopic = "/car_1/ros_simulator/gt_state";

    [Header("UI Elements")]
    public TextMeshProUGUI velocityTextHUD; // Velocity text shown below needle in HUD mode
    public Image needleImage; // The needle/pointer for analog display (HUD mode only)
    public Image speedometerImage; // The speedometer gauge background (HUD mode only)
    public TextMeshPro velocityText3D; // 3D world-space velocity text that follows the car (billboard)

    public enum DisplayMode { HUD, FollowCar }

    [Header("Follow Car Settings")]
    [Tooltip("The car GameObject to follow. Will auto-find 'Car' if not assigned.")]
    public GameObject carObject;
    [Tooltip("Height above car where velocity text appears")]
    public float textHeightAboveCar = 0.4f;
    [Tooltip("Forward offset from car center")]
    public float textForwardOffset = 0.0f;

    [Header("Display Settings")]
    public DisplayMode currentDisplayMode = DisplayMode.HUD;
    public float maxDisplaySpeed = 3f; // m/s (manual setting) - adjusted for low-speed scenarios
    public bool autoScale = false; // Automatically adjust max speed based on observed velocities
    public float autoScaleMargin = 1.2f; // Add 20% margin when auto-scaling
    public float autoScaleUpdateInterval = 2f; // How often to update auto-scale (seconds)

    public Color lowSpeedColor = Color.green;
    public Color midSpeedColor = Color.yellow;
    public Color highSpeedColor = Color.red;
    public float colorTransitionSpeed = 0.5f; // At what percentage to transition (0-1)

    [Header("Needle Settings")]
    [Tooltip("Min velocity in m/s that maps to the start of the needle gauge. Fixed unless autoScale is enabled.")]
    public float minVelocityForMapping = 1.0f;
    [Tooltip("Max velocity in m/s that maps to the end of the needle gauge. Auto-updated when autoScale is enabled.")]
    public float maxVelocityForMapping = 2.5f;

    [Tooltip("Needle rotation angle at minimum velocity in degrees (right side of gauge)")]
    public float minNeedleAngle = -120f;
    [Tooltip("Needle rotation angle at maximum velocity in degrees (left side of gauge)")]
    public float maxNeedleAngle = 120f;

    [Tooltip("Speed of needle interpolation (higher = faster response)")]
    public float needleSmoothSpeed = 5f;

    [Header("Visibility")]
    public bool isVisible = true; // Toggle visibility of speedometer
    public GameObject velocityPanel; // Parent panel containing all UI elements

    private float currentVelocity = 0f;
    private float observedMaxVelocity = 0f;
    private float lastAutoScaleUpdate = 0f;
    private float targetNeedleAngle = 0f;
    private float currentNeedleAngle = 0f;

    void Awake()
    {
        _ros = ROSConnection.GetOrCreateInstance();
    }

    void Start()
    {
        _msgType = "crs_msgs/car_state_cart";

        // Subscribe to the velocity topic
        _ros.Subscribe<Car_state_cartMsg>(velocityTopic, OnVelocityUpdate);

        // Auto-find car if not assigned
        if (carObject == null)
        {
            carObject = GameObject.Find("Car");
            if (carObject == null)
            {
                Debug.LogWarning("VelocityStream: Could not find 'Car' GameObject. Please assign manually.");
            }
        }

        // Initialize HUD velocity text (shown below needle)
        if (velocityTextHUD != null)
        {
            velocityTextHUD.text = "0.00 m/s";
            velocityTextHUD.color = lowSpeedColor;
            velocityTextHUD.alignment = TextAlignmentOptions.Center;
            velocityTextHUD.fontStyle = FontStyles.Bold;
        }

        // Initialize needle
        if (needleImage != null)
        {
            currentNeedleAngle = minNeedleAngle;
            needleImage.rectTransform.localRotation = Quaternion.Euler(0, 0, currentNeedleAngle);
        }

        // Initialize 3D velocity text (follows car)
        if (velocityText3D != null)
        {
            velocityText3D.text = "0.00 m/s";
            velocityText3D.color = lowSpeedColor;
            velocityText3D.alignment = TextAlignmentOptions.Center;
            velocityText3D.fontStyle = FontStyles.Bold;
        }

        // Set initial visibility
        UpdateDisplayMode();
        UpdateVisibility();
    }

    void Update()
    {
        // Desktop: L key to toggle display mode
        if (Input.GetKeyDown(KeyCode.L))
        {
            ToggleDisplayMode();
        }

        // Quest: Right controller B button (secondary button) to toggle display mode
        #if UNITY_XR_MANAGEMENT
        if (UnityEngine.XR.InputDevices.GetDeviceAtXRNode(UnityEngine.XR.XRNode.RightHand).TryGetFeatureValue(UnityEngine.XR.CommonUsages.secondaryButton, out bool secondaryButtonValue) && secondaryButtonValue)
        {
            ToggleDisplayMode();
        }
        #endif

        // Smooth needle rotation (HUD mode only)
        if (needleImage != null && currentDisplayMode == DisplayMode.HUD)
        {
            currentNeedleAngle = Mathf.Lerp(currentNeedleAngle, targetNeedleAngle, Time.deltaTime * needleSmoothSpeed);
            needleImage.rectTransform.localRotation = Quaternion.Euler(0, 0, currentNeedleAngle);
        }

        // Update 3D text position and billboard effect (FollowCar mode only)
        if (currentDisplayMode == DisplayMode.FollowCar && velocityText3D != null && carObject != null)
        {
            // Position text above the car
            Vector3 targetPosition = carObject.transform.position
                + Vector3.up * textHeightAboveCar
                + carObject.transform.forward * textForwardOffset;
            velocityText3D.transform.position = targetPosition;

            // Billboard effect: Always face the camera
            if (Camera.main != null)
            {
                velocityText3D.transform.LookAt(Camera.main.transform);
                velocityText3D.transform.Rotate(0, 180, 0); // Flip so text faces camera correctly

                // Slightly offset toward camera to ensure it renders on top
                velocityText3D.transform.position += Camera.main.transform.forward * -0.01f;
            }
        }
    }

    public override void OnTopicChange(string newTopic)
    {
        if (!string.IsNullOrEmpty(velocityTopic))
        {
            _ros.Unsubscribe(velocityTopic);
        }

        velocityTopic = newTopic;
        _ros.Subscribe<Car_state_cartMsg>(velocityTopic, OnVelocityUpdate);
    }

    public override void ToggleTrack(int mode)
    {
        _trackingState = mode;
    }

    /// <summary>
    /// Manually set the maximum display speed
    /// </summary>
    public void SetMaxDisplaySpeed(float speed)
    {
        maxDisplaySpeed = Mathf.Max(0.1f, speed);
        autoScale = false; // Disable auto-scale when manually setting
    }

    /// <summary>
    /// Reset the observed maximum velocity (useful when switching environments)
    /// </summary>
    public void ResetAutoScale()
    {
        observedMaxVelocity = 0f;
        lastAutoScaleUpdate = 0f;
    }

    /// <summary>
    /// Toggle between HUD (needle + speedometer) and FollowCar (3D text above car) display modes
    /// </summary>
    public void ToggleDisplayMode()
    {
        currentDisplayMode = (currentDisplayMode == DisplayMode.HUD) ? DisplayMode.FollowCar : DisplayMode.HUD;
        UpdateDisplayMode();
        Debug.Log($"Display Mode: {currentDisplayMode}");
    }

    /// <summary>
    /// Update which UI elements are visible based on display mode
    /// </summary>
    private void UpdateDisplayMode()
    {
        if (currentDisplayMode == DisplayMode.HUD)
        {
            // Show HUD elements (needle + speedometer), hide 3D text
            if (velocityTextHUD != null) velocityTextHUD.gameObject.SetActive(true);
            if (needleImage != null) needleImage.gameObject.SetActive(true);
            if (speedometerImage != null) speedometerImage.gameObject.SetActive(true);
            if (velocityText3D != null) velocityText3D.gameObject.SetActive(false);
        }
        else // FollowCar mode
        {
            // Show 3D text above car, hide HUD elements
            if (velocityTextHUD != null) velocityTextHUD.gameObject.SetActive(false);
            if (needleImage != null) needleImage.gameObject.SetActive(false);
            if (speedometerImage != null) speedometerImage.gameObject.SetActive(false);
            if (velocityText3D != null) velocityText3D.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Toggle visibility of the velocity display
    /// </summary>
    public void ToggleVisibility()
    {
        isVisible = !isVisible;
        UpdateVisibility();
        Debug.Log($"Velocity Display: {(isVisible ? "Visible" : "Hidden")}");
    }

    /// <summary>
    /// Update the visibility state of UI elements
    /// </summary>
    private void UpdateVisibility()
    {
        if (velocityPanel != null)
        {
            velocityPanel.SetActive(isVisible);
        }
        else
        {
            // Fallback: toggle individual elements if panel not assigned
            if (velocityTextHUD != null) velocityTextHUD.gameObject.SetActive(isVisible);
            if (needleImage != null) needleImage.gameObject.SetActive(isVisible);
            if (speedometerImage != null) speedometerImage.gameObject.SetActive(isVisible);
            if (velocityText3D != null) velocityText3D.gameObject.SetActive(isVisible);
        }
    }

    private void OnVelocityUpdate(Car_state_cartMsg msg)
    {
        currentVelocity = (float)msg.v_tot;

        // Auto-scale logic
        if (autoScale)
        {
            observedMaxVelocity = Mathf.Max(observedMaxVelocity, currentVelocity);

            // Update max display speed periodically
            if (Time.time - lastAutoScaleUpdate > autoScaleUpdateInterval)
            {
                if (observedMaxVelocity > 0)
                {
                    maxDisplaySpeed = observedMaxVelocity * autoScaleMargin;
                    // Also update the max velocity for needle gauge mapping
                    maxVelocityForMapping = observedMaxVelocity;
                }
                lastAutoScaleUpdate = Time.time;
            }
        }

        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        // Calculate velocity percentage within the mapped range for needle positioning
        float velocityPercentage = Mathf.InverseLerp(minVelocityForMapping, maxVelocityForMapping, currentVelocity);

        // Determine what percentage to use for color transitions
        float fillPercentage = CalculateFillPercentage();

        // Calculate color based on speed percentage (green → yellow → red)
        Color targetColor = CalculateSpeedColor(fillPercentage);

        // Update HUD display (needle + speedometer)
        if (currentDisplayMode == DisplayMode.HUD)
        {
            // Calculate needle angle based on velocity percentage
            // Invert the lerp so higher velocity goes right (negative angle) and lower velocity goes left (positive angle)
            targetNeedleAngle = Mathf.Lerp(maxNeedleAngle, minNeedleAngle, velocityPercentage);

            // Update velocity text showing actual m/s value
            if (velocityTextHUD != null)
            {
                velocityTextHUD.text = $"{currentVelocity:F2} m/s";
                velocityTextHUD.color = targetColor;
            }

            // Update needle color
            if (needleImage != null)
            {
                needleImage.color = targetColor;
            }
        }
        // Update 3D text following car
        else if (currentDisplayMode == DisplayMode.FollowCar)
        {
            if (velocityText3D != null)
            {
                velocityText3D.text = $"{currentVelocity:F2} m/s";
                velocityText3D.color = targetColor;
            }
        }
    }

    /// <summary>
    /// Calculate the percentage (0-1) used for color interpolation.
    /// Uses velocity percentage within the mapped range for consistent color across all display modes
    /// </summary>
    private float CalculateFillPercentage()
    {
        // Use velocity percentage within mapped range so colors are consistent
        return Mathf.InverseLerp(minVelocityForMapping, maxVelocityForMapping, currentVelocity);
    }

    /// <summary>
    /// Calculates color based on speed percentage.
    /// Transitions: green (0%) → yellow (50% default) → red (100%)
    /// The transition point is configurable via colorTransitionSpeed
    /// </summary>
    private Color CalculateSpeedColor(float fillPercentage)
    {
        if (fillPercentage < colorTransitionSpeed)
        {
            // Low to mid range: green → yellow
            float t = fillPercentage / colorTransitionSpeed;
            return Color.Lerp(lowSpeedColor, midSpeedColor, t);
        }
        else
        {
            // Mid to high range: yellow → red
            float t = (fillPercentage - colorTransitionSpeed) / (1f - colorTransitionSpeed);
            return Color.Lerp(midSpeedColor, highSpeedColor, t);
        }
    }

    void OnDestroy()
    {
        if (!string.IsNullOrEmpty(velocityTopic))
        {
            _ros.Unsubscribe(velocityTopic);
        }
    }
}
