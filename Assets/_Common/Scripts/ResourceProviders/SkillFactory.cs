using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

namespace DungeonShooter
{
    public interface ISkillFactory
    {
        public UniTask<Skill> CreateSkillAsync(int skillEntryId);
    }
    
    /// <summary>
    /// Skill 인스턴스를 생성하는 팩토리
    /// </summary>
    public class SkillFactory : ISkillFactory
    {
        private readonly ISceneResourceProvider _resourceProvider;
        private readonly ITableRepository _tableRepository;

        [Inject]
        public SkillFactory(ISceneResourceProvider resourceProvider, ITableRepository tableRepository)
        {
            _resourceProvider = resourceProvider;
            _tableRepository = tableRepository;
        }

        /// <summary>
        /// 스킬 ID를 기반으로 Skill을 생성합니다.
        /// </summary>
        /// <param name="skillEntryId">스킬 테이블 엔트리 ID</param>
        /// <returns>생성된 Skill 인스턴스 (실패 시 null)</returns>
        public async UniTask<Skill> CreateSkillAsync(int skillEntryId)
        {
            if (_tableRepository == null)
            {
                LogHandler.LogError<SkillFactory>("TableRepository가 null입니다.");
                return null;
            }
    
            if (_resourceProvider == null)
            {
                LogHandler.LogError<SkillFactory>("ResourceProvider가 null입니다.");
                return null;
            }

            try
            {
                // SkillTableEntry 조회
                var skillTableEntry = _tableRepository.GetTableEntry<SkillTableEntry>(skillEntryId);
                if (skillTableEntry == null)
                {
                    LogHandler.LogError<SkillFactory>($"SkillTableEntry를 찾을 수 없습니다: {skillEntryId}");
                    return null;
                }

                // SkillData 로드
                var skillData = await _resourceProvider.GetAssetAsync<SkillData>(skillTableEntry.SkillDataKey);
                if (skillData == null)
                {
                    LogHandler.LogError<SkillFactory>($"SkillData를 로드할 수 없습니다: {skillTableEntry.SkillDataKey}");
                    return null;
                }

                // 스킬 아이콘 로드
                Sprite icon = await _resourceProvider.GetAssetAsync<Sprite>(skillTableEntry.SkillIconKey, SpriteAtlasAddresses.SkillIconAtlas);

                // Skill 인스턴스 생성
                return new Skill(skillTableEntry, skillData, icon, _resourceProvider);
            }
            catch (Exception ex)
            {
                LogHandler.LogException<SkillFactory>(ex, $"Skill 생성 실패: {skillEntryId}");
                return null;
            }
        }
    }
}
