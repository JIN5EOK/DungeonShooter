using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;
using Random = UnityEngine.Random;

namespace DungeonShooter
{
    /// <summary>
    /// 지정한 아이템을 지정한 위치에 드랍하고, 가중치 테이블에 따른 랜덤 드랍을 처리하는 서비스
    /// </summary>
    public interface IItemDropService
    {
        /// <summary>
        /// 아이템 ID와 위치로 필드 아이템을 생성합니다.
        /// </summary>
        public UniTask<FieldItem> ItemDropAsync(int itemId, Vector3 position);

        /// <summary>
        /// 아이템 ID별 가중치로 독립 확률 판정 후, 성공 시 해당 월드 위치에 드랍합니다. (예: 100 = 1%, 10000 = 100%)
        /// </summary>
        public void TryDropItemsByWeight(IReadOnlyDictionary<int, int> itemIdWeights, Vector3 worldPosition);
    }

    /// <summary>
    /// 가중치 기반 필드 아이템 드랍을 담당합니다.
    /// </summary>
    public class ItemDropService : IItemDropService
    {
        /// <summary>
        /// 아이템 드랍 가중치, 10000이면 100%확률 드랍
        /// </summary>
        private const int WeightBase = 10000;

        private readonly IFieldItemFactory _fieldItemFactory;

        [Inject]
        public ItemDropService(IFieldItemFactory fieldItemFactory)
        {
            _fieldItemFactory = fieldItemFactory;
        }

        /// <inheritdoc />
        public UniTask<FieldItem> ItemDropAsync(int itemId, Vector3 position)
        {
            return _fieldItemFactory.CreateFieldItemAsync(itemId, position);
        }

        /// <summary>
        /// 가중치를 기준으로 각 아이템을 독립 확률로 판정하고, 성공 시 해당 위치에 드랍합니다. (예: 100 = 1%, 500 = 5%)
        /// </summary>
        public void TryDropItemsByWeight(IReadOnlyDictionary<int, int> weights, Vector3 position)
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
    }
}
