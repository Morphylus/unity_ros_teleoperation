using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RosMessageTypes.Sensor;
using RosMessageTypes.BuiltinInterfaces;
using RosMessageTypes.Std;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;

public class Streamer : MonoBehaviour
{
    public string topic = "/quest/image";
    public bool enabled = true;

    private RenderTexture _renderTexture;
    private Texture2D _texture2D;
    private ROSConnection _ros;
    private Camera _camera;
    private HeaderMsg _header;
    private static Streamer _instance;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        _ros = ROSConnection.GetOrCreateInstance();

        GameObject cameraObject = new GameObject("ROSCamera");
        _camera = cameraObject.AddComponent<Camera>();

        _camera.transform.SetParent(Camera.main.transform);

        _renderTexture = new RenderTexture(1280, 720, 24);
        _camera.targetTexture = _renderTexture;
        _camera.transform.localPosition = Vector3.zero;
        _camera.transform.localRotation = Quaternion.identity;
        _camera.transform.localScale = Vector3.one;

        _texture2D = new Texture2D(1280, 720, TextureFormat.RGB24, false);

        _camera.CopyFrom(Camera.main);

        _header = new HeaderMsg(0, new TimeMsg(0, 0), "VR");


        _camera.targetTexture = _renderTexture;

        _ros.RegisterPublisher<ImageMsg>(topic);
    }

    void Update()
    {
        if (!enabled)
            return;

        // copy the RenderTexture to the Texture2D
        RenderTexture.active = _renderTexture;
        _texture2D.ReadPixels(new Rect(0, 0, _renderTexture.width, _renderTexture.height), 0, 0);
        _texture2D.Apply();
        RenderTexture.active = null;

        ImageMsg msg = _texture2D.ToImageMsg(_header);

        _ros.Send(topic, msg);
    }

    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }
}
