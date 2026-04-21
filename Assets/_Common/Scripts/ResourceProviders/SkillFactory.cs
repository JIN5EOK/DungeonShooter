using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using VContainer;

namespace DungeonShooter
{
    public interface ISkillFactory
    {
        public UniTask<Skill> CreateSkillAsync(int skillEntryId, int skillLevelIndex = 0);
        public UniTask<Skill> CreateSkillAsync(SkillTableEntrySo entrySo, int skillLevelIndex = 0);
        public Skill CreateSkillSync(int skillEntryId, int skillLevelIndex = 0);
        public Skill CreateSkillSync(SkillTableEntrySo entrySo, int skillLevelIndex = 0);
    }
    
    /// <summary>
    /// Skill 인스턴스를 생성하는 팩토리
    /// </summary>
    public class SkillFactory : ISkillFactory
    {
        private readonly IResourceProvider _resourceProvider;
        private readonly ITableRepository _tableRepository;
        private readonly ISkillObjectFactory _skillObjectFactory;
        private readonly ISoundSfxService _soundSfxService;

        [Inject]
        public SkillFactory(IResourceProvider resourceProvider, ITableRepository tableRepository, ISkillObjectFactory skillObjectFactory, ISoundSfxService soundSfxService)
        {
            _resourceProvider = resourceProvider;
            _tableRepository = tableRepository;
            _skillObjectFactory = skillObjectFactory;
            _soundSfxService = soundSfxService;
        }

        /// <summary>
        /// 스킬 ID(공유 루트 Id)와 레벨 인덱스(0부터)로 Skill을 생성합니다.
        /// </summary>
        public async UniTask<Skill> CreateSkillAsync(int skillEntryId, int skillLevelIndex = 0)
        {
            try
            {
                var entrySo = _tableRepository.GetTableEntry<SkillTableEntrySo>(skillEntryId);
                return await CreateSkillAsyncInternal(entrySo, skillEntryId, skillLevelIndex);
            }
            catch (Exception e)
            {
                LogHandler.LogException<SkillFactory>(e, "스킬 데이터 로드에 실패했습니다.");
                return null;
            }
        }
        
        public async UniTask<Skill> CreateSkillAsync(AssetReferenceT<SkillTableEntrySo> skillEntryRef, int skillLevelIndex = 0)
        {
            try
            {
                var key = GetAddressOrNull(skillEntryRef);
                var entrySo = !string.IsNullOrEmpty(key)
                    ? await _resourceProvider.GetAssetAsync<SkillTableEntrySo>(key)
                    : null;
                return await CreateSkillAsyncInternal(entrySo, 0, skillLevelIndex);
            }
            catch (Exception e)
            {
                LogHandler.LogException<SkillFactory>(e, "스킬 데이터 로드에 실패했습니다.");
                return null;
            }
        }

        public Skill CreateSkillSync(int skillEntryId, int skillLevelIndex = 0)
        {
            try
            {
                var entrySo = _tableRepository.GetTableEntry<SkillTableEntrySo>(skillEntryId);
                return CreateSkillSyncInternal(entrySo, skillEntryId, skillLevelIndex);
            }
            catch (Exception e)
            {
                LogHandler.LogException<SkillFactory>(e, "스킬 데이터 로드에 실패했습니다.");
                return null;
            }
        }
        
        public Skill CreateSkillSync(AssetReferenceT<SkillTableEntrySo> skillEntryRef, int skillLevelIndex = 0)
        {
            try
            {
                var key = GetAddressOrNull(skillEntryRef);
                var entrySo = !string.IsNullOrEmpty(key)
                    ? _resourceProvider.GetAssetSync<SkillTableEntrySo>(key)
                    : null;
                return CreateSkillSyncInternal(entrySo, 0, skillLevelIndex);
            }
            catch (Exception e)
            {
                LogHandler.LogException<SkillFactory>(e, "스킬 데이터 로드에 실패했습니다.");
                return null;
            }
        }

        public Skill CreateSkillSync(SkillTableEntrySo entrySo, int skillLevelIndex = 0)
        {
            try
            {
                return CreateSkillSyncInternal(entrySo, 0, skillLevelIndex);
            }
            catch (Exception e)
            {
                LogHandler.LogException<SkillFactory>(e, "스킬 데이터 로드에 실패했습니다.");
                return null;
            }
        }

        public async UniTask<Skill> CreateSkillAsync(SkillTableEntrySo entrySo, int skillLevelIndex = 0)
        {
            try
            {
                return await CreateSkillAsyncInternal(entrySo, 0, skillLevelIndex);
            }
            catch (Exception e)
            {
                LogHandler.LogException<SkillFactory>(e, "스킬 데이터 로드에 실패했습니다.");
                return null;
            }
        }

        private async UniTask<Skill> CreateSkillAsyncInternal(SkillTableEntrySo entrySo, int skillEntryIdForLog, int skillLevelIndex)
        {
            if (entrySo?.SkillLevels == null || skillLevelIndex < 0 || skillLevelIndex >= entrySo.SkillLevels.Count)
            {
                LogHandler.LogError<SkillFactory>($"스킬 정의 또는 레벨 인덱스가 유효하지 않습니다. id={skillEntryIdForLog}, levelIndex={skillLevelIndex}");
                return null;
            }

            var levelData = entrySo.SkillLevels[skillLevelIndex];
            var skillDataKey = GetAddressOrNull(levelData.SkillDataRef);
            var skillData = !string.IsNullOrEmpty(skillDataKey)
                ? await _resourceProvider.GetAssetAsync<SkillData>(skillDataKey)
                : null;
            var iconKey = GetAddressOrNull(entrySo.SkillIconRef);
            var icon = !string.IsNullOrEmpty(iconKey)
                ? await _resourceProvider.GetAssetAsync<Sprite>(iconKey)
                : null;

            return new Skill(entrySo, skillLevelIndex, skillData, icon, _skillObjectFactory, _soundSfxService);
        }

        private Skill CreateSkillSyncInternal(SkillTableEntrySo entrySo, int skillEntryIdForLog, int skillLevelIndex)
        {
            if (entrySo?.SkillLevels == null || skillLevelIndex < 0 || skillLevelIndex >= entrySo.SkillLevels.Count)
            {
                LogHandler.LogError<SkillFactory>($"스킬 정의 또는 레벨 인덱스가 유효하지 않습니다. id={skillEntryIdForLog}, levelIndex={skillLevelIndex}");
                return null;
            }

            var levelData = entrySo.SkillLevels[skillLevelIndex];
            var skillDataKey = GetAddressOrNull(levelData.SkillDataRef);
            var skillData = !string.IsNullOrEmpty(skillDataKey)
                ? _resourceProvider.GetAssetSync<SkillData>(skillDataKey)
                : null;
            var iconKey = GetAddressOrNull(entrySo.SkillIconRef);
            var icon = !string.IsNullOrEmpty(iconKey)
                ? _resourceProvider.GetAssetSync<Sprite>(iconKey)
                : null;

            return new Skill(entrySo, skillLevelIndex, skillData, icon, _skillObjectFactory, _soundSfxService);
        }

        private static string GetAddressOrNull(AssetReference assetReference)
        {
            if (assetReference == null || !assetReference.RuntimeKeyIsValid())
                return null;
            var key = assetReference.RuntimeKey.ToString();
            return string.IsNullOrEmpty(key) ? null : key;
        }
    }
}
