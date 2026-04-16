namespace DungeonShooter
{
    public sealed class SkillTableDto : ITableEntry
    {
        public string Memo { get; set; }
        public int Id { get; set; }
        public string SkillName { get; set; }
        public string SkillDescription { get; set; }
        public string SkillIconKey { get; set; }
        public int? Level { get; set; }
        public string SkillLevelName { get; set; }
        public string SkillLevelDescription { get; set; }
        public string SkillDataKey { get; set; }
        public int? Amount { get; set; }
        public float? Cooldown { get; set; }

        public SkillTableDto(int id, string skillNameId, string skillDescriptionId, string skillIconKey,
            int? level, string skillLevelName, string skillLevelDescription, string skillDataKey, int? amount, float? cooldown, string memo = "")
        {
            Id = id;
            SkillName = skillNameId;
            SkillDescription = skillDescriptionId;
            SkillIconKey = skillIconKey;
            Level = level;
            SkillLevelName = skillLevelName;
            SkillLevelDescription = skillLevelDescription;
            SkillDataKey = skillDataKey;
            Amount = amount;
            Cooldown = cooldown;
            Memo = memo;
        }
        public SkillTableDto(){}
    }
}

