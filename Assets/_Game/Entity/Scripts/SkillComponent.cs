using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Entity의 스킬을 관리하는 컴포넌트
/// </summary>
public class SkillComponent : MonoBehaviour
{
    private Dictionary<string, ISkill> _skills = new Dictionary<string, ISkill>();
    
    /// <summary>
    /// 스킬을 사용합니다.
    /// </summary>
    /// <param name="skillKey">사용할 스킬의 키</param>
    /// <returns>사용 성공 여부</returns>
    public bool UseSkill(string skillKey)
    {
        // TODO: 구현 예정
        return false;
    }
    
    /// <summary>
    /// 스킬을 등록합니다.
    /// </summary>
    /// <param name="skillKey">스킬의 키</param>
    /// <returns>등록 성공 여부</returns>
    public bool RegistSkill(string skillKey)
    {
        // TODO: 구현 예정
        return false;
    }
    
    /// <summary>
    /// 스킬 등록을 해제합니다.
    /// </summary>
    /// <param name="skillKey">스킬의 키</param>
    /// <returns>해제 성공 여부</returns>
    public bool UnRegistSkill(string skillKey)
    {
        // TODO: 구현 예정
        return false;
    }
}
