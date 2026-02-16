using Jin5eok;
using UnityEngine;

namespace DungeonShooter
{
    /// <summary>
    /// 캐스터형 적 AI 행동트리 정의입니다.
    /// 플레이어가 사거리 밖이면 근접하고, 너무 가까우면 퇴각하며, 스킬 사용 거리에서 스킬을 사용합니다.
    /// </summary>
    [CreateAssetMenu(fileName = "EnemyCasterAiBT", menuName = "DungeonShooter/AI/Enemy Caster AiBT", order = 1)]
    public class EnemyCasterAiBT : AiBTBase
    {
        [SerializeField]
        [Header("플레이어 감지 거리")]
        private float _detectionRange = 12f;
        
        [SerializeField]
        [Header("플레이어와의 후퇴 거리")]
        private float _tooCloseDistance = 6f;

        [SerializeField]
        [Header("스킬 사용 거리")]
        private float _skillUseDistance = 8f;

        [SerializeField]
        [Header("사용할 액티브 스킬 인덱스")]
        private int _skillIndex;

        public override IBehaviourTreeNode<AiBTContext> GetTree()
        {
            var retreatSequence = new SequencerNode<AiBTContext>()
                .AddChild(new ConditionPlayerInRangeNode(_tooCloseDistance))
                .AddChild(new ActionRetreatNode());

            var useSkillSequence = new SequencerNode<AiBTContext>()
                .AddChild(new ActionFacePlayerNode())
                .AddChild(new InverterNode<AiBTContext>().AddChild(new ConditionPlayerInRangeNode(_tooCloseDistance)))
                .AddChild(new ConditionPlayerInRangeNode(_skillUseDistance))
                .AddChild(new ActionUseActiveSkillNode(_skillIndex));     

            var chaseSequence = new SequencerNode<AiBTContext>()
                .AddChild(new ConditionPlayerInRangeNode(_detectionRange))
                .AddChild(new InverterNode<AiBTContext>().AddChild(new ConditionPlayerInRangeNode(_skillUseDistance)))
                .AddChild(new ActionChaseNode());

            var mainSelector = new SelectorNode<AiBTContext>()
                .AddChild(retreatSequence)
                .AddChild(useSkillSequence)
                .AddChild(chaseSequence)
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
