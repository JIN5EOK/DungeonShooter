using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

namespace DungeonShooter
{
    /// <summary>
    /// 게임 매니저. 스테이지 시작 전 플레이어 세션 초기화 및 스테이지 생성을 담당합니다.
    /// </summary>
    public class StageSceneInitializer : MonoBehaviour
    {
        private StageManager _stageManager;
        private StageContext _stageContext;
        private ITableRepository _tableRepository;
        private PlayerStatusManager _playerStatusManager;
        private IPlayerSkillManager _playerSkillManager;
        private Inventory _inventory;
        private IItemFactory _itemFactory;
        private StageSceneUIManager _stageSceneUIManager;
        [Inject]
        public void Construct(StageManager stageManager
            , StageContext stageContext
            , ITableRepository tableRepository
            , PlayerStatusManager playerStatusManager
            , IPlayerSkillManager playerSkillManager
            , Inventory inventory
            , IItemFactory itemFactory
            , StageSceneUIManager stageSceneUIManager)
        {
            _stageManager = stageManager;
            _stageContext = stageContext;
            _tableRepository = tableRepository;
            _playerStatusManager = playerStatusManager;
            _playerSkillManager = playerSkillManager;
            _inventory = inventory;
            _itemFactory = itemFactory;
            _stageSceneUIManager = stageSceneUIManager;
        }

        private async UniTaskVoid Start()
        {
            await InitializePlayerData();
            await _stageManager.CreateStageAsync();
        }

        private async UniTask InitializePlayerData()
        {
            var config = _tableRepository.GetTableEntry<PlayerConfigTableEntry>(_stageContext.PlayerConfigTableId);
            if (config == null)
            {
                LogHandler.LogError($"[{nameof(StageSceneInitializer)}] PlayerConfigTableEntry를 찾을 수 없습니다. ID: {_stageContext.PlayerConfigTableId}");
                return;
            }
            
            _playerStatusManager.Initialize(config);
            await _playerSkillManager.InitializeAsync(config);

            var weapon = await _itemFactory.CreateItemAsync(config.StartWeaponId);
            _inventory.AddItem(weapon);
            _inventory.EquipItem(weapon);
            
            await _stageSceneUIManager.InitializeAsync();
            _stageSceneUIManager.ShowHud();
        }
    }
}
