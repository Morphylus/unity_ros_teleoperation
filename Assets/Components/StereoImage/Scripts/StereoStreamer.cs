using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RosMessageTypes.Std;
using RosMessageTypes.Sensor;
using Unity.Robotics.ROSTCPConnector;
using System.Threading.Tasks;
using TMPro;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(StereoStreamer))]
public class StereoStreamerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        StereoStreamer myScript = (StereoStreamer)target;
        if(GUILayout.Button("On click"))
        {
            myScript.OnClick();
        }
        if(GUILayout.Button("Select"))
        {
            myScript.OnSelect(1);
        }
        if(GUILayout.Button("Clear"))
        {
            myScript.OnSelect(0);
        }
        if(GUILayout.Button("Flip"))
        {
            myScript.Flip();
        }
        if(GUILayout.Button("Scale Up"))
        {
            myScript.ScaleUp();
        }
        if(GUILayout.Button("Scale Down"))
        {
            myScript.ScaleDown();
        }
    }
}
#endif


public class StereoStreamer : ImageView
{

    private Texture2D _leftTexture2D;
    private Texture2D _rightTexture2D;



    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();

        _Img = transform.Find("Img");
        material = _Img.GetComponent<MeshRenderer>().material;

        dropdown.onValueChanged.AddListener(OnSelect);

        dropdown.gameObject.SetActive(false);
        topMenu.SetActive(false);

        ros.GetTopicAndTypeList(UpdateTopics);
        name.text = "None";

        _icon = topMenu.transform.Find("Track/Image/Image").GetComponent<Image>();
        _frustrum = transform.Find("Frustrum").gameObject;
    }

    void UpdateTopics(Dictionary<string, string> topics)
    {
        List<string> options = new List<string>();
        options.Add("None");
        foreach (var topic in topics)
        {
            if (topic.Value == "sensor_msgs/Image" || topic.Value == "sensor_msgs/CompressedImage")
            {
                // issue with depth images at the moment
                if (topic.Key.Contains("left")) 
                    options.Add(topic.Key);
            }
        }

        if(options.Count == 1)
        {
            Debug.LogWarning("No image topics found!");
            return;
        }
        dropdown.ClearOptions();

        dropdown.AddOptions(options);

        dropdown.value = Mathf.Min(_lastSelected, options.Count - 1);
    }

    public void Flip()
    {
        Debug.Log("Flip not yet implemented");    
    }

    public void OnSelect(int value)
    {
        Debug.Log("OnSelect");

        if (value == _lastSelected) return;
        _lastSelected = value;

        if (topicName != null)
            ros.Unsubscribe(topicName);

        name.text = dropdown.options[value].text.Split(' ')[0];

        if (value == 0)
        {
            topicName = null;
            // set texture to grey
            _leftTexture2D = new Texture2D(3, 2, TextureFormat.RGBA32, false);
            material.SetTexture("_LeftTex", _leftTexture2D);

            _rightTexture2D = new Texture2D(3, 2, TextureFormat.RGBA32, false);
            material.SetTexture("_RightTex", _rightTexture2D);

            
            dropdown.gameObject.SetActive(false);
            topMenu.SetActive(false);
            return;
        }

        topicName = dropdown.options[value].text;

        if (topicName.EndsWith("compressed"))
        {
            ros.Subscribe<CompressedImageMsg>(topicName, OnCompressedLeft);
            ros.Subscribe<CompressedImageMsg>(topicName.Replace("left", "right"), OnCompressedRight);
        }
        else
        {
            // ros.Subscribe<ImageMsg>(topicName, OnImage);
        }
        dropdown.gameObject.SetActive(false);
        topMenu.SetActive(false);

    }


    void SetupTex(int width = 2, int height = 2, bool left = true)
    {
        if (left)
        {
            if (_leftTexture2D == null || _leftTexture2D.width != width || _leftTexture2D.height != height)
            {
                _leftTexture2D = new Texture2D(width, height, TextureFormat.RGBA32, false);
                _leftTexture2D.wrapMode = TextureWrapMode.Clamp;
                _leftTexture2D.filterMode = FilterMode.Bilinear;
                material.SetTexture("_LeftTex", _leftTexture2D);
            }
        }
        else
        {
            if (_rightTexture2D == null || _rightTexture2D.width != width || _rightTexture2D.height != height)
            {
                _rightTexture2D = new Texture2D(width, height, TextureFormat.RGBA32, false);
                _rightTexture2D.wrapMode = TextureWrapMode.Clamp;
                _rightTexture2D.filterMode = FilterMode.Bilinear;
                material.SetTexture("_RightTex", _rightTexture2D);
            }
        }
    }

    void Resize()
    {
        if (_leftTexture2D == null) return;
        float aspectRatio = (float)_leftTexture2D.width/(float)_leftTexture2D.height;

        float width = _Img.transform.localScale.x;
        float height = width / aspectRatio;

        
        _Img.localScale = new Vector3(width, 1, height);
    }

    void OnCompressedLeft(CompressedImageMsg msg)
    {
        SetupTex(2,2,true);
        ParseHeader(msg.header);

        try
        {
            ImageConversion.LoadImage(_leftTexture2D , msg.data);
            _leftTexture2D.Apply();
            Resize();
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
        }
    }


    void OnCompressedRight(CompressedImageMsg msg)
    {
        SetupTex(2,2,false);
        ParseHeader(msg.header);

        try
        {
            ImageConversion.LoadImage(_rightTexture2D , msg.data);
            _rightTexture2D.Apply();
            Resize();
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
        }
    }


}
