using System.Collections.Generic;
using UnityEngine;

public class CooldownManager
{
    private Dictionary<string, float> cooldowns = new Dictionary<string, float>();
    private Dictionary<string, float> cooldownTimers = new Dictionary<string, float>();

    public void RegisterCooldown(string name, float cooldownDuration)
    {
        cooldowns[name] = cooldownDuration;
        cooldownTimers[name] = 0f;
    }

    public bool IsReady(string name)
    {
        return cooldownTimers.ContainsKey(name) && cooldownTimers[name] <= 0f;
    }

    public void StartCooldown(string name)
    {
        if (cooldowns.ContainsKey(name))
        {
            cooldownTimers[name] = cooldowns[name];
        }
    }

    public void UpdateCooldowns()
    {
        var keys = new List<string>(cooldownTimers.Keys);
        foreach (string key in keys)
        {
            if (cooldownTimers[key] > 0)
            {
                cooldownTimers[key] -= Time.deltaTime;
            }
        }
    }

    public float GetCooldownPercent(string name)
    {
        if (!cooldowns.ContainsKey(name) || !cooldownTimers.ContainsKey(name))
            return 0f;

        return Mathf.Max(0, cooldownTimers[name] / cooldowns[name]);
    }

    /// <summary>
    /// 남은 쿨다운 시간 반환 (UI용)
    /// </summary>
    public float GetRemainingCooldown(string name)
    {
        if (!cooldownTimers.ContainsKey(name))
            return 0f;

        return Mathf.Max(0f, cooldownTimers[name]);
    }

    /// <summary>
    /// 총 쿨다운 시간 반환 (UI용)
    /// </summary>
    public float GetTotalCooldown(string name)
    {
        if (!cooldowns.ContainsKey(name))
            return 0f;

        return cooldowns[name];
    }
}
