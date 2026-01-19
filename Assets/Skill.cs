using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Skill", menuName = "New Skill")]
public class Skill : ScriptableObject
{
    public string skillName;
    public string skillDescription;
    public Sprite skillIcon;
    [SerializeReference]
    public List<EffectBase> effects;
}
[Serializable]
public abstract class EffectBase
{

}
[Serializable]
public class HealEffect : EffectBase
{
    public int healAmount;
}
[Serializable]
public class SpawnProjectileEffect : EffectBase
{
    public GameObject projectilePrefab;
    public int projectileAmount;
    [SerializeReference]
    public List<EffectBase> effects;
}