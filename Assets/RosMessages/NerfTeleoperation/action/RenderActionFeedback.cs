using System.Collections.Generic;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;
using RosMessageTypes.Std;
using RosMessageTypes.Actionlib;

namespace RosMessageTypes.NerfTeleoperation
{
    public class RenderActionFeedback : ActionFeedback<RenderFeedback>
    {
        public const string k_RosMessageName = "nerf_teleoperation/RenderActionFeedback";
        public override string RosMessageName => k_RosMessageName;


        public RenderActionFeedback() : base()
        {
            this.feedback = new RenderFeedback();
        }

        public RenderActionFeedback(HeaderMsg header, GoalStatusMsg status, RenderFeedback feedback) : base(header, status)
        {
            this.feedback = feedback;
        }
        public static RenderActionFeedback Deserialize(MessageDeserializer deserializer) => new RenderActionFeedback(deserializer);

        RenderActionFeedback(MessageDeserializer deserializer) : base(deserializer)
        {
            this.feedback = RenderFeedback.Deserialize(deserializer);
        }
        public override void SerializeTo(MessageSerializer serializer)
        {
            serializer.Write(this.header);
            serializer.Write(this.status);
            serializer.Write(this.feedback);
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
