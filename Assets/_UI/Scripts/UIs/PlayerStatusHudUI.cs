using TMPro;
using UnityEngine;
using VContainer;

namespace DungeonShooter
{
    /// <summary>
    /// 플레이어 스탯(공격력, 방어력, 이동속도)과 남은 적 수를 수치로 표시하는 HUD.
    /// </summary>
    public class PlayerStatusHudUI : HudUI
    {
        [SerializeField] private TextMeshProUGUI _attackText;
        [SerializeField] private TextMeshProUGUI _defenseText;
        [SerializeField] private TextMeshProUGUI _moveSpeedText;
        [SerializeField] private TextMeshProUGUI _remainingEnemyCountText;

        private IPlayerDataService _playerDataService;
        private EntityManager _entityManager;

        [Inject]
        public void Construct(IPlayerDataService playerDataService, EntityManager entityManager)
        {
            _playerDataService = playerDataService;
            _entityManager = entityManager;

            if (_playerDataService.StatContainer != null)
            {
                _playerDataService.StatContainer.GetStat(StatType.Attack).OnValueChanged += OnAttackStatChanged;
                _playerDataService.StatContainer.GetStat(StatType.Defense).OnValueChanged += OnDefenseStatChanged;
                _playerDataService.StatContainer.GetStat(StatType.MoveSpeed).OnValueChanged += OnMoveSpeedStatChanged;
            }

            _entityManager.OnRemainingEnemyCountChanged += SetRemainingEnemyCount;

            RefreshAllStatTexts();
            SetRemainingEnemyCount(_entityManager.RemainingEnemyCount);
        }

        private void OnAttackStatChanged(int value)
        {
            if (_attackText != null) _attackText.text = value.ToString();
        }

        private void OnDefenseStatChanged(int value)
        {
            if (_defenseText != null) _defenseText.text = value.ToString();
        }

        private void OnMoveSpeedStatChanged(int value)
        {
            if (_moveSpeedText != null) _moveSpeedText.text = value.ToString();
        }

        private void RefreshAllStatTexts()
        {
            if (_playerDataService.StatContainer == null) return;
            if (_attackText != null) _attackText.text = _playerDataService.StatContainer.GetStat(StatType.Attack).GetValue().ToString();
            if (_defenseText != null) _defenseText.text = _playerDataService.StatContainer.GetStat(StatType.Defense).GetValue().ToString();
            if (_moveSpeedText != null) _moveSpeedText.text = _playerDataService.StatContainer.GetStat(StatType.MoveSpeed).GetValue().ToString();
        }

        private void SetRemainingEnemyCount(int count)
        {
            if (_remainingEnemyCountText != null)
                _remainingEnemyCountText.text = count.ToString();
        }

        protected override void OnDestroy()
        {
            if (_playerDataService?.StatContainer != null)
            {
                _playerDataService.StatContainer.GetStat(StatType.Attack).OnValueChanged -= OnAttackStatChanged;
                _playerDataService.StatContainer.GetStat(StatType.Defense).OnValueChanged -= OnDefenseStatChanged;
                _playerDataService.StatContainer.GetStat(StatType.MoveSpeed).OnValueChanged -= OnMoveSpeedStatChanged;
            }

            _entityManager.OnRemainingEnemyCountChanged -= SetRemainingEnemyCount;
        }
    }
}
