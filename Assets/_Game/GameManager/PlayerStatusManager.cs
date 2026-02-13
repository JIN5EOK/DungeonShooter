using System;
using Jin5eok;
using UnityEngine;
using VContainer;

namespace DungeonShooter
{
    /// <summary>
    /// 플레이어의 스탯, 현재 스테이터스를 담당합니다.
    /// </summary>
    public class PlayerStatusManager
    {
        public EntityStatGroup StatGroup { get; private set; }
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
                LogHandler.LogWarning<PlayerStatusManager>("PlayerConfigTableEntry가 null입니다.");
                return;
            }

            var statsEntry = _tableRepository.GetTableEntry<EntityStatsTableEntry>(config.StatsId);
            if (statsEntry == null)
            {
                LogHandler.LogWarning<PlayerStatusManager>($"EntityStatsTableEntry를 찾을 수 없습니다. ID: {config.StatsId}");
                return;
            }

            StatGroup = new EntityStatGroup();
            StatGroup.Initialize(statsEntry);
            _hp = StatGroup.GetStat(StatType.Hp).GetValue();
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
        }
        
        /// <summary>
        /// 플레이어 엔티티 바인딩을 해제합니다.
        /// </summary>
        public void UnbindPlayerInstance(EntityBase player)
        {
            _playerInstance = null;
        }
    }
}
