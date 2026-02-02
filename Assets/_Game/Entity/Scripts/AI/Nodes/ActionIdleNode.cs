using Jin5eok;

namespace DungeonShooter
{
    /// <summary>
    /// 아무 행동도 하지 않는 Leaf 노드입니다.
    /// </summary>
    public class ActionIdleNode : LeafNode<AiBTContext>
    {
        /// <inheritdoc />
        public override BTStatus Execute(AiBTContext context)
        {
            if (context.Self == null)
            {
                return BTStatus.Failure;
            }

            var movement = context.Self.GetComponent<MovementComponent>();
            if (movement != null)
            {
                movement.Direction = UnityEngine.Vector2.zero;
            }

            return BTStatus.Success;
        }
    }
}
