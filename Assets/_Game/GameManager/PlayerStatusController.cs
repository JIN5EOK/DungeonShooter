using System;
using Jin5eok;
using UnityEngine;
using VContainer;

namespace DungeonShooter
{
    /// <summary>
    /// 플레이어 스탯·경험치·레벨을 담당합니다.
    /// StatGroup을 소유하며, 엔티티 바인딩 시 데이터만 연동합니다.
    /// </summary>
    public class PlayerStatusController
    {
        public const int ExpPerLevel = 100;

        public EntityStatGroup StatGroup { get; private set; }
        public event Action<int> OnLevelChanged;
        public event Action<int> OnExpChanged;
        public int Level => _level;
        public int Exp => _exp;
        public int Hp => _hp;

        private int _level = 1;
        private int _exp;
        private int _hp;

        private ITableRepository _tableRepository;
        private Inventory _inventory;
        private EntityBase _playerInstance;
        private HealthComponent _boundHealthComponent;

        [Inject]
        private void Construct(ITableRepository tableRepository, Inventory inventory)
        {
            _tableRepository = tableRepository;
            _inventory = inventory;
        }

        /// <summary>
        /// 선택한 플레이어 정보로 스탯 세션을 초기화합니다.
        /// </summary>
        public void Initialize(PlayerConfigTableEntry config)
        {
            if (config == null)
            {
                LogHandler.LogWarning<PlayerStatusController>("PlayerConfigTableEntry가 null입니다.");
                return;
            }

            var statsEntry = _tableRepository.GetTableEntry<EntityStatsTableEntry>(config.StatsId);
            if (statsEntry == null)
            {
                LogHandler.LogWarning<PlayerStatusController>($"EntityStatsTableEntry를 찾을 수 없습니다. ID: {config.StatsId}");
                return;
            }

            StatGroup = new EntityStatGroup();
            StatGroup.Initialize(statsEntry);
            _level = 1;
            _exp = 0;
            _hp = StatGroup.GetStat(StatType.Hp);
        }

        /// <summary>
        /// 경험치를 추가합니다.
        /// </summary>
        public void AddExp(int amount)
        {
            if (amount <= 0) return;
            var levelBefore = _level;
            _exp += amount;
            while (_exp >= ExpPerLevel)
            {
                _exp -= ExpPerLevel;
                _level++;
            }
            OnExpChanged?.Invoke(_exp);
            if (_level != levelBefore)
                OnLevelChanged?.Invoke(_level);
        }

        /// <summary>
        /// 플레이어 게임오브젝트와 데이터를 바인딩 합니다.
        /// </summary>
        public void BindPlayerInstance(EntityBase entity)
        {
            if (entity == null) return;

            _inventory.SetStatGroup(StatGroup);
            entity.SetStatGroup(StatGroup);
            _playerInstance = entity;
            entity.OnDestroyed += UnbindPlayerInstance;
        }

        /// <summary>
        /// HealthComponent와 체력을 동기화하도록 합니다.
        /// </summary>
        public void SyncFromHealthComponent(HealthComponent healthComponent)
        {
            if (healthComponent == null) return;

            UnsyncFromHealthComponent();

            _boundHealthComponent = healthComponent;
            healthComponent.SetCurrentHealth(_hp);
            _boundHealthComponent.OnHealthChanged += OnHealthChangedFromComponent;
        }

        /// <summary>
        /// HealthComponent와의 동기화를 해제합니다.
        /// </summary>
        public void UnsyncFromHealthComponent()
        {
            if (_boundHealthComponent != null)
            {
                _boundHealthComponent.OnHealthChanged -= OnHealthChangedFromComponent;
                _boundHealthComponent = null;
            }
        }

        private void OnHealthChangedFromComponent(int current, int max)
        {
            _hp = current;
        }

        /// <summary>
        /// 플레이어 엔티티 바인딩을 해제합니다.
        /// </summary>
        public void UnbindPlayerInstance(EntityBase entity)
        {
            if (_playerInstance != entity) return;
            UnsyncFromHealthComponent();
            _playerInstance = null;
        }
    }
}
