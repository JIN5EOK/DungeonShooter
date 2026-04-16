namespace DungeonShooter
{
    public sealed class EnemyConfigDto : ITableEntry
    {
        public string Memo { get; set; }
        public int Id { get; set; }

        /// <summary>적 이름 (LocalizedString 직렬화)</summary>
        public string Name { get; set; }

        /// <summary>적 프리팹 어드레서블 키(GUID)</summary>
        public string GameObjectKey { get; set; }

        /// <summary>AI 동작 타입</summary>
        public string AITypeKey { get; set; }

        public int MaxHp { get; set; }
        public int Attack { get; set; }
        public int Defense { get; set; }
        public int MoveSpeed { get; set; }

        /// <summary>처치 시 획득 경험치</summary>
        public int Exp { get; set; }

        /// <summary>활성 스킬 키(GUID) 목록. 예: "guid1/guid2/guid3"</summary>
        public string ActiveSkills { get; set; }

        public EnemyConfigDto(
            int id,
            string name,
            string gameObjectKey,
            string aiTypeKey,
            int maxHp,
            int attack,
            int defense,
            int moveSpeed,
            int exp,
            string activeSkills,
            string memo = "")
        {
            Id = id;
            Name = name;
            GameObjectKey = gameObjectKey;
            AITypeKey = aiTypeKey;
            MaxHp = maxHp;
            Attack = attack;
            Defense = defense;
            MoveSpeed = moveSpeed;
            Exp = exp;
            ActiveSkills = activeSkills;
            Memo = memo;
        }

        public EnemyConfigDto() { }
    }
}

