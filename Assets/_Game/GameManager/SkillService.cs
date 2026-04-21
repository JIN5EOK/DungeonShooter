using System.Collections.Generic;
using UnityEngine;
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
        /// <summary>
        /// 보유 스킬 중 다음 레벨이 존재하는 스킬만 필터링하여 반환합니다.
        /// </summary>
        IReadOnlyList<LevelUpableSkillInfo> GetLevelUpableSkills(IReadOnlyList<Skill> skills);
    }

    /// <summary>
    /// 스킬 관련 기획 비즈니스 로직(레벨업 가능 여부 판정 등)을 담당합니다.
    /// </summary>
    public class SkillService : ISkillService
    {
        public IReadOnlyList<LevelUpableSkillInfo> GetLevelUpableSkills(IReadOnlyList<Skill> skills)
        {
            var result = new List<LevelUpableSkillInfo>();
            foreach (var skill in skills)
            {
                if (!skill.CanLevelUp)
                    continue;

                var so = skill.SkillTableEntrySo;
                var nextLevelData = so.SkillLevels[skill.SkillLevelIndex + 1];
                result.Add(new LevelUpableSkillInfo(skill, nextLevelData, skill.Icon, skill.Icon));
            }

            for (var i = result.Count - 1; i > 0; i--)
            {
                var j = Random.Range(0, i + 1);
                (result[i], result[j]) = (result[j], result[i]);
            }

            return result;
        }
    }
}
