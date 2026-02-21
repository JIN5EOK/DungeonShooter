using Cysharp.Threading.Tasks;
using Jin5eok;
using UnityEngine;
using VContainer;

namespace DungeonShooter
{
    /// <summary>
    /// 필드 아이템 게임오브젝트를 생성하는 팩토리. 한 종류의 오브젝트만 풀링하고 아이템을 바꿔치기합니다.
    /// </summary>
    public interface IFieldItemFactory
    {
        /// <summary>
        /// ItemTableEntry ID로 필드 아이템을 생성합니다. 풀에 있으면 재사용하고, 없으면 새로 만듭니다.
        /// </summary>
        UniTask<FieldItem> CreateFieldItemAsync(int itemTableEntryId, Vector3 position = default, Quaternion rotation = default, Transform parent = null, bool instantiateInWorldSpace = true);
    }

    /// <summary>
    /// 필드 아이템을 풀링하여 생성합니다. 게임오브젝트는 한 종류만 사용하며, 생성 시 ItemTableEntry ID로 아이템을 설정합니다.
    /// </summary>
    public class FieldItemFactory : IFieldItemFactory
    {
        private const string PoolKey = "FieldItem";
        private const float ColliderRadius = 0.5f;

        private readonly ISceneResourceProvider _sceneResourceProvider;
        private readonly IItemFactory _itemFactory;
        private readonly Inventory _inventory;
        private readonly GameObjectPool _pool = new();

        [Inject]
        public FieldItemFactory(ISceneResourceProvider sceneResourceProvider, IItemFactory itemFactory, Inventory inventory)
        {
            _sceneResourceProvider = sceneResourceProvider;
            _itemFactory = itemFactory;
            _inventory = inventory;
        }

        /// <inheritdoc />
        public async UniTask<FieldItem> CreateFieldItemAsync(int itemTableEntryId, Vector3 position = default, Quaternion rotation = default, Transform parent = null, bool instantiateInWorldSpace = true)
        {
            var item = await _itemFactory.CreateItemAsync(itemTableEntryId);
            if (item == null)
                return null;

            var go = _pool.Get(PoolKey);
            if (go != null)
            {
                ApplyTransform(go.transform, position, rotation, parent, instantiateInWorldSpace);
                go.SetActive(true);
                var fieldItem = go.GetComponent<FieldItem>();
                if (fieldItem != null)
                {
                    fieldItem.Initialize(item, _inventory);
                    return fieldItem;
                }
            }

            go = await _sceneResourceProvider.GetInstanceAsync(CommonAddresses.FieldItem, position, rotation, parent, instantiateInWorldSpace);
            if (go == null)
                return null;

            var component = go.AddOrGetComponent<FieldItem>();
            EnsurePoolable(go);
            component.Initialize(item, _inventory);
            return component;
        }

        private void EnsurePoolable(GameObject go)
        {
            var poolable = go.AddOrGetComponent<PoolableComponent>();
            poolable.PoolKey = PoolKey;
            poolable.OnReleased -= OnFieldItemReleased;
            poolable.OnReleased += OnFieldItemReleased;
        }

        private void OnFieldItemReleased(PoolableComponent poolable)
        {
            if (poolable != null && !string.IsNullOrEmpty(poolable.PoolKey))
                _pool.Return(poolable.PoolKey, poolable.gameObject);
        }

        private static void ApplyTransform(Transform transform, Vector3 position, Quaternion rotation, Transform parent, bool instantiateInWorldSpace)
        {
            if (parent != null)
                transform.SetParent(parent, instantiateInWorldSpace);

            if (instantiateInWorldSpace)
                transform.SetPositionAndRotation(position, rotation);
            else
            {
                transform.localPosition = position;
                transform.localRotation = rotation;
            }
        }
    }
}
