using Jin5eok;
using UnityEngine;

namespace DungeonShooter
{
    /// <summary>
    /// 행동트리 정의의 추상 베이스입니다.
    /// 스크립터블 오브젝트로 에디터에서 수치 편집이 가능합니다.
    /// </summary>
    public abstract class AiBTBase : ScriptableObject
    {
        /// <summary>
        /// 이 행동트리 정의로부터 실행용 루트 노드를 생성합니다.
        /// </summary>
        /// <returns>행동트리 루트 노드</returns>
        public abstract IBehaviourTreeNode<AiBTContext> GetTree();

        /// <summary>
        /// 컨텍스트에 넣을 감지 거리입니다. 서브클래스에서 오버라이드합니다.
        /// </summary>
        public virtual float GetDetectionRange() => 0f;
    }
}
