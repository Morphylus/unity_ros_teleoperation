using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RosMessageTypes.Sensor;


public class LidarUtils 
{
    public static Mesh MakePolygon(int sides)
    {
        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[sides];
        int[] triangles = new int[(sides - 2) * 3];
        for(int i = 0; i < sides; i++)
        {
            float angle = 2 * Mathf.PI * i / sides;
            vertices[i] = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0);
        }
        for(int i = 0; i < sides - 2; i++)
        {
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = i + 2;
        }
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.UploadMeshData(false);
        return mesh;
    }

    public static Mesh MakeCube()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = new Vector3[]
        {
            new Vector3(-0.5f, -0.5f, -0.5f),
            new Vector3(0.5f, -0.5f, -0.5f),
            new Vector3(0.5f, 0.5f, -0.5f),
            new Vector3(-0.5f, 0.5f, -0.5f),
            new Vector3(-0.5f, -0.5f, 0.5f),
            new Vector3(0.5f, -0.5f, 0.5f),
            new Vector3(0.5f, 0.5f, 0.5f),
            new Vector3(-0.5f, 0.5f, 0.5f)
        };
        mesh.triangles = new int[]
        {
            // Front face
            0, 2, 1,
            0, 3, 2,
            // Top face
            3, 7, 2,
            2, 7, 6,
            // Right face
            1, 2, 6,
            1, 6, 5,
            // Left face
            0, 4, 7,
            0, 7, 3,
            // Back face
            4, 5, 6,
            4, 6, 7,
            // Bottom face
            0, 1, 5,
            0, 5, 4
        };
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }
    public static Mesh MakeCylinder(int sides)
    {
        Mesh mesh = new Mesh();

        // Vertices: bottom circle, top circle
        List<Vector3> vertices = new List<Vector3>();
        float radius = 0.5f;
        float height = 1.0f;

        // Bottom and top center points
        vertices.Add(new Vector3(0, -height / 2, 0)); // bottom center
        vertices.Add(new Vector3(0, height / 2, 0));  // top center

        // Circle points
        for (int i = 0; i < sides; i++)
        {
            float angle = 2 * Mathf.PI * i / sides;
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;
            vertices.Add(new Vector3(x, -height / 2, z)); // bottom ring
            vertices.Add(new Vector3(x, height / 2, z));  // top ring
        }

        List<int> triangles = new List<int>();

        // Bottom face (fan)
        for (int i = 0; i < sides; i++)
        {
            int curr = 2 + i * 2;
            int next = 2 + ((i + 1) % sides) * 2;
            triangles.Add(0);      // bottom center
            triangles.Add(next);   // next bottom
            triangles.Add(curr);   // current bottom
        }

        // Top face (fan)
        for (int i = 0; i < sides; i++)
        {
            int curr = 3 + i * 2;
            int next = 3 + ((i + 1) % sides) * 2;
            triangles.Add(1);      // top center
            triangles.Add(curr);   // current top
            triangles.Add(next);   // next top
        }

        // Side faces (quads split into triangles)
        for (int i = 0; i < sides; i++)
        {
            int b0 = 2 + i * 2;
            int t0 = b0 + 1;
            int b1 = 2 + ((i + 1) % sides) * 2;
            int t1 = b1 + 1;

            // First triangle
            triangles.Add(b0);
            triangles.Add(t0);
            triangles.Add(t1);

            // Second triangle
            triangles.Add(b0);
            triangles.Add(t1);
            triangles.Add(b1);
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    public static Mesh MakeSphere(int sides)
    {
        Mesh mesh = new Mesh();
        int latitudeBands = sides;
        int longitudeBands = sides;
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        for (int latNumber = 0; latNumber <= latitudeBands; latNumber++)
        {
            float theta = latNumber * Mathf.PI / latitudeBands;
            float sinTheta = Mathf.Sin(theta);
            float cosTheta = Mathf.Cos(theta);

            for (int longNumber = 0; longNumber <= longitudeBands; longNumber++)
            {
                float phi = longNumber * 2 * Mathf.PI / longitudeBands;
                float sinPhi = Mathf.Sin(phi);
                float cosPhi = Mathf.Cos(phi);

                Vector3 vertex = new Vector3(cosPhi * sinTheta, cosTheta, sinPhi * sinTheta);
                vertices.Add(vertex);
            }
        }

        for (int latNumber = 0; latNumber < latitudeBands; latNumber++)
        {
            for (int longNumber = 0; longNumber < longitudeBands; longNumber++)
            {
                int first = (latNumber * (longitudeBands + 1)) + longNumber;
                int second = first + longitudeBands + 1;

                triangles.Add(first);
                triangles.Add(second);
                triangles.Add(first + 1);

                triangles.Add(second);
                triangles.Add(second + 1);
                triangles.Add(first + 1);
            }
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    public static byte[] ExtractData(PointCloud2Msg data, int maxPts, VizType vizType, out int numPts)
    {

        /**
        For different data type the order is 
        Lidar: x, y, z, intensity
        RGBD: x, y, z, rgb0
        RGBD Mesh: ??
        Splat: x, y, z, rgba, scalex, scaley, scalez, rot0, rot1, rot2, rot3, nx, ny, nz, fc_dc_0, fc_dc_1, fc_dc_2, opacity
        */

        // Just in case...
        if (maxPts < 1) maxPts = 1;
        int decmiator = 1;

        int data_size = vizType.GetSize();

        numPts = (int)(data.data.Length / data.point_step);

        if (numPts > maxPts)
        {
            decmiator = Mathf.CeilToInt((float)numPts / maxPts);
            numPts = numPts / decmiator;
        }

        byte[] outData = new byte[numPts * data_size];

        // For each point...
        for (int i = 0; i < numPts; i++)
        {
            // Grab the point at the decimated index
            int inIdx = (int)(i * data.point_step * (decmiator));
            int outIdx = i * data_size;

            // For each field in the point...
            for (int j = 0; j < vizType.GetFieldCount(); j++)
            {
                if (j >= data.fields.Length)
                {
                    // Debug.LogWarning($"LidarUtils: Field index {j} out of bounds for fields count {data.fields.Length}. Filling with 0.");

                    // If we are missing the last field (ie xyz only) then fill with 0
                    for (int k = 0; k < 4; k++)
                    {
                        outData[outIdx + j * 4 + k] = 0;
                    }
                    continue;

                }
                // Copy the 4 bytes in the float
                for (int k = 0; k < 4; k++)
                {
                    outData[outIdx + j * 4 + k] = data.data[inIdx + (int)data.fields[j].offset + k];
                }
            }
        }
        return outData;
    }
}
