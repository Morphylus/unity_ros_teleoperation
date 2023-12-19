using System.Collections.Generic;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;


namespace RosMessageTypes.NerfTeleoperation
{
    public class RenderAction : Action<RenderActionGoal, RenderActionResult, RenderActionFeedback, RenderGoal, RenderResult, RenderFeedback>
    {
        public const string k_RosMessageName = "nerf_teleoperation/RenderAction";
        public override string RosMessageName => k_RosMessageName;


        public RenderAction() : base()
        {
            this.action_goal = new RenderActionGoal();
            this.action_result = new RenderActionResult();
            this.action_feedback = new RenderActionFeedback();
        }

        public static RenderAction Deserialize(MessageDeserializer deserializer) => new RenderAction(deserializer);

        RenderAction(MessageDeserializer deserializer)
        {
            this.action_goal = RenderActionGoal.Deserialize(deserializer);
            this.action_result = RenderActionResult.Deserialize(deserializer);
            this.action_feedback = RenderActionFeedback.Deserialize(deserializer);
        }

        public override void SerializeTo(MessageSerializer serializer)
        {
            serializer.Write(this.action_goal);
            serializer.Write(this.action_result);
            serializer.Write(this.action_feedback);
        }

    }
}
