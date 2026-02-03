using Jin5eok;
using UnityEngine;

namespace DungeonShooter
{
    /// <summary>
    /// 행동트리를 실행하는 AI 컴포넌트입니다.
    /// </summary>
    public class AIComponent : MonoBehaviour
    {
        private AiBTContext _context = new AiBTContext();
        private IBehaviourTreeNode<AiBTContext> _rootNode;

        private void Start()
        {
            _context.Self = GetComponent<EntityBase>();
        }

        private void Update()
        {
            if (_rootNode == null || _context == null)
                return;

            _rootNode.Execute(_context);
        }
        
        /// <summary>
        /// 사용할 행동트리 정의를 설정합니다.
        /// </summary>
        /// <param name="aiBT">행동트리 정의(스크립터블 오브젝트)</param>
        public void SetBT(AiBTBase aiBT)
        {
            _rootNode = aiBT.GetTree();
        }
    }
}
