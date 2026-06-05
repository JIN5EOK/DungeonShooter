using Jin5eok;
using UnityEngine;

namespace DungeonShooter
{
    /// <summary>
    /// 타겟에게서 멀어지도록 이동하도록 지시하는 Leaf 노드입니다.
    /// </summary>
    public class ActionRetreatNode : LeafNode<AiBTContext>
    {
        /// <inheritdoc />
        public override BTStatus Execute(AiBTContext context)
        {
            if (context.Self == null || context.Target == null)
            {
                return BTStatus.Failure;
            }

            var direction = (context.Self.transform.position - context.Target.transform.position).normalized;
            context.Self.EntityContext.InputContext.MoveInput = direction;
            return BTStatus.Success;
        }
    }
}
