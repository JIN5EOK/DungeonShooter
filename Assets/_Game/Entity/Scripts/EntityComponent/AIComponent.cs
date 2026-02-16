using System;
using System.Collections.Generic;
using Jin5eok;
using UnityEngine;
using VContainer;

namespace DungeonShooter
{
    /// <summary>
    /// 행동트리를 실행하는 AI 컴포넌트입니다.
    /// </summary>
    public class AIComponent : MonoBehaviour
    {
        private AiBTContext _context = new AiBTContext();
        private IBehaviourTreeNode<AiBTContext> _rootNode;

        [Inject]
        private void Construct(EntityBase entityBase)
        {
            _context.Self = entityBase;
        }

        public void Initialize(AiBTBase aiBT, List<Skill> activeSkills)
        {
            _rootNode = aiBT.GetTree();
            _context.ActiveSkills = activeSkills ?? new List<Skill>();
        }
        
        private void OnEnable()
        {
            _context.Target = null;
        }

        private void Update()
        {
            if (_rootNode == null || _context == null)
                return;

            _rootNode.Execute(_context);
        }
    }
}
