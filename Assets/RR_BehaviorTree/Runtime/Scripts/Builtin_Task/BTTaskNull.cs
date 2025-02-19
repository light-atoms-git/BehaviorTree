using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace RR.AI.BehaviorTree
{
    public class BTTaskNull : BTBaseTask
    {
        public override string Name => string.Empty;

        public override void Init(GameObject actor, RuntimeBlackboard blackboard, string nodeGuid)
        {}

        public override BTNodeState Tick(GameObject actor, RuntimeBlackboard blackboard, string nodeGuid) => BTNodeState.Success;
    }
}
