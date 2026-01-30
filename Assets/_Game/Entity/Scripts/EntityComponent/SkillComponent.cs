using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

namespace DungeonShooter
{
    /// <summary>
    /// Entity의 스킬을 관리하는 컴포넌트
    /// </summary>
    public class SkillComponent : MonoBehaviour
    {
        private Dictionary<int, Skill> _skills = new Dictionary<int, Skill>();
        private ISkillFactory _skillFactory;
        private EntityBase _owner;

        [Inject]
        private void Construct(ISkillFactory skillFactory)
        {
            _skillFactory = skillFactory;
        }

        private void Awake()
        {
            _owner = GetComponent<EntityBase>();
            if (_owner == null)
            {
                LogHandler.LogError<SkillComponent>("EntityBase 컴포넌트를 찾을 수 없습니다.");
            }
        }

        /// <summary>
        /// 스킬을 사용합니다.
        /// </summary>
        /// <param name="skillEntryId">사용할 스킬의 Entry ID</param>
        /// <param name="target">스킬에 적중된 Entity (선택적)</param>
        /// <returns>사용 성공 여부</returns>
        public async UniTask<bool> UseSkill(int skillEntryId, EntityBase target = null)
        {
            if (!_skills.TryGetValue(skillEntryId, out var skill))
            {
                LogHandler.LogWarning<SkillComponent>($"스킬을 찾을 수 없습니다: {skillEntryId}");
                return false;
            }

            if (_owner == null)
            {
                LogHandler.LogError<SkillComponent>("Target이 null입니다.");
                return false;
            }

            var actualTarget = target ?? _owner;
            return await skill.Execute(actualTarget);
        }

        /// <summary>
        /// 스킬을 등록합니다.
        /// </summary>
        /// <param name="skillEntryId">스킬의 Entry ID</param>
        /// <returns>등록 성공 여부</returns>
        public async UniTask<Skill> GetOrRegistSkill(int skillEntryId)
        {
            if (_skills.TryGetValue(skillEntryId, out var registSkill))
            {
                return registSkill;
            }

            if (_skillFactory == null)
            {
                LogHandler.LogError<SkillComponent>("SkillFactory가 null입니다.");
                return null;
            }

            try
            {
                // SkillFactory를 통해 Skill 생성
                var skill = await _skillFactory.CreateSkillAsync(skillEntryId);
                if (skill == null)
                {
                    LogHandler.LogError<SkillComponent>($"Skill 생성 실패: {skillEntryId}");
                    return null;
                }

                _skills[skillEntryId] = skill;

                // 패시브 효과 자동 활성화
                if (skill.SkillData.IsPassiveSkill)
                {
                    skill.Activate(_owner);
                }

                LogHandler.Log<SkillComponent>($"스킬 등록 완료: {skillEntryId} ({skill.SkillTableEntry.SkillName})");
                return skill;
            }
            catch (Exception e)
            {
                LogHandler.LogException<SkillComponent>(e, "스킬 등록 중 오류 발생");
                return null;
            }
        }

        /// <summary>
        /// 스킬 등록을 해제합니다.
        /// </summary>
        /// <param name="skillEntryId">스킬의 Entry ID</param>
        public void UnregistSkill(int skillEntryId)
        {
            if (!_skills.TryGetValue(skillEntryId, out var skill))
            {
                LogHandler.LogWarning<SkillComponent>($"등록되지 않은 스킬입니다: {skillEntryId}");
                return;
            }

            // 패시브 효과 비활성화
            if (skill.SkillData.IsPassiveSkill)
            {
                skill.Deactivate(_owner);
            }

            // 리소스 정리
            skill.Dispose();

            _skills.Remove(skillEntryId);
            LogHandler.Log<SkillComponent>($"스킬 등록 해제 완료: {skillEntryId}");
        }

        private void OnDestroy()
        {
            // 모든 스킬 리소스 정리
            foreach (var skill in _skills.Values)
            {
                skill.Dispose();
            }

            _skills.Clear();
        }
    }
}
