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
    public class PlayerStatusSession
    {
        public const int ExpPerLevel = 100;

        public EntityStatGroup StatGroup { get; private set; }
        public event Action<int> OnHpChanged;
        public event Action<int> OnLevelChanged;
        public event Action<int> OnExpChanged;
        public int Level { get; private set; } = 1;
        public int Exp {get; private set; }

        public int Hp
        {
            get => _hp;
            private set
            {
                _hp = value;
                OnHpChanged?.Invoke(value);
            }
        }
        private int _hp;
        

        private ITableRepository _tableRepository;
        private EntityBase _playerInstance;
        private HealthComponent _boundHealthComponent;

        [Inject]
        private void Construct(ITableRepository tableRepository)
        {
            _tableRepository = tableRepository;
        }

        /// <summary>
        /// 선택한 플레이어 정보로 스탯 세션을 초기화합니다.
        /// </summary>
        public void Initialize(PlayerConfigTableEntry config)
        {
            if (config == null)
            {
                LogHandler.LogWarning<PlayerStatusSession>("PlayerConfigTableEntry가 null입니다.");
                return;
            }

            var statsEntry = _tableRepository.GetTableEntry<EntityStatsTableEntry>(config.StatsId);
            if (statsEntry == null)
            {
                LogHandler.LogWarning<PlayerStatusSession>($"EntityStatsTableEntry를 찾을 수 없습니다. ID: {config.StatsId}");
                return;
            }

            StatGroup = new EntityStatGroup();
            StatGroup.Initialize(statsEntry);
            Level = 1;
            Exp = 0;
            _hp = StatGroup.GetStat(StatType.Hp);
        }

        /// <summary>
        /// 경험치를 추가합니다.
        /// </summary>
        public void AddExp(int amount)
        {
            if (amount <= 0) return;
            var levelBefore = Level;
            Exp += amount;
            while (Exp >= ExpPerLevel)
            {
                Exp -= ExpPerLevel;
                Level++;
            }
            OnExpChanged?.Invoke(Exp);
            
            if (Level != levelBefore)
                OnLevelChanged?.Invoke(Level);
        }

        /// <summary>
        /// 플레이어 게임오브젝트와 데이터를 바인딩 합니다.
        /// </summary>
        public void BindPlayerInstance(EntityBase entity)
        {
            if (entity == null) return;

            entity.SetStatGroup(StatGroup);
            _playerInstance = entity;
            entity.OnDestroyed += UnbindPlayerInstance;

            _boundHealthComponent = _playerInstance.gameObject.AddOrGetComponent<HealthComponent>();
            _boundHealthComponent.SetCurrentHealth(_hp);
            _boundHealthComponent.OnHealthChanged += OnHealthChangedFromComponent;
        }
        
        /// <summary>
        /// 플레이어 엔티티 바인딩을 해제합니다.
        /// </summary>
        public void UnbindPlayerInstance(EntityBase player)
        {
            _boundHealthComponent.OnHealthChanged -= OnHealthChangedFromComponent;
            _boundHealthComponent = null;
            _playerInstance = null;
        }

        private void OnHealthChangedFromComponent(int current, int max)
        {
            Hp = current;
        }
    }
}
