using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using RosMessageTypes.Geometry;
using RosMessageTypes.Std;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using UnityEngine.Timeline;


public class MarkerPointStream : MonoBehaviour, IMarkerViz
{

    public Material point_material;

    GraphicsBuffer _meshTriangles;
    GraphicsBuffer _meshVertices;
    GraphicsBuffer _ptData;

    public float scale = .01f;
    public int maxPts = 100_000;
    public int sides = 3;
    private RenderParams renderParams;
    private Mesh mesh;
    public bool _enabled = true;
    public int _numPts = 0;
    public MarkerType markerType = MarkerType.Points;



    // Start is called before the first frame update
    void Awake()
    {



        // mesh = LidarUtils.MakePolygon(sides);
        mesh = LidarUtils.MakeCube();


        _meshTriangles = new GraphicsBuffer(GraphicsBuffer.Target.Structured, mesh.triangles.Length, sizeof(int));
        _meshTriangles.SetData(mesh.triangles);
        _meshVertices = new GraphicsBuffer(GraphicsBuffer.Target.Structured, mesh.vertices.Length, 3 * sizeof(float));
        _meshVertices.SetData(mesh.vertices);
        _ptData = new GraphicsBuffer(GraphicsBuffer.Target.Structured, maxPts, 4);


        renderParams = new RenderParams(point_material);

        renderParams.worldBounds = new Bounds(Vector3.zero, Vector3.one * 100);
        renderParams.matProps = new MaterialPropertyBlock();

        renderParams.matProps.SetMatrix("_ObjectToWorld", Matrix4x4.Translate(new Vector3(0, 0, 0)));
        renderParams.matProps.SetFloat("_PointSize", scale);
        renderParams.matProps.SetBuffer("_PointData", _ptData);

    }


    private void Update()
    {
        if (_enabled && mesh != null)
        {
            renderParams.matProps.SetMatrix("_ObjectToWorld", transform.localToWorldMatrix);
            Graphics.RenderPrimitivesIndexed(renderParams, MeshTopology.Triangles, _meshTriangles, _meshTriangles.count, (int)mesh.GetIndexStart(0), _numPts);
        }
    }

    public void SetMesh(Mesh newMesh)
    {
        if (newMesh == null)
        {
            Debug.LogError("MarkerPointStream: SetMesh called with null mesh");
            return;
        }

        _meshTriangles?.Dispose();
        _meshTriangles = new GraphicsBuffer(GraphicsBuffer.Target.Structured, newMesh.triangles.Length, sizeof(int));
        _meshTriangles.SetData(newMesh.triangles);

        _meshVertices?.Dispose();
        _meshVertices = new GraphicsBuffer(GraphicsBuffer.Target.Structured, newMesh.vertices.Length, 3 * sizeof(float));
        _meshVertices.SetData(newMesh.vertices);

        renderParams.matProps.SetBuffer("_Positions", _meshVertices);
    }



    private void OnDestroy()
    {
        _meshTriangles?.Dispose();
        _meshTriangles = null;
        _meshVertices?.Dispose();
        _meshVertices = null;
        _ptData?.Dispose();
        _ptData = null;
    }


    public void SetData(PoseMsg pose, Vector3Msg scale, ColorRGBAMsg[] colors, PointMsg[] points)
    {
        if (points == null || points.Length == 0)
        {
            _ptData.SetData(new Vector4[0]);
            return;
        }

        Vector4[] pointData = new Vector4[points.Length];
        for (int i = 0; i < points.Length; i++)
        {
            Vector3 pt = points[i].From<FLU>();
            float color = colors.Length == 1 ? PackRGBA(colors[0]) : PackRGBA(colors[i]);
            pointData[i] = new Vector4(pt.x, pt.y, pt.z, color);
        }

        _ptData.SetData(pointData);
        _numPts = points.Length;
    }

    public static float PackRGBA(ColorRGBAMsg color)
    {
        // Pack a ColorRGBAMsg (r,g,b,a in 0-1 range) into a float
        int r = Mathf.Clamp(Mathf.RoundToInt(color.r * 255f), 0, 255);
        int g = Mathf.Clamp(Mathf.RoundToInt(color.g * 255f), 0, 255);
        int b = Mathf.Clamp(Mathf.RoundToInt(color.b * 255f), 0, 255);
        int a = 255; // Fixed alpha
        int packed = (r << 16) | (g << 8) | (b);
        return System.BitConverter.Int32BitsToSingle(packed);
    }


    private void OnValidate()
    {
        if (renderParams.matProps != null)
        {
            renderParams.matProps.SetFloat("_PointSize", scale);
        }
    }
    
    
    public void OnSizeChange(float size)
    {
        scale = size / 10f;
        renderParams.matProps.SetFloat("_PointSize", scale);
    }



}
