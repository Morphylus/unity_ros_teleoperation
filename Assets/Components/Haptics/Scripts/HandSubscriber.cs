using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.PsyonicSdkRos;

public class HandSubscriber : MonoBehaviour
{
    public string topicName = "/psyonic_sdk_ros/touch_state";

    private static Dictionary<string, int> handMap = new Dictionary<string, int> {
        { "thumb_site0", 0 },
        { "thumb_site1", 0 },
        { "thumb_site2", 0 },
        { "thumb_site3", 0 },
        { "thumb_site4", 0 },
        { "thumb_site5", 0 },
        { "index_site0", 1 },
        { "index_site1", 1 },
        { "index_site2", 1 },
        { "index_site3", 1 },
        { "index_site4", 1 },
        { "index_site5", 1 },
        { "middle_site0", 2 },
        { "middle_site1", 2 },
        { "middle_site2", 2 },
        { "middle_site3", 2 },
        { "middle_site4", 2 },
        { "middle_site5", 2 },
        { "ring_site0", 3 },
        { "ring_site1", 3 },
        { "ring_site2", 3 },
        { "ring_site3", 3 },
        { "ring_site4", 3 },
        { "ring_site5", 3 },
        { "pinky_site0", 4 },
        { "pinky_site1", 4 },
        { "pinky_site2", 4 },
        { "pinky_site3", 4 },
        { "pinky_site4", 4 },
        { "pinky_site5", 4 }
    };

    ROSConnection ros;

    // Start is called before the first frame update
    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.Subscribe<TouchStateMsg>(topicName, ReceiveMessage);
    }

    int ForceMapper(float force)
    {
        int value = (int)(100 * force);
        value = Mathf.Clamp(value, 0, 100);
        return value;
    }

    void ReceiveMessage(TouchStateMsg msg)
    {
        bool isRight = true;

        int[] values = new int[6];
        for (int i = 0; i < msg.name.Length; i++)
        {
            if (handMap.ContainsKey(msg.name[i]))
            {
                int index = handMap[msg.name[i]];
                values[index] += ForceMapper((float)msg.force[i]);
                values[index] = Mathf.Clamp(values[index], 0, 100);
            }
        }

        HandManager.Instance.UpdateValues(isRight, values);
    }

}
