using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 스킬 효과 리스트와 기타 정보를 담는 ScriptableObject
/// </summary>
[CreateAssetMenu(fileName = "New Skill", menuName = "Game/Skill")]
public class SkillData : ScriptableObject
{
    [Header("스킬 기본 정보")]
    public string skillName;
    public string skillDescription;
    public Sprite skillIcon;
    
    [Header("스킬 설정")]
    public float cooldown;
    
    [Header("스킬 효과")]
    [SerializeReference]
    public List<EffectBase> skillEffects = new List<EffectBase>();
}
