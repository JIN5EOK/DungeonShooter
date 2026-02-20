using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

namespace DungeonShooter
{
    /// <summary>
    /// 스테이지 씬 Hud 및 팝업 UI들을 관리하는 클래스
    /// </summary>
    public class StageSceneUIManager
    {
        private UIManager _uiManager;
        private HealthBarHudUI _healthBarHudUI;
        private ExpGaugeHudUI _expGaugeHudUI;
        private PlayerStatusHudUI _playerStatusHudUI;
        private SkillLevelUpUI _skillLevelUpUI;
        private SkillCooldownHudUI _skillCooldownHudUI;
        private InventoryUI _inventoryUI;
        [Inject]
        public StageSceneUIManager(UIManager uiManager)
        {
            _uiManager = uiManager;
        }

        public async UniTask InitializeAsync()
        {
            _healthBarHudUI = await _uiManager.GetSingletonUIAsync<HealthBarHudUI>(UIAddresses.UI_HpHud);
            _expGaugeHudUI = await _uiManager.GetSingletonUIAsync<ExpGaugeHudUI>(UIAddresses.UI_ExpHud);
            _playerStatusHudUI = await _uiManager.GetSingletonUIAsync<PlayerStatusHudUI>(UIAddresses.UI_PlayerStatusHud);
            _skillLevelUpUI = await _uiManager.GetSingletonUIAsync<SkillLevelUpUI>(UIAddresses.UI_SkillLevelUp);
            _skillCooldownHudUI = await _uiManager.GetSingletonUIAsync<SkillCooldownHudUI>(UIAddresses.UI_SkillCooldownHud);
            _inventoryUI = await _uiManager.GetSingletonUIAsync<InventoryUI>(UIAddresses.UI_Inventory);
            _inventoryUI.Hide();
            HideHud();
        }

        public void ShowHud()
        {
            _healthBarHudUI.Show();
            _expGaugeHudUI.Show();
            _playerStatusHudUI.Show();
            _skillCooldownHudUI.Show();
            _skillLevelUpUI.Hide();
        }

        public void HideHud()
        {
            _healthBarHudUI.Hide();
            _expGaugeHudUI.Hide();
            _playerStatusHudUI.Hide();
            _skillCooldownHudUI.Hide();
            _skillLevelUpUI.Hide();
        }
        
        public void ShowInventory() => _inventoryUI.Show();
        
        public void HideInventory() => _inventoryUI.Hide();
        public bool IsInventoryActivated() => _inventoryUI.gameObject.activeSelf;
    }
}
