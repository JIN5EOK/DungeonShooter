using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

namespace DungeonShooter
{
    public interface ISkillFactory
    {
        public UniTask<Skill> CreateSkillAsync(int skillEntryId);
        public Skill CreateSkillSync(int skillEntryId);
    }
    
    /// <summary>
    /// Skill 인스턴스를 생성하는 팩토리
    /// </summary>
    public class SkillFactory : ISkillFactory
    {
        private readonly ISceneResourceProvider _resourceProvider;
        private readonly ITableRepository _tableRepository;
        private readonly ISkillObjectFactory _skillObjectFactory;
        private readonly ISoundSfxService _soundSfxService;

        [Inject]
        public SkillFactory(ISceneResourceProvider resourceProvider, ITableRepository tableRepository, ISkillObjectFactory skillObjectFactory, ISoundSfxService soundSfxService)
        {
            _resourceProvider = resourceProvider;
            _tableRepository = tableRepository;
            _skillObjectFactory = skillObjectFactory;
            _soundSfxService = soundSfxService;
        }

        /// <summary>
        /// 스킬 ID를 기반으로 Skill을 생성합니다.
        /// </summary>
        public async UniTask<Skill> CreateSkillAsync(int skillEntryId)
        {
            try
            {
                var skillTableEntry = _tableRepository.GetTableEntry<SkillTableEntry>(skillEntryId);
                var skillData = !String.IsNullOrEmpty(skillTableEntry.SkillDataKey) ? 
                    _resourceProvider.GetAssetSync<SkillData>(skillTableEntry.SkillDataKey) : null;
                Sprite icon = await _resourceProvider.GetAssetAsync<Sprite>(skillTableEntry.SkillIconKey, SpriteAtlasAddresses.SkillIconAtlas);

                return new Skill(skillTableEntry, skillData, icon, _resourceProvider, _skillObjectFactory, _soundSfxService, _tableRepository);
            }
            catch (Exception e)
            {
                LogHandler.LogException<SkillFactory>(e, "스킬 데이터 로드에 실패했습니다.");
                throw;
            }
        }

        public Skill CreateSkillSync(int skillEntryId)
        {
            try
            {
                var skillTableEntry = _tableRepository.GetTableEntry<SkillTableEntry>(skillEntryId);
                
                var skillData = !String.IsNullOrEmpty(skillTableEntry.SkillDataKey) ? 
                    _resourceProvider.GetAssetSync<SkillData>(skillTableEntry.SkillDataKey) : null;
                Sprite icon = _resourceProvider.GetAssetSync<Sprite>(skillTableEntry.SkillIconKey, SpriteAtlasAddresses.SkillIconAtlas);

                return new Skill(skillTableEntry, skillData, icon, _resourceProvider, _skillObjectFactory, _soundSfxService, _tableRepository);
            }
            catch (Exception e)
            {
                LogHandler.LogException<SkillFactory>(e, "스킬 데이터 로드에 실패했습니다.");
                throw;
            }
        }
    }
}
