using System;
using System.Collections.Generic;
using System.Linq;
using VContainer;

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
    
    /// <summary>
    /// Entity의 스킬을 관리하는 Pure C# 객체.
    /// 스킬 등록/해제만 담당하며, 스킬 사용은 Skill 인스턴스를 직접 호출한다.
    /// </summary>
    public class EntitySkills : IEntitySkills
    {
        private readonly HashSet<Skill> _skills = new();

        public event Action<Skill> OnSkillRegisted;
        public event Action<Skill> OnSkillUnregisted;

        public bool Contains(Skill skill)
        {
            return _skills.Contains(skill);
        }

        public IReadOnlyList<Skill> GetRegistedSkills()
        {
            return _skills.ToList();
        }

        /// <summary>
        /// 스킬을 등록합니다. 등록 후 OnSkillRegisted를 발생시킵니다.
        /// </summary>
        public void Regist(Skill skill)
        {
            if (skill == null)
            {
                LogHandler.LogWarning<EntitySkills>("등록할 스킬이 null입니다.");
                return;
            }

            if (_skills.Contains(skill))
            {
                LogHandler.LogWarning<EntitySkills>($"이미 등록된 스킬입니다: {skill.SkillTableEntry.SkillName}");
                return;
            }

            _skills.Add(skill);
            OnSkillRegisted?.Invoke(skill);
            LogHandler.Log<EntitySkills>($"스킬 등록 완료: {skill.SkillTableEntry.Id} ({skill.SkillTableEntry.SkillName})");
        }

        /// <summary>
        /// 스킬 등록을 해제합니다. OnSkillUnregisted 발생 후 리소스를 정리합니다.
        /// </summary>
        public void Unregist(Skill skill)
        {
            if (skill == null)
            {
                LogHandler.LogWarning<EntitySkills>("스킬해제 실패: 스킬이 null입니다.");
                return;
            }

            if (!_skills.Remove(skill))
            {
                LogHandler.LogWarning<EntitySkills>($"스킬해제 실패: 등록되지 않은 스킬입니다: {skill.SkillTableEntry?.SkillName}");
                return;
            }

            OnSkillUnregisted?.Invoke(skill);
            LogHandler.Log<EntitySkills>($"스킬 등록 해제 완료: {skill.SkillTableEntry?.Id}");
        }
        
        /// <summary>
        /// 등록된 모든 스킬에 대해 OnSkillUnregisted를 발생시키고 리소스를 정리합니다.
        /// </summary>
        public void Clear()
        {
            var copy = new List<Skill>(_skills);
            _skills.Clear();
            foreach (var skill in copy)
            {
                OnSkillUnregisted?.Invoke(skill);
            }
        }
    }
}
