Shader "Unlit/3dMeshRender"
{
    Properties
    {
        _ColorMin ("Intensity min", Color) = (0, 0, 0, 0)
        _ColorMax ("Intensity max", Color) = (1, 1, 1, 1)
        [Toggle] _ColorIntensity ("Color by intensity", Float) = 0
        [Toggle] _ColorRGB ("Color by RGB", Float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "Assets/Components/Lidar/Materials/Shaders/PointHelper.cginc"


            struct v2f
            {
                float4 pos: SV_POSITION;
                float4 color: COLOR0;
            };

            struct meshdata
            {
                float3 position;
                float color;
            };

            StructuredBuffer<float3> _Positions;
            StructuredBuffer<int> _MeshTriangles;
            StructuredBuffer<meshdata> _PointData;

            uniform uint _BaseVertexIndex;
            uniform float _PointSize;
            uniform float4x4 _ObjectToWorld;
            uniform float4 _ColorMin;
            uniform float4 _ColorMax;

            v2f vert (uint vertexID: SV_VertexID, uint instanceID: SV_InstanceID)
            {
                v2f o;
                float3 pos = _PointData[instanceID].position;
                float3 mesh_pt = _Positions[_BaseVertexIndex + vertexID] * _PointSize;
                float4 wpos = mul(_ObjectToWorld, float4(pos + mesh_pt, 1.0f));

                o.pos = mul(UNITY_MATRIX_VP, wpos);
                o.color = UnpackRGBA(_PointData[instanceID].color);

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return i.color;
            }
            ENDCG
        }
    }
}
