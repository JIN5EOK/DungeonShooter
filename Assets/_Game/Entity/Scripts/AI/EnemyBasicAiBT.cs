using Jin5eok;
using UnityEngine;

namespace DungeonShooter
{
    /// <summary>
    /// 기본 적 AI 행동트리 정의입니다.
    /// 플레이어가 감지 거리 안이면 추적, 아니면 대기합니다.
    /// </summary>
    [CreateAssetMenu(fileName = "EnemyBasicAiBT", menuName = "DungeonShooter/AI/Enemy Basic AiBT", order = 0)]
    public class EnemyBasicAiBT : AiBTBase
    {
        [SerializeField]
        [Tooltip("적 감지 거리")]
        private float _detectionRange = 10f;

        public override IBehaviourTreeNode<AiBTContext> GetTree()
        {
            var chaseSequence = new SequencerNode<AiBTContext>()
                .AddChild(new ConditionPlayerInRangeNode(_detectionRange))
                .AddChild(new ActionChaseNode());

            var mainSelector = new SelectorNode<AiBTContext>()
                .AddChild(chaseSequence)
                .AddChild(new ActionIdleNode());

            var withFindPlayer = new SequencerNode<AiBTContext>()
                .AddChild(new ActionFindPlayerNode())
                .AddChild(mainSelector);

            var root = new SelectorNode<AiBTContext>()
                .AddChild(withFindPlayer)
                .AddChild(new ActionIdleNode());

            return root;
        }
    }
}
