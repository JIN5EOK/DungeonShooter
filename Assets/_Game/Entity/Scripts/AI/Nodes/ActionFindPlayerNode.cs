using Jin5eok;
using UnityEngine;

namespace DungeonShooter
{
    /// <summary>
    /// 씬에서 플레이어를 찾아 컨텍스트 Target에 설정하는 Leaf 노드입니다.
    /// 검색 간격을 0.5~1.0초 사이 랜덤으로 두어 매 프레임 Find 호출과 부하 스파이크를 피합니다.
    /// </summary>
    public class ActionFindPlayerNode : LeafNode<AiBTContext>
    {
        private readonly float _searchInterval;
        private float _lastSearchTime = float.MinValue;
        private Player _cachedPlayer;

        public ActionFindPlayerNode()
        {
            // 한꺼번에 찾는거 조금이라도 줄이기 위해 대기시간 랜덤 간격으로 설정
            _searchInterval = Random.Range(0.5f, 1f);
        }

        /// <inheritdoc />
        public override BTStatus Execute(AiBTContext context)
        {
            if (Time.time - _lastSearchTime >= _searchInterval)
            {
                _cachedPlayer = Object.FindFirstObjectByType<Player>();
                _lastSearchTime = Time.time;
            }

            var valid = _cachedPlayer != null && _cachedPlayer;
            context.Target = valid ? _cachedPlayer : null;
            return valid ? BTStatus.Success : BTStatus.Failure;
        }
    }
}
