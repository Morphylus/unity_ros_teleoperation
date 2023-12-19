using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RosMessageTypes.Sensor;
// using Unity.Robotics.Visualizations;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Tf2;
using RosMessageTypes.Geometry;

public class LidarViz : MonoBehaviour
{
    ROSConnection ros;
    public int throttle = 1;
    public string topic;
    private Mesh _mesh;
    private ParticleSystem _particleSystem;

    public float size=0.1f;

    private static int counter = 0;

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.Subscribe<PointCloud2Msg>(topic, LidarCallback);

        _mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = _mesh;

        _particleSystem = GetComponent<ParticleSystem>();

        // create 4 particles at +1, +1, +1

        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[4];
        particles[0].position = new Vector3(1, 1, 1);
        particles[1].position = new Vector3(1, -1, 1);
        particles[2].position = new Vector3(-1, -1, 1);
        particles[3].position = new Vector3(-1, 1, 1);
        particles[0].startColor = Color.red;
        particles[1].startColor = Color.green;
        particles[2].startColor = Color.blue;
        particles[3].startColor = Color.yellow;
        particles[0].startSize = 0.1f;
        particles[1].startSize = 0.1f;
        particles[2].startSize = 0.1f;
        particles[3].startSize = 0.1f;
        _particleSystem.SetParticles(particles, 4);

        


        // _particleSystem.Play();
    }

    void LidarCallback(PointCloud2Msg msg)
    {
        counter++;
        if(counter > throttle)
        {
            counter = 0;
        } else {
            return;
        }

        // if parent is null, use the header frame id and set that as parent
        if(transform.parent == null){
            Debug.Log("Setting parent to " + msg.header.frame_id + " for " + gameObject.name  );
            var tfSystem = TFSystem.GetOrCreateInstance();
            var parent = tfSystem.GetOrCreateFrame(msg.header.frame_id);
            transform.SetParent(parent.GameObject.transform);
        }

        // set the positions of the particles based on msg data
        Color[] colors;
        ParticleSystem.Particle[] particles;
        Vector3[] verts;
        int[] faces;
        Vector3[] points = PointFieldParse(msg, out colors, out particles, out verts, out faces);
        // Debug.Log(particles.Length+ " "+points[0]);

        // // for now debug draw the points
        // for (int i = 0; i < points.Length; i+= 100)
        // {
        //     Debug.DrawLine(points[i], points[i] + Vector3.up * 0.1f, colors[i], 100f);
        // }
        // _particleSystem.Clear();

        _particleSystem.SetParticles(particles, particles.Length);


                
    }

    public void ChangeSize(float size)
    {
        this.size = size;
    }


    Vector3[] PointFieldParse(PointCloud2Msg msg, out Color[] colors, out ParticleSystem.Particle[] particles, out Vector3[] verts, out int[] faces)
    {
        bool bigEndian = msg.is_bigendian;
        PointFieldMsg[] fields = msg.fields;
        int pointStep = (int)msg.point_step;
        int pointCount = (int)(msg.width * msg.height);
        byte[] data = msg.data;
        bool isDense = msg.is_dense;

        Vector3[] points = new Vector3[pointCount];
        colors = new Color[pointCount];
        particles = new ParticleSystem.Particle[pointCount];
        // create a tri for each point
        verts = new Vector3[pointCount*3];
        faces = new int[pointCount*3];

        for (int i = 0; i < pointCount; i++)
        {
            int offset = i * pointStep;
            float x = 0;
            float y = 0;
            float z = 0;
            foreach (PointFieldMsg field in fields)
            {
                int fieldOffset = (int)(offset + field.offset);
                float value = 0;
                if (field.datatype == PointFieldMsg.FLOAT32)
                {
                    value = System.BitConverter.ToSingle(data, fieldOffset);
                }
                else if (field.datatype == PointFieldMsg.FLOAT64)
                {
                    value = (float)System.BitConverter.ToDouble(data, fieldOffset);
                }
                else if (field.datatype == PointFieldMsg.INT8)
                {
                    value = data[fieldOffset];
                }
                else if (field.datatype == PointFieldMsg.UINT8)
                {
                    value = data[fieldOffset];
                }
                else if (field.datatype == PointFieldMsg.INT16)
                {
                    value = System.BitConverter.ToInt16(data, fieldOffset);
                }
                else if (field.datatype == PointFieldMsg.UINT16)
                {
                    value = System.BitConverter.ToUInt16(data, fieldOffset);
                }
                else if (field.datatype == PointFieldMsg.INT32)
                {
                    value = System.BitConverter.ToInt32(data, fieldOffset);
                }
                else if (field.datatype == PointFieldMsg.UINT32)
                {
                    value = System.BitConverter.ToUInt32(data, fieldOffset);
                }
                else
                {
                    Debug.LogError("Unknown datatype: " + field.datatype);
                }

                if (field.name == "x")
                {
                    x = value;
                }
                else if (field.name == "y")
                {
                    y = -value;
                }
                else if (field.name == "z")
                {
                    z = value;
                }
                else if (field.name == "rgb")
                {
                    // convert float32 value to rgb colors using bit shifting
                    uint rgb = System.BitConverter.ToUInt32(data, fieldOffset);

                    int red = (int)(rgb >> 16 & 0x0000ff);
                    int green = (int)(rgb >> 8 & 0x0000ff);
                    int blue = (int)(rgb & 0x0000ff);

                    colors[i] = new Color(red / 255f, green / 255f, blue / 255f);


                    particles[i].startColor = colors[i];
                }
            }
            points[i] = new Vector3(x, y, z);
            particles[i].position = points[i];
            particles[i].startSize = size;
            // particles[i].startLifetime = 100f;


            // create a tri for each point
            // verts[i*3] = points[i];
            // verts[i*3+1] = points[i] + Vector3.up * 0.1f;
            // verts[i*3+2] = points[i] + Vector3.right * 0.1f;
            // faces[i*3] = i*3;
            // faces[i*3+1] = i*3+1;
            // faces[i*3+2] = i*3+2;

        }

        return points;
    }

}
