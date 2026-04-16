namespace DungeonShooter
{
    public sealed class SerializedPlayerConfigTableDto : ITableEntry
    {
        public string Memo { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string GameObjectKey { get; set; }
        public string Skill1Key { get; set; }
        public string Skill2Key { get; set; }
        public string Skills { get; set; }
        public int MaxHp { get; set; }
        public int Attack { get; set; }
        public int Defense { get; set; }
        public int MoveSpeed { get; set; }

        public SerializedPlayerConfigTableDto(
            int id,
            string name,
            string description,
            string gameObjectKey,
            string skill1Key,
            string skill2Key,
            int maxHp,
            int attack,
            int defense,
            int moveSpeed,
            string skills,
            string memo = "")
        {
            Id = id;
            Name = name;
            Description = description;
            GameObjectKey = gameObjectKey;
            Skill1Key = skill1Key;
            Skill2Key = skill2Key;
            MaxHp = maxHp;
            Attack = attack;
            Defense = defense;
            MoveSpeed = moveSpeed;
            Skills = skills;
            Memo = memo;
        }

        public SerializedPlayerConfigTableDto() { }
    }
}