using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

namespace DungeonShooter
{
    /// <summary>
    /// 게임 매니저. 스테이지 시작 전 플레이어 세션 초기화 및 스테이지 생성을 담당합니다.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        private StageManager _stageManager;
        private StageContext _stageContext;
        private ITableRepository _tableRepository;
        private PlayerStatusManager _playerStatusManager;
        private PlayerSkillSession _playerSkillSession;
        private Inventory _inventory;
        private IItemFactory _itemFactory;
        private UIManager _uiManager;
        [Inject]
        public void Construct(StageManager stageManager
            , StageContext stageContext
            , ITableRepository tableRepository
            , PlayerStatusManager playerStatusManager
            , PlayerSkillSession playerSkillSession
            , Inventory inventory
            , IItemFactory itemFactory
            , UIManager uiManager)
        {
            _stageManager = stageManager;
            _stageContext = stageContext;
            _tableRepository = tableRepository;
            _playerStatusManager = playerStatusManager;
            _playerSkillSession = playerSkillSession;
            _inventory = inventory;
            _itemFactory = itemFactory;
            _uiManager = uiManager;
        }

        private async UniTaskVoid Start()
        {
            var config = _tableRepository.GetTableEntry<PlayerConfigTableEntry>(_stageContext.PlayerConfigTableId);
            if (config == null)
            {
                UnityEngine.Debug.LogError($"[{nameof(GameManager)}] PlayerConfigTableEntry를 찾을 수 없습니다. ID: {_stageContext.PlayerConfigTableId}");
                return;
            }

            _playerStatusManager.Initialize(config);
            await _playerSkillSession.InitializeAsync(config);

            _inventory.Clear();
            _inventory.SetStatGroup(_playerStatusManager.StatGroup);
            _inventory.SetSkillGroup(_playerSkillSession.SkillContainer);
            
            var weapon = await _itemFactory.CreateItemAsync(config.StartWeaponId);
            await _inventory.AddItem(weapon);
            await _inventory.EquipItem(weapon);
            
            await _uiManager.GetSingletonUIAsync<HealthBarHudUI>(UIAddresses.UI_HpHud);
            await _uiManager.GetSingletonUIAsync<ExpGaugeHudUI>(UIAddresses.UI_ExpHud);
            
            await _stageManager.CreateStageAsync();
        }
    }
}
