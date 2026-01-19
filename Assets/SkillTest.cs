using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Skill", menuName = "New Skill")]
public class SkillTest : ScriptableObject
{
    public string skillName;
    public string skillDescription;
    public Sprite skillIcon;
    [SerializeReference]
    public List<EffectBaseTest> effects;
}
[Serializable]
public abstract class EffectBaseTest
{

}
[Serializable]
public class HealEffectTest : EffectBaseTest
{
    public int healAmount;
}
[Serializable]
public class SpawnProjectileEffectTest : EffectBaseTest
{
    public GameObject projectilePrefab;
    public int projectileAmount;
    [SerializeReference]
    public List<EffectBaseTest> effects;
}