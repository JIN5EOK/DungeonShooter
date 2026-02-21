using System;
using VContainer;

namespace DungeonShooter
{
    /// <summary>
    /// 적 사망 시 아이템 드랍을 담당하는 서비스 인터페이스
    /// </summary>
    public interface IItemDropService
    {
        /// <summary>
        /// 적 사망 이벤트에 따라 드랍 처리합니다. (실제 드랍 로직은 구현체에서 수행)
        /// </summary>
        void ExecuteDrop(EnemyDeadEvent ev);
    }

    /// <summary>
    /// 적 사망 이벤트를 구독하여 아이템 드랍을 실행합니다.
    /// 실제 아이템 스폰 등 드랍 기능은 추후 구현 예정입니다.
    /// </summary>
    public class ItemDropService : IItemDropService, IDisposable
    {
        private readonly IEventBus _eventBus;

        [Inject]
        public ItemDropService(IEventBus eventBus)
        {
            _eventBus = eventBus;
            _eventBus.Subscribe<EnemyDeadEvent>(OnEnemyDead);
        }

        private void OnEnemyDead(EnemyDeadEvent ev)
        {
            ExecuteDrop(ev);
        }

        /// <inheritdoc />
        public void ExecuteDrop(EnemyDeadEvent ev)
        {
            if (ev.enemyConfigTableEntry?.DropItemWeights == null || ev.enemyConfigTableEntry.DropItemWeights.Count == 0)
                return;

            // TODO: 가중치 기반 드랍 판정 후 실제 아이템 스폰
            // var weights = ev.enemyConfigTableEntry.DropItemWeights;
            // var position = ev.enemy.transform.position;
        }

        public void Dispose()
        {
            _eventBus.Unsubscribe<EnemyDeadEvent>(OnEnemyDead);
        }
    }
}
