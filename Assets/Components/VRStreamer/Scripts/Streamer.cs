using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RosMessageTypes.Sensor;
using Unity.Robotics.ROSTCPConnector;

public class Streamer : MonoBehaviour
{
    public string topic = "/quest/image";
    public RenderTexture _renderTexture;

    private ROSConnection _ros;
    private Camera _camera;
    void Start()
    {
        _ros = ROSConnection.GetOrCreateInstance();
        _camera = Camera.main;

        _renderTexture = new RenderTexture(_camera.pixelWidth, _camera.pixelHeight, 24);
        _camera.targetTexture = _renderTexture;
                
    }

    void Update()
    {
            ImageMsg msg = new ImageMsg();
            msg.header.frame_id = "base";
            msg.height = (uint)_camera.pixelHeight;
            msg.width = (uint)_camera.pixelWidth;
            msg.encoding = "rgba8";
            msg.is_bigendian = 0;
            msg.step = 4 * (uint)_camera.pixelWidth;
            msg.data = new byte[msg.step * msg.height];
            // for (int i = 0; i < msg.height; i++)
            // {
            //     for (int j = 0; j < msg.width; j++)
            //     {
            //         Color color = _camera.GetPixel(j, i);
            //         int index = 4 * (i * msg.width + j);
            //         msg.data[index] = (byte)(color.r * 255);
            //         msg.data[index + 1] = (byte)(color.g * 255);
            //         msg.data[index + 2] = (byte)(color.b * 255);
            //         msg.data[index + 3] = (byte)(color.a * 255);
            //     }
            // }

            _ros.Send(topic, msg);
        
    }

}
