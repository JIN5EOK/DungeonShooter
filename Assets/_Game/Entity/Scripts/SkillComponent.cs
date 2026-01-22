using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

namespace DungeonShooter
{
    /// <summary>
    /// Entity의 스킬을 관리하는 컴포넌트
    /// </summary>
    public class SkillComponent : MonoBehaviour
    {
    private Dictionary<string, ISkill> _skills = new Dictionary<string, ISkill>();
    private IStageResourceProvider _resourceProvider;
    private EntityBase _owner;
    
    [Inject]
    private void Construct(IStageResourceProvider resourceProvider)
    {
        _resourceProvider = resourceProvider;
    }
    
    private void Awake()
    {
        _owner = GetComponent<EntityBase>();
        if (_owner == null)
        {
            LogHandler.LogError<SkillComponent>("EntityBase 컴포넌트를 찾을 수 없습니다.");
        }
    }
    
    /// <summary>
    /// 스킬을 사용합니다.
    /// </summary>
    /// <param name="skillKey">사용할 스킬의 키</param>
    /// <param name="target">스킬에 적중된 Entity (선택적)</param>
    /// <returns>사용 성공 여부</returns>
    public async UniTask<bool> UseSkill(string skillKey, EntityBase target = null)
    {
        if (!_skills.TryGetValue(skillKey, out var skill))
        {
            LogHandler.LogWarning<SkillComponent>($"스킬을 찾을 수 없습니다: {skillKey}");
            return false;
        }
        
        if (_owner == null)
        {
            LogHandler.LogError<SkillComponent>("Owner가 null입니다.");
            return false;
        }
        
        return await skill.Execute(_owner);
    }
    
    /// <summary>
    /// 스킬을 등록합니다.
    /// </summary>
    /// <param name="skillKey">스킬의 Addressable 키</param>
    /// <returns>등록 성공 여부</returns>
    public async UniTask<bool> RegistSkill(string skillKey)
    {
        if (string.IsNullOrEmpty(skillKey))
        {
            LogHandler.LogError<SkillComponent>("skillKey가 null이거나 비어있습니다.");
            return false;
        }
        
        if (_skills.ContainsKey(skillKey))
        {
            LogHandler.LogWarning<SkillComponent>($"이미 등록된 스킬입니다: {skillKey}");
            return false;
        }
        
        if (_resourceProvider == null)
        {
            LogHandler.LogError<SkillComponent>("ResourceProvider가 null입니다.");
            return false;
        }
        
        try
        {
            // SkillData 로드
            var skillData = await _resourceProvider.GetAsset<SkillData>(skillKey);
            
            if (skillData == null)
            {
                LogHandler.LogError<SkillComponent>($"SkillData를 로드할 수 없습니다: {skillKey}");
                return false;
            }
            
            // ISkill 인스턴스 생성
            var skill = new Skill(skillData);
            _skills[skillKey] = skill;
            
            // 패시브 효과 자동 활성화
            if (skillData.IsPassiveSkill)
            {
                skill.Activate(_owner);
            }
            
            LogHandler.Log<SkillComponent>($"스킬 등록 완료: {skillKey} ({skillData.SkillName})");
            return true;
        }
        catch (Exception e)
        {
            LogHandler.LogError<SkillComponent>(e, "스킬 등록 중 오류 발생");
            return false;
        }
    }
    
    /// <summary>
    /// 스킬 등록을 해제합니다.
    /// </summary>
    /// <param name="skillKey">스킬의 키</param>
    /// <returns>해제 성공 여부</returns>
    public bool UnRegistSkill(string skillKey)
    {
        if (string.IsNullOrEmpty(skillKey))
        {
            LogHandler.LogError<SkillComponent>("skillKey가 null이거나 비어있습니다.");
            return false;
        }
        
        if (!_skills.TryGetValue(skillKey, out var skill))
        {
            LogHandler.LogWarning<SkillComponent>($"등록되지 않은 스킬입니다: {skillKey}");
            return false;
        }
        
        // 패시브 효과 비활성화
        if (skill.SkillData.IsPassiveSkill)
        {
            skill.Deactivate(_owner);
        }
        
        // 리소스 정리
        if (skill is IDisposable disposable)
        {
            disposable.Dispose();
        }
        
        _skills.Remove(skillKey);
        LogHandler.Log<SkillComponent>($"스킬 등록 해제 완료: {skillKey}");
        
        return true;
    }
    
    private void OnDestroy()
    {
        // 모든 스킬 리소스 정리
        foreach (var skill in _skills.Values)
        {
            skill.Dispose();
        }
        
            _skills.Clear();
        }
    }
}
