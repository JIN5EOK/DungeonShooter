using JetBrains.Annotations;

namespace DungeonShooter
{
    public sealed class SerializedSkillTableRow : IIntId
    {
        public int Id { get; set; }
        public string SkillName { get; set; }
        public string SkillDescription { get; set; }
        public string SkillIconKey { get; set; }
        public int? Level { get; set; }
        public string SkillDataKey { get; set; }
        public int? Amount { get; set; }
        public float? Cooldown { get; set; }

        public SerializedSkillTableRow(int id, string skillNameId, string skillDescriptionId, string skillIconKey,
            int? level, string skillDataKey, int? amount, float? cooldown)
        {
            Id = id;
            SkillName = skillNameId;
            SkillDescription = skillDescriptionId;
            SkillIconKey = skillIconKey;
            Level = level;
            SkillDataKey = skillDataKey;
            Amount = amount;
            Cooldown = cooldown;
        }
        public SerializedSkillTableRow(){}
    }
}

