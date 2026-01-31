using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DungeonShooter;
using UnityEngine;
using UnityEngine.AddressableAssets;
using VContainer;

namespace DungeonShooter
{
    /// <summary>
    /// 스킬 효과 리스트와 기타 정보를 담는 ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "New Skill", menuName = "Game/Skill")]
    public class SkillData : ScriptableObject
    {
        public IReadOnlyList<EffectBase> ActiveEffects => _activeEffects;
        public IReadOnlyList<EffectBase> PassiveEffects => _passiveEffects;
        public bool IsActiveSkill => _activeEffects.Count > 0;
        public bool IsPassiveSkill => _passiveEffects.Count > 0;

        [Header("액티브 스킬 효과")]
        [SerializeReference]
        private List<EffectBase> _activeEffects = new List<EffectBase>();

        [Header("패시브 스킬 효과")]
        [SerializeReference]
        private List<EffectBase> _passiveEffects = new List<EffectBase>();

        // 이펙트에 의존성 주입
        [Inject]
        private void Construct(ISceneResourceProvider provider)
        {
            foreach (var effect in _activeEffects)
            {
                effect.Initialize(provider);
            }
            
            foreach(var effect in _passiveEffects)
            {
                effect.Initialize(provider);
            }
        }
    }
}
