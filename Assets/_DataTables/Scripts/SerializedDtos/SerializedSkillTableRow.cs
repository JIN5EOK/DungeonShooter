using JetBrains.Annotations;

namespace DungeonShooter
{
    public sealed class SerializedSkillTableRow : IIntId
    {
        public int Id { get; set; }
        public int? SkillNameId { get; set; }
        public int? SkillDescriptionId { get; set; }
        public string SkillIconKey { get; set; }
        public int? Level { get; set; }
        public string SkillDataKey { get; set; }
        public int? Amount { get; set; }
        public float? Cooldown { get; set; }

        public SerializedSkillTableRow(int id, int? skillNameId, int? skillDescriptionId, string skillIconKey,
            int? level, string skillDataKey, int? amount, float? cooldown)
        {
            Id = id;
            SkillNameId = skillNameId;
            SkillDescriptionId = skillDescriptionId;
            SkillIconKey = skillIconKey;
            Level = level;
            SkillDataKey = skillDataKey;
            Amount = amount;
            Cooldown = cooldown;
        }
        public SerializedSkillTableRow(){}
    }
}

