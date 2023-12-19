using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RosMessageTypes.Sensor;
using Unity.Robotics.ROSTCPConnector;
using UnityEngine.UI;
using System.Threading.Tasks;


#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(ImageView))]
public class ImageViewEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ImageView imageView = (ImageView)target;
        if (GUILayout.Button("Click"))
        {
            imageView.OnClick();
        }
        if (GUILayout.Button("Select 1"))
        {
            imageView.OnSelect(1);
        }
    }
}
#endif

public class ImageView : MonoBehaviour
{
    public Dropdown dropdown;

    public string topicName;

    private Texture2D _texture2D;
    private RawImage _uiImage;

    private int _lastSelected = 0;
    ROSConnection ros;

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        _uiImage = GetComponent<RawImage>();

        dropdown.onValueChanged.AddListener(OnSelect);

        dropdown.gameObject.SetActive(false);

        ros.GetTopicAndTypeList(UpdateTopics);
    }

    void UpdateTopics(Dictionary<string, string> topics)
    {
        dropdown.ClearOptions();
        List<string> options = new List<string>();
        options.Add("Disable");
        foreach (var topic in topics)
        {
            if (topic.Value == "sensor_msgs/Image" || topic.Value == "sensor_msgs/CompressedImage")
            {
                // issue with depth images at the moment
                if (topic.Key.Contains("depth")) continue;
                options.Add(topic.Key);
            }
        }
        dropdown.AddOptions(options);

        dropdown.value = Mathf.Min(_lastSelected, options.Count - 1);
    }

    public void OnClick()
    {
        ros.GetTopicAndTypeList(UpdateTopics);
        dropdown.gameObject.SetActive(!dropdown.gameObject.activeSelf);
    }

    public void OnSelect(int value)
    {
        if (value == _lastSelected) return;
        _lastSelected = value;
        if (topicName != null)
            ros.Unsubscribe(topicName);

        if (value == 0)
        {
            topicName = null;
            // set texture to grey
            _uiImage.texture = null;
            dropdown.gameObject.SetActive(false);
            return;
        }

        topicName = dropdown.options[value].text;

        if (topicName.EndsWith("compressed"))
        {
            ros.Subscribe<CompressedImageMsg>(topicName, OnCompressed);
        }
        else
        {
            ros.Subscribe<ImageMsg>(topicName, OnImage);
        }
        dropdown.gameObject.SetActive(false);
    }


    void SetupTex(int width = 2, int height = 2)
    {
        if (_texture2D == null)
        {
            _texture2D = new Texture2D(width, height, TextureFormat.RGB24, false);
            _uiImage.texture = _texture2D;
            _uiImage.color = Color.white;
        }
    }

    void Resize()
    {
        if (_texture2D == null) return;

        float aspectRatio = (float)_texture2D.height / (float)_texture2D.width;
        float width = _uiImage.rectTransform.rect.width;
        float height = width * aspectRatio;
        _uiImage.rectTransform.sizeDelta = new Vector2(_uiImage.rectTransform.sizeDelta.x, height);
    }

    void OnCompressed(CompressedImageMsg msg)
    {
        SetupTex();

        try
        {
            ImageConversion.LoadImage(_texture2D, msg.data);

            _texture2D.Apply();

            // debayer the image using bilinear interpolation for rggb format

            // Color[] pixels = _texture2D.GetPixels();
            // int width = _texture2D.width;
            // int height = _texture2D.height;

            // for(int i = 0; i < pixels.Length; i++)
            // {
            //     int x = i % width;
            //     int y = i / width;

            //     // check for the r, g, g, b pattern and interpolate
            //     // check for out of bounds issues
            //     if (x < width - 1 && y < height - 1)
            //     {
            //         if (x % 2 == 0 && y % 2 == 0)
            //         {
            //             // red
            //             pixels[i] = (pixels[i] + pixels[i + 1] + pixels[i + width] + pixels[i + width + 1]) / 4;
            //         }
            //         else if (x % 2 == 1 && y % 2 == 0)
            //         {
            //             // green
            //             pixels[i] = (pixels[i - 1] + pixels[i] + pixels[i + width - 1] + pixels[i + width]) / 4;
            //         }
            //         else if (x % 2 == 0 && y % 2 == 1)
            //         {
            //             // green
            //             pixels[i] = (pixels[i - width] + pixels[i - width + 1] + pixels[i] + pixels[i + 1]) / 4;
            //         }
            //         else if (x % 2 == 1 && y % 2 == 1)
            //         {
            //             // blue
            //             pixels[i] = (pixels[i - width - 1] + pixels[i - width] + pixels[i - 1] + pixels[i]) / 4;
            //         }
            //     }
            // }

            // _texture2D.SetPixels(pixels);


            // _texture2D.Apply();

            Resize();
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
        }
    }

    void OnImage(ImageMsg msg)
    {
        SetupTex((int)msg.width, (int)msg.height);

        try
        {

            _texture2D.LoadRawTextureData(msg.data);
            _texture2D.Apply();
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
        }
        Resize();
    }

}
