using UnityEngine;

namespace DungeonShooter
{
    public struct PlayerObjectSpawnEvent
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

    public struct EnemyDestroyEvent
    {
        
        public EntityBase enemy;
        public EnemyConfigTableEntry enemyConfigTableEntry;
    }

    public struct PlayerLevelChangeEvent
    {
        public int level;
    }
    
    public struct ExpUpEvent
    {
        public int exp;
    }
    
    public struct SkillLevelUpEvent
    {
        public Skill beforeSkill;
        public Skill afterSkill;
    }
}