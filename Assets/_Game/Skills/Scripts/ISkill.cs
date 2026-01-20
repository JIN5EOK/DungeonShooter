using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 스킬 인스턴스 인터페이스
/// </summary>
public interface ISkill : IDisposable
{
    SkillData SkillData { get; }
    bool IsCooldown { get; }
    float Cooldown { get; }
    
    event Action OnExecute;
    Action<float> OnCooldownChanged { get; set; }
    Action OnCooldownEnded { get; set; }
    
    /// <summary>
    /// 스킬을 실행합니다.
    /// </summary>
    /// <param name="target">스킬을 적용할 Entity</param>
    /// <returns>실행 성공 여부</returns>
    UniTask<bool> Execute(EntityBase target);
}
