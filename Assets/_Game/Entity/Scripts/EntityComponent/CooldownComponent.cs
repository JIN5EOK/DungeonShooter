using UnityEngine;

namespace DungeonShooter
{
    /// <summary>
    /// 쿨다운 관리를 담당하는 MonoBehaviour 컴포넌트
    /// Unity 생명주기와 통합하여 자동으로 쿨다운을 업데이트합니다.
    /// </summary>
    public class CooldownComponent : MonoBehaviour
    {
    private CooldownManager _manager = new CooldownManager();

    private void Update()
    {
        _manager.UpdateCooldowns(Time.deltaTime);
    }

    /// <summary>
    /// 쿨다운을 등록합니다.
    /// </summary>
    public void RegisterCooldown(string name, float cooldownDuration)
    {
        _manager.RegisterCooldown(name, cooldownDuration);
    }

    /// <summary>
    /// 쿨다운이 준비되었는지 확인합니다.
    /// </summary>
    public bool IsReady(string name)
    {
        return _manager.IsReady(name);
    }

    /// <summary>
    /// 쿨다운을 시작합니다.
    /// </summary>
    public void StartCooldown(string name)
    {
        _manager.StartCooldown(name);
    }

    /// <summary>
    /// 쿨다운 진행률을 반환합니다 (0~1).
    /// </summary>
    public float GetCooldownPercent(string name)
    {
        return _manager.GetCooldownPercent(name);
    }

    /// <summary>
    /// 남은 쿨다운 시간을 반환합니다 (UI용).
    /// </summary>
    public float GetRemainingCooldown(string name)
    {
        return _manager.GetRemainingCooldown(name);
    }

    /// <summary>
    /// 총 쿨다운 시간을 반환합니다 (UI용).
    /// </summary>
    public float GetTotalCooldown(string name)
    {
        return _manager.GetTotalCooldown(name);
    }
    }
}

