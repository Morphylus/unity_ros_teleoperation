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


public class StereoStreamer : MonoBehaviour
{

    public Dropdown dropdown;
    public GameObject topMenu;
    public CameraManager manager;
    public TMPro.TextMeshProUGUI name;
    public Sprite untracked;
    public Sprite tracked;
    public string topicName;
    public Material material;

    private Texture2D _leftTexture2D;
    private Texture2D _rightTexture2D;

    private int _lastSelected = 0;

    public bool _tracking = false;
    private GameObject _frustrum;
    private Image _icon;

    ROSConnection ros;


    public bool CleanTF(string name)
    {
        GameObject target = GameObject.Find(name);

        List<GameObject> children = new List<GameObject>();

        // check if this is connected to root
        int count = 0;
        while(target.transform.parent != null)
        {
            count++;
            children.Add(target);
            target = target.transform.parent.gameObject;
            if(target.name == "odom")
            {
                children.Clear();
                Debug.Log("Connected to root");
                return true;
            }
            if(count > 100)
            {
                Debug.LogError("Looping too much");
                return false;
            }
        }

        foreach(GameObject child in children)
        {
            Destroy(child);
        }
        return false;
    }


    void UpdatePose(string frame)
    {
        if(!CleanTF(frame))
        {
            return;
        }
        GameObject _parent = GameObject.Find(frame);
        if(_parent == null) return;

        transform.parent = _parent.transform;
        transform.localPosition = new Vector3(0.1f, 0.2f, 0);
        transform.localRotation = Quaternion.Euler(-90, 90, 180);
        // transform.localScale = new Vector3(-1, 1, 1);
    }

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();

        material = GetComponent<MeshRenderer>().material;

        dropdown.onValueChanged.AddListener(OnSelect);

        dropdown.gameObject.SetActive(false);
        topMenu.SetActive(false);

        ros.GetTopicAndTypeList(UpdateTopics);
        name.text = "None";

        _icon = topMenu.transform.Find("Track/Image/Image").GetComponent<Image>();
        _frustrum = transform.Find("Frustrum").gameObject;
    }

    private void OnDestroy() {
        ros.Unsubscribe(topicName);
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

    public void Clear()
    {
        manager.Remove(gameObject);
    }

    public void ToggleTrack()
    {
        _tracking = !_tracking;

        _icon.sprite = _tracking ? tracked : untracked;
        dropdown.gameObject.SetActive(false);
        topMenu.SetActive(false);
    }

    public void Flip()
    {
        Debug.Log("Flip not yet implemented");    
    }

    public void ScaleUp()
    {
        transform.localScale *= 1.1f;
    }

    public void ScaleDown()
    {
        transform.localScale *= 0.9f;
    }

    public void OnClick()
    {
        ros.GetTopicAndTypeList(UpdateTopics);
        dropdown.gameObject.SetActive(!dropdown.gameObject.activeSelf);
        topMenu.gameObject.SetActive(dropdown.gameObject.activeSelf);
    }

    public void OnSelect(int value)
    {
        if (value == _lastSelected) return;
        _lastSelected = value;
        if (topicName != null)
            ros.Unsubscribe(topicName);

        name.text = dropdown.options[value].text;

        if (value == 0)
        {
            topicName = null;
            // set texture to grey
            _leftTexture2D = new Texture2D(2, 2, TextureFormat.RGBA32, false);

            _rightTexture2D = new Texture2D(2, 2, TextureFormat.RGBA32, false);

            
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
        float height = transform.localScale.y;
        float width = height * aspectRatio;
        
        transform.localScale = new Vector3(width, height, 1);
    }

    void ParseHeader(HeaderMsg header)
    {

        if (_tracking)
        {
            // If we are tracking to the TF, update the parent
            if(header.frame_id != null && (transform.parent == null || header.frame_id != transform.parent.name))
            {
                _frustrum.SetActive(true);
                // If the parent is not the same as the frame_id, update the parent
                UpdatePose(header.frame_id);
            }

        } else if (transform.parent != null && transform.parent.name != "odom")
        {
            _frustrum.SetActive(false);
            // Otherwise, set the parent to the odom frame but keep the current position
            Vector3 pos = transform.position;
            Quaternion rot = transform.rotation;
            UpdatePose("odom");
            transform.position = pos;
            transform.rotation = rot;
        }
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
