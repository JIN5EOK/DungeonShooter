using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

namespace DungeonShooter
{
    /// <summary>
    /// 스테이지 씬 Hud 및 팝업 UI들을 관리하는 클래스
    /// </summary>
    public class StageSceneUIController
    {
        private UIManager _uiManager;
        private HealthBarHudUI _healthBarHudUI;
        private ExpGaugeHudUI _expGaugeHudUI;
        private SkillLevelUpUI _skillLevelUpUI;
        private SkillCooldownHudUI _skillCooldownHudUI;
        private InventoryUI _inventoryUI;
        private PlayerSkillManager _playerSkillManager;
        
        [Inject]
        public StageSceneUIController(UIManager uiManager, PlayerSkillManager playerSkillManager)
        {
            _uiManager = uiManager;
            _playerSkillManager = playerSkillManager;
        }

        public async UniTask InitializeAsync()
        {
            _healthBarHudUI = await _uiManager.GetSingletonUIAsync<HealthBarHudUI>(UIAddresses.UI_HpHud);
            _expGaugeHudUI = await _uiManager.GetSingletonUIAsync<ExpGaugeHudUI>(UIAddresses.UI_ExpHud);
            _skillLevelUpUI = await _uiManager.GetSingletonUIAsync<SkillLevelUpUI>(UIAddresses.UI_SkillLevelUp);
            _skillCooldownHudUI = await _uiManager.GetSingletonUIAsync<SkillCooldownHudUI>(UIAddresses.UI_SkillCooldownHud);
            _inventoryUI = await _uiManager.GetSingletonUIAsync<InventoryUI>(UIAddresses.UI_Inventory);
            _inventoryUI.Hide();
            
            _skillCooldownHudUI.AddSkillCooldownSlot(_playerSkillManager.GetActiveSkill(0));
            _skillCooldownHudUI.AddSkillCooldownSlot(_playerSkillManager.GetActiveSkill(1));

            HideHud();
        }

        public void ShowHud()
        {
            _healthBarHudUI.Show();
            _expGaugeHudUI.Show();
            _skillCooldownHudUI.Show();
            _skillLevelUpUI.Hide();
        }

        public void HideHud()
        {
            _healthBarHudUI.Hide();
            _expGaugeHudUI.Hide();
            _skillCooldownHudUI.Hide();
            _skillLevelUpUI.Hide();
        }
        
        public void ShowInventory() => _inventoryUI.Show();
        
        public void HideInventory() => _inventoryUI.Hide();
        public bool IsInventoryActivated() => _inventoryUI.gameObject.activeSelf;
    }
}
