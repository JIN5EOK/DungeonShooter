using Jin5eok;
using UnityEngine;

namespace DungeonShooter
{
    /// <summary>
    /// 타겟(플레이어) 방향으로 방향만 전환합니다. 이동하지 않고 LastMoveDirection만 갱신합니다.
    /// </summary>
    public class ActionFacePlayerNode : LeafNode<AiBTContext>
    {
        /// <inheritdoc />
        public override BTStatus Execute(AiBTContext context)
        {
            if (context.Self == null || context.Target == null)
            {
                return BTStatus.Failure;
            }

            var direction = (context.Target.transform.position - context.Self.transform.position).normalized;
            context.Self.EntityInputContext.MoveInput = direction;
            context.Self.EntityInputContext.MoveInput = Vector2.zero;
            return BTStatus.Success;
        }
    }
}
