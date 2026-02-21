using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;
using Random = UnityEngine.Random;

namespace DungeonShooter
{
    /// <summary>
    /// 지정한 아이템을 지정한 위치에 드랍하고 적 사망시 아이템 드랍을 처리하는 서비스
    /// </summary>
    public interface IItemDropService
    {
        /// <summary>
        /// 아이템 ID와 위치로 필드 아이템을 생성합니다.
        /// </summary>
        UniTask<FieldItem> ItemDropAsync(int itemId, Vector3 position);
    }

    /// <summary>
    /// 적 사망 이벤트를 구독하여 가중치 기반으로 필드 아이템을 스폰합니다.
    /// </summary>
    public class ItemDropService : IItemDropService, IDisposable
    {
        /// <summary>
        /// 아이템 드랍 가중치, 10000이면 100%확률 드랍
        /// </summary>
        private const int WeightBase = 10000;

        private readonly IEventBus _eventBus;
        private readonly IFieldItemFactory _fieldItemFactory;

        [Inject]
        public ItemDropService(IEventBus eventBus, IFieldItemFactory fieldItemFactory)
        {
            _eventBus = eventBus;
            _fieldItemFactory = fieldItemFactory;
            _eventBus.Subscribe<EnemyDeadEvent>(OnEnemyDead);
        }

        private void OnEnemyDead(EnemyDeadEvent ev)
        {
            if (ev.enemyConfigTableEntry?.DropItemWeights == null || ev.enemyConfigTableEntry.DropItemWeights.Count == 0)
                return;

            var position = ev.enemy != null ? ev.enemy.transform.position : Vector3.zero;
            TryDropItemsByWeight(ev.enemyConfigTableEntry.DropItemWeights, position);
        }

        /// <inheritdoc />
        public UniTask<FieldItem> ItemDropAsync(int itemId, Vector3 position)
        {
            return _fieldItemFactory.CreateFieldItemAsync(itemId, position);
        }

        /// <summary>
        /// 가중치를 기준으로 각 아이템을 독립 확률로 판정하고, 성공 시 해당 위치에 드랍합니다. (예: 100 = 1%, 500 = 5%)
        /// </summary>
        private void TryDropItemsByWeight(Dictionary<int, int> weights, Vector3 position)
        {
            if (weights == null || weights.Count == 0)
                return;

            foreach (var kv in weights)
            {
                if (kv.Value <= 0)
                    continue;
                if (Random.Range(0, WeightBase) < kv.Value)
                    ItemDropAsync(kv.Key, position).Forget();
            }
        }

        public void Dispose()
        {
            _eventBus.Unsubscribe<EnemyDeadEvent>(OnEnemyDead);
        }
    }
}
