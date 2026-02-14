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
        private PlayerSkillManager _playerSkillManager;
        private Inventory _inventory;
        private IItemFactory _itemFactory;
        private UIManager _uiManager;
        [Inject]
        public void Construct(StageManager stageManager
            , StageContext stageContext
            , ITableRepository tableRepository
            , PlayerStatusManager playerStatusManager
            , PlayerSkillManager playerSkillManager
            , Inventory inventory
            , IItemFactory itemFactory
            , UIManager uiManager
            , PlayerLevelManager playerLevelManager)
        {
            _stageManager = stageManager;
            _stageContext = stageContext;
            _tableRepository = tableRepository;
            _playerStatusManager = playerStatusManager;
            _playerSkillManager = playerSkillManager;
            _inventory = inventory;
            _itemFactory = itemFactory;
            _uiManager = uiManager;
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
                LogHandler.LogError($"[{nameof(GameManager)}] PlayerConfigTableEntry를 찾을 수 없습니다. ID: {_stageContext.PlayerConfigTableId}");
                return;
            }

            _playerStatusManager.Initialize(config);
            await _playerSkillManager.InitializeAsync(config);

            _inventory.Clear();
            _inventory.SetStatGroup(_playerStatusManager.StatGroup);
            _inventory.SetSkillGroup(_playerSkillManager.SkillContainer);
            
            var weapon = await _itemFactory.CreateItemAsync(config.StartWeaponId);
            await _inventory.AddItem(weapon);
            await _inventory.EquipItem(weapon);
            
            var hpUI = await _uiManager.GetSingletonUIAsync<HealthBarHudUI>(UIAddresses.UI_HpHud);
            hpUI.Show();
            var expUI = await _uiManager.GetSingletonUIAsync<ExpGaugeHudUI>(UIAddresses.UI_ExpHud);
            expUI.Show();
            var skillLevelUpUI = await _uiManager.GetSingletonUIAsync<SkillLevelUpUI>(UIAddresses.UI_SkillLevelUp);
            skillLevelUpUI.Hide();
            var cooldownHudUI = await _uiManager.GetSingletonUIAsync<SkillCooldownHudUI>(UIAddresses.UI_SkillCooldownHud);
            cooldownHudUI.Show();
            cooldownHudUI.AddSkillCooldownSlot(_playerSkillManager.GetActiveSkill(0));
            cooldownHudUI.AddSkillCooldownSlot(_playerSkillManager.GetActiveSkill(1));
        }
    }
}
