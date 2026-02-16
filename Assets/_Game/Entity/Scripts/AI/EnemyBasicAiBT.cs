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
        [Header("적 감지 거리")]
        private float _detectionRange = 10f;

        [SerializeField]
        [Header("적 감지 거리")]
        private float _attackRagne = 1f;
        
        [SerializeField]
        [Header("공격 스킬 인덱스")]
        private int _skillIndex = 0;
        
        public override IBehaviourTreeNode<AiBTContext> GetTree()
        {
            var chaseSequence = new SequencerNode<AiBTContext>()
                .AddChild(new ConditionPlayerInRangeNode(_detectionRange))
                .AddChild(new ActionChaseNode());

            var attackSequence = new SequencerNode<AiBTContext>()
                .AddChild(new ConditionPlayerInRangeNode(_attackRagne))
                .AddChild(new ActionUseActiveSkillNode(_skillIndex));

            var chaseOrMoveSelector = new SelectorNode<AiBTContext>()
                .AddChild(attackSequence)
                .AddChild(chaseSequence);
            
            var mainSelector = new SelectorNode<AiBTContext>()
                .AddChild(chaseOrMoveSelector)
                .AddChild(new ActionIdleNode());

            var findPlayerSequence = new SequencerNode<AiBTContext>()
                .AddChild(new ActionFindPlayerNode())
                .AddChild(mainSelector);

            var root = new SelectorNode<AiBTContext>()
                .AddChild(findPlayerSequence)
                .AddChild(new ActionIdleNode());

            return root;
        }
    }
}
