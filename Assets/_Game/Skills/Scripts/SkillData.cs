using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

/// <summary>
/// 스킬 효과 리스트와 기타 정보를 담는 ScriptableObject
/// </summary>
[CreateAssetMenu(fileName = "New Skill", menuName = "Game/Skill")]
public class SkillData : ScriptableObject
{
    public string SkillName => _skillName;
    public string SkillDescription => _skillDescription;
    public float Cooldown => _cooldown;
    public IReadOnlyList<EffectBase> SkillEffects => _skillEffects;
    public string SkillIconAddress => _skillIcon.RuntimeKey.ToString();

    [Header("스킬 기본 정보")]
    [SerializeField] private string _skillName;
    [SerializeField] private string _skillDescription;
    [SerializeField] private AssetReferenceT<Sprite> _skillIcon;
    
    [Header("스킬 설정")]
    [SerializeField] private float _cooldown;

    [Header("스킬 효과")]
    [SerializeReference]
    private List<EffectBase> _skillEffects = new List<EffectBase>();
}
