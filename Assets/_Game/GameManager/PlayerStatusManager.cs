using System;
using VContainer;

namespace DungeonShooter
{
    /// <summary>
    /// 플레이어의 스탯, 현재 스테이터스를 담당하고 플레이어 게임오브젝트와 데이터를 연동합니다.
    /// </summary>
    public class PlayerStatusManager : IDisposable
    {
        public EntityStats StatContainer { get; private set; } = new();
        public event Action<int> OnHpChanged;

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
        
        private EntityBase _playerInstance;
        private IHealthComponent _boundHealthComponent;
        
        private ITableRepository _tableRepository;
        private IEventBus _eventBus;
        
        [Inject]
        public void Construct(ITableRepository tableRepository, IEventBus eventBus)
        {
            _tableRepository = tableRepository;
            _eventBus = eventBus;
            
            _eventBus.Subscribe<PlayerObjectSpawnEvent>(PlayerObjectSpawned);
            _eventBus.Subscribe<PlayerObjectDestroyEvent>(PlayerObjectDestroyed);
        }

                
        /// <summary>
        /// 선택한 플레이어 정보로 스탯 세션을 초기화합니다.
        /// </summary>
        public void Initialize(PlayerConfigTableEntry config)
        {
            if (config == null)
            {
                LogHandler.LogWarning<PlayerStatusManager>("PlayerConfigTableEntry가 null입니다.");
                return;
            }

            var statsEntry = _tableRepository.GetTableEntry<EntityStatsTableEntry>(config.StatsId);
            if (statsEntry == null)
            {
                LogHandler.LogWarning<PlayerStatusManager>($"EntityStatsTableEntry를 찾을 수 없습니다. ID: {config.StatsId}");
                return;
            }

            StatContainer.Initialize(statsEntry);
            Hp = StatContainer.GetStat(StatType.Hp).GetValue();
        }
        
        private void PlayerObjectSpawned(PlayerObjectSpawnEvent spawnEvent)
        {
            _playerInstance = spawnEvent.player;
            
            _boundHealthComponent = _playerInstance.GetComponent<HealthComponent>();
            _boundHealthComponent.SetCurrentHealth(Hp);
            _boundHealthComponent.OnHealthChanged += HealthComponentHpChanged;
        }

        private void PlayerObjectDestroyed(PlayerObjectDestroyEvent destroyEvent)
        {
            if(_boundHealthComponent != null)
                _boundHealthComponent.OnHealthChanged -= HealthComponentHpChanged;
        }
        
        private void HealthComponentHpChanged(int health)
        {
            Hp = health;
        }
        
        public void Dispose()
        {
            if(_boundHealthComponent != null)
                _boundHealthComponent.OnHealthChanged -= HealthComponentHpChanged;
            
            _eventBus.Unsubscribe<PlayerObjectSpawnEvent>(PlayerObjectSpawned);
        }
    }
}
