using System;
using System.Collections.Generic;

namespace DungeonShooter
{
    /// <summary>
    /// Entity가 지닌 스킬 관련 인터페이스.
    /// </summary>
    public interface IEntitySkills
    {
        public event Action<Skill> OnSkillRegisted;
        public event Action<Skill> OnSkillUnregisted;
        public bool Contains(Skill skill);
        public IReadOnlyList<Skill> GetRegistedSkills();
        public void Regist(Skill skill);
        public void Unregist(Skill skill);
        public void Clear();
    }
}
