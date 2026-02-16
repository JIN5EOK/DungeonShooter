using Jin5eok;
using UnityEngine;

namespace DungeonShooter
{
    /// <summary>
    /// 씬에서 플레이어를 찾아 컨텍스트 Target에 설정하는 Leaf 노드입니다.
    /// </summary>
    public class ActionFindPlayerNode : LeafNode<AiBTContext>
    {
        // 부하 피하기 위해 랜덤 간격으로 서치
        private float _searchInterval = Random.Range(0.5f, 1f);
        private float _lastSearchTime = float.MinValue;
        private EntityBase _cachedTarget;

        /// <inheritdoc />
        public override BTStatus Execute(AiBTContext context)
        {
            if (context.Target != null)
                return BTStatus.Success;
            
            if (Time.time - _lastSearchTime >= _searchInterval)
            {
                var playerGo = GameObject.FindWithTag(GameTags.Player);
                _cachedTarget = playerGo != null ? playerGo.GetComponent<EntityBase>() : null;
                _lastSearchTime = Time.time;
            }

            var valid = _cachedTarget != null && _cachedTarget;
            context.Target = valid ? _cachedTarget : null;
            return valid ? BTStatus.Success : BTStatus.Failure;
        }
    }
}
