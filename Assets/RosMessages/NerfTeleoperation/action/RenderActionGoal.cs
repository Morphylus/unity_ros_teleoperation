using System.Collections.Generic;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;
using RosMessageTypes.Std;
using RosMessageTypes.Actionlib;

namespace RosMessageTypes.NerfTeleoperation
{
    public class RenderActionGoal : ActionGoal<RenderGoal>
    {
        public const string k_RosMessageName = "nerf_teleoperation/RenderActionGoal";
        public override string RosMessageName => k_RosMessageName;


        public RenderActionGoal() : base()
        {
            this.goal = new RenderGoal();
        }

        public RenderActionGoal(HeaderMsg header, GoalIDMsg goal_id, RenderGoal goal) : base(header, goal_id)
        {
            this.goal = goal;
        }
        public static RenderActionGoal Deserialize(MessageDeserializer deserializer) => new RenderActionGoal(deserializer);

        RenderActionGoal(MessageDeserializer deserializer) : base(deserializer)
        {
            this.goal = RenderGoal.Deserialize(deserializer);
        }
        public override void SerializeTo(MessageSerializer serializer)
        {
            serializer.Write(this.header);
            serializer.Write(this.goal_id);
            serializer.Write(this.goal);
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
