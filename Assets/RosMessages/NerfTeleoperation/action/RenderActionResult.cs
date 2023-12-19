using System.Collections.Generic;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;
using RosMessageTypes.Std;
using RosMessageTypes.Actionlib;

namespace RosMessageTypes.NerfTeleoperation
{
    public class RenderActionResult : ActionResult<RenderResult>
    {
        public const string k_RosMessageName = "nerf_teleoperation/RenderActionResult";
        public override string RosMessageName => k_RosMessageName;


        public RenderActionResult() : base()
        {
            this.result = new RenderResult();
        }

        public RenderActionResult(HeaderMsg header, GoalStatusMsg status, RenderResult result) : base(header, status)
        {
            this.result = result;
        }
        public static RenderActionResult Deserialize(MessageDeserializer deserializer) => new RenderActionResult(deserializer);

        RenderActionResult(MessageDeserializer deserializer) : base(deserializer)
        {
            this.result = RenderResult.Deserialize(deserializer);
        }
        public override void SerializeTo(MessageSerializer serializer)
        {
            serializer.Write(this.header);
            serializer.Write(this.status);
            serializer.Write(this.result);
        }


#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
#else
        [UnityEngine.RuntimeInitializeOnLoadMethod]
#endif
        public static void Register()
        {
            MessageRegistry.Register(k_RosMessageName, Deserialize);
        }
    }
}
