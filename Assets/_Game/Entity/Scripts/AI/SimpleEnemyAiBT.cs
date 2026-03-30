using Jin5eok;
using UnityEngine;

namespace DungeonShooter
{
    /// <summary>
    /// 플레이어에게 무조건 근접하는 적 AI입니다.
    /// </summary>
    [CreateAssetMenu(fileName = "SimpleEnemyAiBT", menuName = "DungeonShooter/AI/Simple Enemy AiBT", order = 1)]
    public class SimpleEnemyAIBt : AiBTBase
    {
        [SerializeField]
        [Header("플레이어 감지 거리")]
        private float _detectionRange = 1000f;

        public override IBehaviourTreeNode<AiBTContext> GetTree()
        {
            var chaseSequence = new SequencerNode<AiBTContext>()
                .AddChild(new ActionFindPlayerNode())
                .AddChild(new ConditionPlayerInRangeNode(_detectionRange))
                .AddChild(new ActionChaseNode());
            
            return chaseSequence;
        }
    }
}
