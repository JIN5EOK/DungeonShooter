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

            var stat = _playerDataService.EntityContext?.Stat;
            if (stat != null)
            {
                stat.GetStat(StatType.Attack).OnValueChanged += OnAttackStatChanged;
                stat.GetStat(StatType.Defense).OnValueChanged += OnDefenseStatChanged;
                stat.GetStat(StatType.MoveSpeed).OnValueChanged += OnMoveSpeedStatChanged;
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
            var stat = _playerDataService.EntityContext?.Stat;
            if (stat == null) return;
            if (_attackText != null) _attackText.text = stat.GetStat(StatType.Attack).GetValue().ToString();
            if (_defenseText != null) _defenseText.text = stat.GetStat(StatType.Defense).GetValue().ToString();
            if (_moveSpeedText != null) _moveSpeedText.text = stat.GetStat(StatType.MoveSpeed).GetValue().ToString();
        }

        private void SetRemainingEnemyCount(int count)
        {
            if (_remainingEnemyCountText != null)
                _remainingEnemyCountText.text = count.ToString();
        }

        protected override void OnDestroy()
        {
            var stat = _playerDataService?.EntityContext?.Stat;
            if (stat != null)
            {
                stat.GetStat(StatType.Attack).OnValueChanged -= OnAttackStatChanged;
                stat.GetStat(StatType.Defense).OnValueChanged -= OnDefenseStatChanged;
                stat.GetStat(StatType.MoveSpeed).OnValueChanged -= OnMoveSpeedStatChanged;
            }

            _entityManager.OnRemainingEnemyCountChanged -= SetRemainingEnemyCount;
        }
    }
}
