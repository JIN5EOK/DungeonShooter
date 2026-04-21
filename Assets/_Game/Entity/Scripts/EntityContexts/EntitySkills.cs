using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;

namespace DungeonShooter
{
    /// <summary>
    /// Entity가 지닌 스킬 관련 인터페이스.
    /// 테이블 엔트리 ID당 스킬 인스턴스는 최대 하나입니다.
    /// </summary>
    public interface IEntitySkills
    {
        public event Action<Skill> OnSkillRegisted;
        public event Action<Skill> OnSkillUnregisted;

        public bool Contains(Skill skill);
        public bool ContainsSkill(int skillEntryId);

        public Skill GetSkill(int skillEntryId);

        public IReadOnlyList<Skill> GetSkills();

        public void RegistSkill(Skill skill);

        public void UnregistSkill(int skillEntryId);

        public UniTask<bool> ExecuteSkillAsync(int skillEntryId, EntityBase caster);

        public void Clear();
    }

    /// <summary>
    /// Entity의 스킬을 관리하는 Pure C# 객체.
    /// 스킬 등록/해제는 테이블 엔트리 ID를 기준으로 하며, 스킬 실행은 ID로 조회 후 수행합니다.
    /// </summary>
    public class EntitySkills : IEntitySkills
    {
        private readonly Dictionary<int, Skill> _skillsByEntryId = new();
        
        public event Action<Skill> OnSkillRegisted;
        public event Action<Skill> OnSkillUnregisted;

        public bool Contains(Skill skill)
        {
            var id = skill.SkillTableEntrySo.Id;
            return _skillsByEntryId.TryGetValue(id, out var registered) && ReferenceEquals(registered, skill);
        }

        public bool ContainsSkill(int skillEntryId)
        {
            return _skillsByEntryId.ContainsKey(skillEntryId);
        }

        public Skill GetSkill(int skillEntryId)
        {
            return _skillsByEntryId.TryGetValue(skillEntryId, out var skill) ? skill : null;
        }

        public IReadOnlyList<Skill> GetSkills()
        {
            return _skillsByEntryId.Values
                .OrderBy(s => s?.SkillTableEntrySo?.Id ?? 0)
                .ToList();
        }

        /// <summary>
        /// 스킬을 등록합니다. 동일 엔트리 ID가 이미 있으면 무시합니다.
        /// </summary>
        public void RegistSkill(Skill skill)
        {
            var entryId = skill.SkillTableEntrySo.Id;
            if (_skillsByEntryId.TryGetValue(entryId, out var existing))
            {
                if (ReferenceEquals(existing, skill))
                {
                    return;
                }

                var nameText = skill.SkillTableEntrySo.SkillName;
                LogHandler.LogWarning<EntitySkills>($"이미 동일 엔트리 ID의 스킬이 등록되어 있습니다: {entryId} ({nameText})");
                return;
            }

            _skillsByEntryId.Add(entryId, skill);
            OnSkillRegisted?.Invoke(skill);
            var displayName = skill.SkillTableEntrySo.SkillName;
            LogHandler.Log<EntitySkills>($"스킬 등록 완료: {entryId} ({displayName})");
        }

        /// <summary>
        /// 지정한 테이블 엔트리 ID의 스킬 등록을 해제합니다.
        /// </summary>
        public void UnregistSkill(int skillEntryId)
        {
            if (!_skillsByEntryId.Remove(skillEntryId, out var skill))
            {
                LogHandler.LogWarning<EntitySkills>($"스킬해제 실패: 등록되지 않은 엔트리 ID입니다: {skillEntryId}");
                return;
            }

            OnSkillUnregisted?.Invoke(skill);
            LogHandler.Log<EntitySkills>($"스킬 등록 해제 완료: {skillEntryId}");
        }

        public async UniTask<bool> ExecuteSkillAsync(int skillEntryId, EntityBase caster)
        {
            var skill = GetSkill(skillEntryId);
            if (skill == null)
            {
                LogHandler.LogWarning<EntitySkills>($"실행할 스킬이 없습니다. 엔트리 ID={skillEntryId}");
                return false;
            }

            return await skill.Execute(caster);
        }

        /// <summary>
        /// 등록된 모든 스킬에 대해 OnSkillUnregisted를 발생시키고 비웁니다.
        /// </summary>
        public void Clear()
        {
            var copy = _skillsByEntryId.Values.ToList();
            _skillsByEntryId.Clear();
            foreach (var skill in copy)
            {
                OnSkillUnregisted?.Invoke(skill);
            }
        }
    }
}
