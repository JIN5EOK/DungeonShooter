using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

namespace DungeonShooter
{
    /// <summary>
    /// 스테이지 씬 진입 시 플레이어 세션 초기화 및 스테이지 생성을 담당합니다.
    /// </summary>
    public class StageSceneInitializer : SceneInitializerBase
    {
        private StageContext _stageContext;
        private ITableRepository _tableRepository;
        private IStageGenerator _stageGenerator;
        private IStageInstantiator _stageInstantiator;
        private IPlayerSkillManager _playerSkillManager;
        private Inventory _inventory;
        private IItemFactory _itemFactory;
        private StageSceneUIManager _stageSceneUIManager;

        [Inject]
        public void Construct( StageContext stageContext
            , ITableRepository tableRepository
            , IStageGenerator stageGenerator
            , IStageInstantiator stageInstantiator
            , IPlayerSkillManager playerSkillManager
            , Inventory inventory
            , IItemFactory itemFactory
            , StageSceneUIManager stageSceneUIManager)
        {
            _stageContext = stageContext;
            _tableRepository = tableRepository;
            _stageGenerator = stageGenerator;
            _stageInstantiator = stageInstantiator;
            _playerSkillManager = playerSkillManager;
            _inventory = inventory;
            _itemFactory = itemFactory;
            _stageSceneUIManager = stageSceneUIManager;
        }

        private async UniTaskVoid Start()
        {
            await InitializePlayerData();
            await CreateStageAsync();
            IsSceneInitialized = true;
        }

        /// <summary>
        /// 스테이지 구조와 인스턴스를 생성한 뒤 StageManager에 전달합니다.
        /// </summary>
        private async UniTask CreateStageAsync()
        {
            var stage = await _stageGenerator.GenerateStage();
            var stageConfigEntry = _tableRepository.GetTableEntry<StageConfigTableEntry>(_stageContext.StageConfigTableId);
            if (stageConfigEntry == null)
            {
                LogHandler.LogError($"[{nameof(StageSceneInitializer)}] StageConfigTableEntry를 찾을 수 없습니다. ID: {_stageContext.StageConfigTableId}");
                return;
            }
            await _stageInstantiator.InstantiateStage(stageConfigEntry, stage);
        }

        private async UniTask InitializePlayerData()
        {
            var config = _tableRepository.GetTableEntry<PlayerConfigTableEntry>(_stageContext.PlayerConfigTableId);
            if (config == null)
            {
                LogHandler.LogError($"[{nameof(StageSceneInitializer)}] PlayerConfigTableEntry를 찾을 수 없습니다. ID: {_stageContext.PlayerConfigTableId}");
                return;
            }
            
            await _playerSkillManager.InitializeAsync(config);

            var weapon = await _itemFactory.CreateItemAsync(config.StartWeaponId);
            _inventory.AddItem(weapon);
            _inventory.EquipItem(weapon);
            
            await _stageSceneUIManager.InitializeAsync();
            _stageSceneUIManager.ShowHud();
        }

        
    }
}
