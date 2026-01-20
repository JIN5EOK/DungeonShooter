using System.Collections.Generic;
using UnityEngine;

namespace DungeonShooter
{
    /// <summary>
    /// 쿨다운 관리를 담당하는 순수 로직 클래스
    /// MonoBehaviour 없이도 사용 가능하며, 테스트 및 재사용이 용이합니다.
    /// </summary>
    public class CooldownManager
    {
    private Dictionary<string, float> _cooldowns = new Dictionary<string, float>();
    private Dictionary<string, float> _cooldownTimers = new Dictionary<string, float>();

    /// <summary>
    /// 쿨다운을 등록합니다.
    /// </summary>
    public void RegisterCooldown(string name, float cooldownDuration)
    {
        _cooldowns[name] = cooldownDuration;
        _cooldownTimers[name] = 0f;
    }

    /// <summary>
    /// 쿨다운이 준비되었는지 확인합니다.
    /// </summary>
    public bool IsReady(string name)
    {
        return _cooldownTimers.ContainsKey(name) && _cooldownTimers[name] <= 0f;
    }

    /// <summary>
    /// 쿨다운을 시작합니다.
    /// </summary>
    public void StartCooldown(string name)
    {
        if (_cooldowns.ContainsKey(name))
        {
            _cooldownTimers[name] = _cooldowns[name];
        }
    }

    /// <summary>
    /// 쿨다운을 업데이트합니다. deltaTime을 인자로 받아 커스텀 시간 관리가 가능합니다.
    /// </summary>
    public void UpdateCooldowns(float deltaTime)
    {
        var keys = new List<string>(_cooldownTimers.Keys);
        foreach (string key in keys)
        {
            if (_cooldownTimers[key] > 0)
            {
                _cooldownTimers[key] -= deltaTime;
            }
        }
    }

    /// <summary>
    /// 쿨다운 진행률을 반환합니다 (0~1).
    /// </summary>
    public float GetCooldownPercent(string name)
    {
        if (!_cooldowns.ContainsKey(name) || !_cooldownTimers.ContainsKey(name))
            return 0f;

        return Mathf.Max(0, _cooldownTimers[name] / _cooldowns[name]);
    }

    /// <summary>
    /// 남은 쿨다운 시간을 반환합니다 (UI용).
    /// </summary>
    public float GetRemainingCooldown(string name)
    {
        if (!_cooldownTimers.ContainsKey(name))
            return 0f;

        return Mathf.Max(0f, _cooldownTimers[name]);
    }

    /// <summary>
    /// 총 쿨다운 시간을 반환합니다 (UI용).
    /// </summary>
    public float GetTotalCooldown(string name)
    {
        if (!_cooldowns.ContainsKey(name))
            return 0f;

        return _cooldowns[name];
    }
    }
}

