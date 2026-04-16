using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;
using Random = UnityEngine.Random;

namespace DungeonShooter
{
    /// <summary>
    /// 레벨업 가능한 스킬 정보 (현재 스킬 인스턴스 + 다음 레벨 데이터 + 표시용 아이콘)
    /// </summary>
    public readonly struct LevelUpableSkillInfo
    {
        public Skill CurrentSkill { get; }
        public SkillLevelData NextLevelData { get; }
        public Sprite CurrentIcon { get; }
        public Sprite NextLevelIcon { get; }
        
        public LevelUpableSkillInfo(Skill currentSkill, SkillLevelData nextLevelData, Sprite currentIcon, Sprite nextLevelIcon)
        {
            CurrentSkill = currentSkill;
            NextLevelData = nextLevelData;
            CurrentIcon = currentIcon;
            NextLevelIcon = nextLevelIcon;
        }
    }

    public interface ISkillService
    {
        /// <summary>스킬 인스턴스가 레벨업으로 교체되었을 때 (이전, 이후)</summary>
        event Action<Skill, Skill> OnSkillLeveledUp;

        /// <summary>
        /// 보유 스킬 중 다음 레벨이 존재하는 스킬만 필터링하여 반환합니다.
        /// </summary>
        public IReadOnlyList<LevelUpableSkillInfo> GetLevelUpableSkills(IReadOnlyList<Skill> skills);

        /// <summary>
        /// 현재 스킬을 레벨업 적용합니다. 다음 레벨이 없으면 false를 반환합니다.
        /// </summary>
        /// <returns>적용 성공 여부</returns>
        public bool TrySkillLevelUp(IEntitySkills container, Skill currentSkill);
    }

    /// <summary>
    /// 스킬 관련 기획 비즈니스 로직(레벨업 가능 여부 판정 등)을 담당합니다.
    /// </summary>
    public class SkillService : ISkillService
    {
        public event Action<Skill, Skill> OnSkillLeveledUp;

        private readonly IResourceProvider _resourceProvider;
        private readonly ISkillFactory _skillFactory;

        [Inject]
        public SkillService(IResourceProvider resourceProvider, ISkillFactory skillFactory)
        {
            _resourceProvider = resourceProvider;
            _skillFactory = skillFactory;
        }

        /// <summary>
        /// 현재 스킬을 레벨업 시도합니다. 다음 레벨이 테이블에 없으면 false를 반환합니다.
        /// </summary>
        public bool TrySkillLevelUp(IEntitySkills container, Skill currentSkill)
        {
            if (container == null || currentSkill == null)
            {
                LogHandler.LogError<SkillService>("container 또는 currentSkill이 null입니다.");
                return false;
            }

            if (!container.Contains(currentSkill))
            {
                LogHandler.LogError<SkillService>("해당 스킬이 컨테이너에 존재하지 않습니다.");
                return false;
            }

            var so = currentSkill.SkillTableEntrySo;
            if (so?.SkillLevels == null)
                return false;

            var nextIndex = currentSkill.SkillLevelIndex + 1;
            if (nextIndex >= so.SkillLevels.Count)
                return false;

            var after = _skillFactory.CreateSkillSync(so.Id, nextIndex);
            if (after == null)
            {
                LogHandler.LogError<SkillService>("다음 레벨 스킬 생성에 실패했습니다.");
                return false;
            }

            OnSkillLeveledUp?.Invoke(currentSkill, after);
            after.StartCooldown(currentSkill.Cooldown);
            container.Unregist(currentSkill);
            container.Regist(after);
            return true;
        }

        public IReadOnlyList<LevelUpableSkillInfo> GetLevelUpableSkills(IReadOnlyList<Skill> skills)
        {
            var result = new List<LevelUpableSkillInfo>();
            foreach (var skill in skills)
            {
                var so = skill.SkillTableEntrySo;
                if (so?.SkillLevels == null)
                    continue;

                var nextIndex = skill.SkillLevelIndex + 1;
                if (nextIndex >= so.SkillLevels.Count)
                    continue;

                var nextLevelData = so.SkillLevels[nextIndex];
                var iconAddress = GetIconAddress(so);
                var currentIcon = string.IsNullOrEmpty(iconAddress)
                    ? null
                    : _resourceProvider.GetAssetSync<Sprite>(iconAddress);
                var nextIcon = currentIcon;
                result.Add(new LevelUpableSkillInfo(skill, nextLevelData, currentIcon, nextIcon));
            }

            // 스킬 목록 순서를 랜덤하게 섞어서 반환
            for (var i = result.Count - 1; i > 0; i--)
            {
                var j = Random.Range(0, i + 1);
                (result[i], result[j]) = (result[j], result[i]);
            }

            return result;
        }

        private static string GetIconAddress(SkillTableEntrySo so)
        {
            var iconRef = so.SkillIconRef;
            if (iconRef == null || !iconRef.RuntimeKeyIsValid())
                return null;
            return iconRef.RuntimeKey.ToString();
        }
    }
}
