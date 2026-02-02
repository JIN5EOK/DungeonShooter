using Jin5eok;
using UnityEngine;

namespace DungeonShooter
{
    /// <summary>
    /// 행동트리를 실행하는 AI 컴포넌트입니다.
    /// </summary>
    public class AIComponent : MonoBehaviour
    {
        private AiBTContext _context;
        private AiBTBase _aiBT;
        private IBehaviourTreeNode<AiBTContext> _rootNode;

        private void Start()
        {
            _context = new AiBTContext();
            
        }

        private void Update()
        {
            if (_rootNode == null) return;
            
            RefreshContext();
            // if (ShouldSkipExecution())
            // {
            //     return;
            // }

            _rootNode.Execute(_context);
        }

        private bool ShouldSkipExecution()
        {
            if (_context.Self == null) return true;
            var health = _context.Self.GetComponent<HealthComponent>();
            return health == null || health.IsDead;
        }

        /// <summary>
        /// 사용할 행동트리 정의를 설정합니다.
        /// </summary>
        /// <param name="aiBT">행동트리 정의(스크립터블 오브젝트)</param>
        public void SetBT(AiBTBase aiBT)
        {
            _aiBT = aiBT;
            _rootNode = aiBT.GetTree();
        }

        private void RefreshContext()
        {
            if(_context.Self == null)
                _context.Self = GetComponent<EntityBase>();
                   
            if(_context.Target == null)
                _context.Target = FindFirstObjectByType<Player>();
            
            if (_aiBT != null)
            {
                _context.DetectionRange = _aiBT.GetDetectionRange();
            }
        }
    }
}
