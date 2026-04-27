using System;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonShooter
{
    [Serializable]
    public class EnemySpawnInfo
    {
        [field: SerializeField] public float TriggerTime { get; private set; }
        [field: SerializeField] public SpawnRule Rule { get; private set; } = new();
    }

    [CreateAssetMenu(fileName = "StageConfig", menuName = "DungeonShooter/Stage/StageConfig")]
    public class StageConfig : ScriptableObject
    {
        [field: SerializeField] public float StageDuration { get; private set; } = 1800f;
        [field: SerializeField] public PlayerConfigSo PlayerConfig { get; private set; }
        [field: SerializeField] public List<EnemySpawnInfo> SpawnInfos { get; private set; } = new();
    }
}
