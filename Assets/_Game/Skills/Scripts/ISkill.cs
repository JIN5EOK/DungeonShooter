using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace DungeonShooter
{
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
    /// 스킬을 실행합니다. (액티브 스킬 사용 시 호출)
    /// <returns>실행 성공 여부</returns>
    UniTask<bool> Execute(EntityBase target);
    
    /// <summary>
    /// 패시브 효과를 활성화합니다.
    /// </summary>
    void Activate(EntityBase owner);
    
    /// <summary>
    /// 패시브 효과를 비활성화합니다.
    /// </summary>
    void Deactivate(EntityBase owner);
    }
}
