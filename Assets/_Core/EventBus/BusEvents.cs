using UnityEngine;

namespace DungeonShooter
{
    public struct PlayerObjectSpawnEvent
    {
        public PlayerConfigTableEntry playerConfigTableEntry;
        public EntityBase player;
        public Vector2 position;
    }

    public struct PlayerDeadEvent
    {
        public PlayerConfigTableEntry playerConfigTableEntry;
        public EntityBase player;
        public Vector2 position;
    }

    public struct PlayerObjectDestroyEvent
    {
        public EntityBase player;
        public Vector2 position;
    }

    public struct EnemyDeadEvent
    {
        public EntityBase enemy;
        public EnemyConfigTableEntry enemyConfigTableEntry;
    }

    public struct EnemySpawnedEvent
    {
        public EntityBase enemy;
    }

    public struct AllEnemiesEliminatedEvent
    {
    }

    public struct PlayerLevelChangeEvent
    {
        public int level;
    }
    
    public struct SkillLevelUpEvent
    {
        public Skill beforeSkill;
        public Skill afterSkill;
    }
}