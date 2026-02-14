using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

namespace DungeonShooter
{
/// <summary>
/// 플레이어 엔티티 게임오브젝트 인스턴스에 대한 UI/입력/이벤트 연동 담당
/// </summary>
    public class PlayerInstanceManager
    {
        private readonly PlayerSkillManager _playerSkillManager;
        private readonly Inventory _inventory;
        private readonly UIManager _uIManager;
        private readonly PlayerInputSession _playerInputSession;

        private EntityBase _currentPlayerEntity;
        private SkillCooldownHudUI _skillCooldownHudUI;
        private SkillCooldownSlot _weaponCooldownSlot;
        private SkillLevelUpUI _skillLevelUpUI;

        private Skill _boundWeaponActiveSkill;

        [Inject]
        public PlayerInstanceManager(PlayerStatusManager playerStatusManager
            , PlayerSkillManager playerSkillManager
            , Inventory inventory
            , UIManager uIManager
            , PlayerInputSession playerInputSession)
        {
            _playerSkillManager = playerSkillManager;
            _inventory = inventory;
            _uIManager = uIManager;
            _playerInputSession = playerInputSession;
        }

        public async UniTask BindAsync(EntityBase entity)
        {
            if (entity == null)
                return;

            if (_currentPlayerEntity != null)
                UnbindAndDestroy();

            _currentPlayerEntity = entity;
            
            _inventory.BindPlayerInstance(entity);

            _playerInputSession.BindPlayerInstance(entity);
        }

        public void UnbindAndDestroy()
        {
            if (_currentPlayerEntity == null)
                return;

            CleanupPlayerUI();
            _playerInputSession.UnbindPlayerInstance();

            Object.Destroy(_currentPlayerEntity.gameObject);
            _currentPlayerEntity = null;
        }

        private void CleanupPlayerUI()
        {
            _skillLevelUpUI = null;
        }
        
        private async UniTaskVoid ShowSkillLevelUpUIAsync()
        {
            if (_skillLevelUpUI == null)
                _skillLevelUpUI = await _uIManager.GetSingletonUIAsync<SkillLevelUpUI>(UIAddresses.UI_SkillLevelUp);

            await _skillLevelUpUI.ShowSkillLevelUp(
                _playerSkillManager.SkillContainer,
                _playerSkillManager.ReplaceSkillAsync);
        }
    }
}