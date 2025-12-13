using System.Collections.Generic;
using UnityEngine;

public class CooldownManager
{
    private Dictionary<string, float> _cooldowns = new Dictionary<string, float>();
    private Dictionary<string, float> _cooldownTimers = new Dictionary<string, float>();

    public void RegisterCooldown(string name, float cooldownDuration)
    {
        _cooldowns[name] = cooldownDuration;
        _cooldownTimers[name] = 0f;
    }

    public bool IsReady(string name)
    {
        return _cooldownTimers.ContainsKey(name) && _cooldownTimers[name] <= 0f;
    }

    public void StartCooldown(string name)
    {
        if (_cooldowns.ContainsKey(name))
        {
            _cooldownTimers[name] = _cooldowns[name];
        }
    }

    public void UpdateCooldowns()
    {
        var keys = new List<string>(_cooldownTimers.Keys);
        foreach (string key in keys)
        {
            if (_cooldownTimers[key] > 0)
            {
                _cooldownTimers[key] -= Time.deltaTime;
            }
        }
    }

    public float GetCooldownPercent(string name)
    {
        if (!_cooldowns.ContainsKey(name) || !_cooldownTimers.ContainsKey(name))
            return 0f;

        return Mathf.Max(0, _cooldownTimers[name] / _cooldowns[name]);
    }

    /// <summary>
    /// 남은 쿨다운 시간 반환 (UI용)
    /// </summary>
    public float GetRemainingCooldown(string name)
    {
        if (!_cooldownTimers.ContainsKey(name))
            return 0f;

        return Mathf.Max(0f, _cooldownTimers[name]);
    }

    /// <summary>
    /// 총 쿨다운 시간 반환 (UI용)
    /// </summary>
    public float GetTotalCooldown(string name)
    {
        if (!_cooldowns.ContainsKey(name))
            return 0f;

        return _cooldowns[name];
    }
}
