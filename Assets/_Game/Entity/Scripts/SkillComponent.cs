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
        private IStageResourceProvider _resourceProvider;
        private ITableRepository _tableRepository;
        private EntityBase _owner;

        [Inject]
        private void Construct(IStageResourceProvider resourceProvider, ITableRepository tableRepository)
        {
            _resourceProvider = resourceProvider;
            _tableRepository = tableRepository;
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
        public async UniTask<bool> RegistSkill(int skillEntryId)
        {
            if (_skills.ContainsKey(skillEntryId))
            {
                LogHandler.LogWarning<SkillComponent>($"이미 등록된 스킬입니다: {skillEntryId}");
                return false;
            }

            if (_resourceProvider == null)
            {
                LogHandler.LogError<SkillComponent>("ResourceProvider가 null입니다.");
                return false;
            }

            if (_tableRepository == null)
            {
                LogHandler.LogError<SkillComponent>("TableRepository가 null입니다.");
                return false;
            }

            try
            {
                // SkillTableEntry 조회
                var skillTableEntry = _tableRepository.GetTableEntry<SkillTableEntry>(skillEntryId);
                if (skillTableEntry == null)
                {
                    LogHandler.LogError<SkillComponent>($"SkillTableEntry를 찾을 수 없습니다: {skillEntryId}");
                    return false;
                }

                // Skill 인스턴스 생성
                var skill = new Skill(skillTableEntry);
                
                // SkillData 로드 및 초기화
                await skill.InitializeAsync(_resourceProvider);
                
                if (!skill.IsInitialized)
                {
                    LogHandler.LogError<SkillComponent>($"Skill 초기화 실패: {skillEntryId}");
                    return false;
                }

                _skills[skillEntryId] = skill;

                // 패시브 효과 자동 활성화
                if (skill.SkillData.IsPassiveSkill)
                {
                    skill.Activate(_owner);
                }

                LogHandler.Log<SkillComponent>($"스킬 등록 완료: {skillEntryId} ({skillTableEntry.SkillName})");
                return true;
            }
            catch (Exception e)
            {
                LogHandler.LogError<SkillComponent>(e, "스킬 등록 중 오류 발생");
                return false;
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
