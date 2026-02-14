using System;
using System.Collections.Generic;
using System.Linq;

namespace DungeonShooter
{
    /// <summary>
    /// Entity의 스킬을 관리하는 Pure C# 객체.
    /// 스킬 등록/해제만 담당하며, 스킬 사용은 Skill 인스턴스를 직접 호출한다.
    /// </summary>
    public class EntitySkillContainer
    {
        private readonly HashSet<Skill> _skills = new();
        
        public event Action<Skill> OnSkillRegisted;
        public event Action<Skill> OnSkillUnregisted;
        
        private IEventBus _eventBus;

        public EntitySkillContainer(IEventBus eventBus)
        {
            _eventBus = eventBus;
        }
        
        public IReadOnlyList<Skill> GetRegistedSkills()
        {
            return _skills.ToList();
        }

        /// <summary>
        /// 스킬을 변경하고 스킬 변경 이벤트를 실행합니다.
        /// </summary>
        public bool SkillLevelChange(Skill before, Skill after)
        {
            if (!_skills.Contains(before))
            {
                LogHandler.LogError<EntitySkillContainer>($"이전 스킬이 존재하지 않습니다");
                return false;
            }

            if (_skills.Contains(after))
            {
                LogHandler.LogError<EntitySkillContainer>($"변경하려는 스킬이 이미 존재합니다");
                return false;
            }
            
            _eventBus.Publish(new SkillLevelUpEvent {beforeSkill = before, afterSkill = after});
            
            after.StartCooldown(before.Cooldown);
            Unregist(before);
            Regist(after);
            return true;
        }
        
        /// <summary>
        /// 스킬을 등록합니다. 등록 후 OnSkillRegisted를 발생시킵니다.
        /// </summary>
        public void Regist(Skill skill)
        {
            if (skill == null)
            {
                LogHandler.LogWarning<EntitySkillContainer>("등록할 스킬이 null입니다.");
                return;
            }

            if (_skills.Contains(skill))
            {
                LogHandler.LogWarning<EntitySkillContainer>($"이미 등록된 스킬입니다: {skill.SkillTableEntry.SkillName}");
                return;
            }

            _skills.Add(skill);
            OnSkillRegisted?.Invoke(skill);
            LogHandler.Log<EntitySkillContainer>($"스킬 등록 완료: {skill.SkillTableEntry.Id} ({skill.SkillTableEntry.SkillName})");
        }

        /// <summary>
        /// 스킬 등록을 해제합니다. OnSkillUnregisted 발생 후 리소스를 정리합니다.
        /// </summary>
        public void Unregist(Skill skill)
        {
            if (skill == null)
            {
                LogHandler.LogWarning<EntitySkillContainer>("스킬해제 실패: 스킬이 null입니다.");
                return;
            }

            if (!_skills.Remove(skill))
            {
                LogHandler.LogWarning<EntitySkillContainer>($"스킬해제 실패: 등록되지 않은 스킬입니다: {skill.SkillTableEntry?.SkillName}");
                return;
            }

            OnSkillUnregisted?.Invoke(skill);
            LogHandler.Log<EntitySkillContainer>($"스킬 등록 해제 완료: {skill.SkillTableEntry?.Id}");
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
