using System.Collections.Generic;
using UnityEngine;
using VContainer;

namespace DungeonShooter
{
    /// <summary>
    /// 레벨업 가능한 스킬 정보 (현재 스킬 인스턴스 + 다음 레벨 테이블 엔트리 + 표시용 아이콘)
    /// </summary>
    public readonly struct LevelUpableSkillInfo
    {
        public Skill CurrentSkill { get; }
        public SkillTableEntry NextLevelEntry { get; }
        public Sprite CurrentIcon { get; }
        public Sprite NextLevelIcon { get; }

        public LevelUpableSkillInfo(Skill currentSkill, SkillTableEntry nextLevelEntry, Sprite currentIcon, Sprite nextLevelIcon)
        {
            CurrentSkill = currentSkill;
            NextLevelEntry = nextLevelEntry;
            CurrentIcon = currentIcon;
            NextLevelIcon = nextLevelIcon;
        }
    }

    public interface ISkillService
    {
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
        private readonly ITableRepository _tableRepository;
        private readonly ISceneResourceProvider _sceneResourceProvider;
        private readonly IEventBus _eventBus;
        private readonly ISkillFactory _skillFactory;

        [Inject]
        public SkillService(ITableRepository tableRepository, ISceneResourceProvider sceneResourceProvider, IEventBus eventBus, ISkillFactory skillFactory)
        {
            _tableRepository = tableRepository;
            _sceneResourceProvider = sceneResourceProvider;
            _eventBus = eventBus;
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

            var nextSkillId = currentSkill.SkillTableEntry.CalculateNextLevelSkillId();
            var nextSkillEntry = _tableRepository.GetTableEntry<SkillTableEntry>(nextSkillId);
            if (nextSkillEntry == null)
                return false;

            var after = _skillFactory.CreateSkillSync(nextSkillEntry.Id);
            if (after == null)
            {
                LogHandler.LogError<SkillService>("다음 레벨 스킬 생성에 실패했습니다.");
                return false;
            }

            _eventBus.Publish(new SkillLevelUpEvent { beforeSkill = currentSkill, afterSkill = after });
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
                var nextSkillId = skill.SkillTableEntry.CalculateNextLevelSkillId();
                var nextSkillEntry = _tableRepository.GetTableEntry<SkillTableEntry>(nextSkillId);
                if (nextSkillEntry == null)
                    continue;

                var currentIcon = _sceneResourceProvider.GetAssetSync<Sprite>(skill.SkillTableEntry.SkillIconKey, SpriteAtlasAddresses.SkillIconAtlas);
                var nextIcon = _sceneResourceProvider.GetAssetSync<Sprite>(nextSkillEntry.SkillIconKey, SpriteAtlasAddresses.SkillIconAtlas);
                result.Add(new LevelUpableSkillInfo(skill, nextSkillEntry, currentIcon, nextIcon));
            }

            return result;
        }
    }
}
