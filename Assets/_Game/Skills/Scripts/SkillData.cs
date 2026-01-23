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
        public string SkillName => _skillName;
        public string SkillDescription => _skillDescription;
        public IReadOnlyList<EffectBase> ActiveEffects => _activeEffects;
        public IReadOnlyList<EffectBase> PassiveEffects => _passiveEffects;
        public bool IsActiveSkill => _activeEffects.Count > 0;
        public bool IsPassiveSkill => _passiveEffects.Count > 0;
        public string SkillIconAddress => _skillIcon.RuntimeKey.ToString();

        [Header("스킬 기본 정보")]
        [SerializeField] private string _skillName;
        [SerializeField, TextArea(3, 10)] private string _skillDescription;
        [SerializeField] private AssetReferenceT<Sprite> _skillIcon;

        [Header("액티브 스킬 효과")]
        [SerializeReference]
        private List<EffectBase> _activeEffects = new List<EffectBase>();

        [Header("패시브 스킬 효과")]
        [SerializeReference]
        private List<EffectBase> _passiveEffects = new List<EffectBase>();

        // 이펙트에 의존성 주입
        [Inject]
        private void Construct(IObjectResolver resolver)
        {
            foreach(var effect in _activeEffects)
            {
                resolver.Inject(effect);
            }
            
            foreach(var effect in _passiveEffects)
            {
                resolver.Inject(effect);
            }
        }
    }
}
