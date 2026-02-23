using System;
using System.Collections.Generic;

namespace DungeonShooter
{
    /// <summary>
    /// Entity가 지닌 스킬 관련 인터페이스.
    /// </summary>
    public interface IEntitySkills
    {
        event Action<Skill> OnSkillRegisted;
        event Action<Skill> OnSkillUnregisted;
        bool Contains(Skill skill);
        IReadOnlyList<Skill> GetRegistedSkills();
        void Regist(Skill skill);
        void Unregist(Skill skill);
        void Clear();
    }
}
