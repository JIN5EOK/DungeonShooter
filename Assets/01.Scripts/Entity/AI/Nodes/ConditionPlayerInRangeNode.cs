using Jin5eok;
using UnityEngine;

namespace DungeonShooter
{
    /// <summary>
    /// 플레이어가 감지 거리 안에 있는지 판정하는 Leaf 노드입니다.
    /// </summary>
    public class ConditionPlayerInRangeNode : LeafNode<AiBTContext>
    {
        private float _detectionRange;
        public ConditionPlayerInRangeNode(float detectionRange)
        {
            _detectionRange = detectionRange;
        }
        /// <inheritdoc />
        public override BTStatus Execute(AiBTContext context)
        {
            if (context.Self == null || context.Target == null)
            {
                return BTStatus.Failure;
            }

            var distance = Vector2.Distance(context.Self.transform.position, context.Target.transform.position);
            return distance <= _detectionRange ? BTStatus.Success : BTStatus.Failure;
        }
    }
}
