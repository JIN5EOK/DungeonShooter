using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Jin5eok;
using UnityEngine;
using UnityEngine.AddressableAssets;
using VContainer;
using Random = UnityEngine.Random;

namespace DungeonShooter
{
    /// <summary>
    /// 적 캐릭터를 생성하는 팩토리 인터페이스
    /// </summary>
    public interface IEnemyFactory
    {
        UniTask<EntityBase> GetEnemyByConfigIdAsync(int configId, Vector3 position = default, Quaternion rotation = default, Transform parent = null, bool instantiateInWorldSpace = true);
        EntityBase GetEnemyByConfigIdSync(int configId, Vector3 position = default, Quaternion rotation = default, Transform parent = null, bool instantiateInWorldSpace = true);
    }
    
    /// <summary>
    /// 적 캐릭터를 생성하는 팩토리
    /// </summary>
    public class EnemyFactory : IEnemyFactory
    {
        private readonly ITableRepository _tableRepository;
        private readonly IResourceProvider _resourceProvider;
        private readonly ISkillFactory _skillFactory;
        private readonly ISkillObjectFactory _skillObjectFactory;
        private readonly GameObjectPool _pool = new();
        
        private List<int> _enemyIds;
        
        
        [Inject]
        public EnemyFactory(ITableRepository tableRepository, IResourceProvider resourceProvider, ISkillFactory skillFactory, ISkillObjectFactory skillObjectFactory)
        {
            _tableRepository = tableRepository;
            _resourceProvider = resourceProvider;
            _skillFactory = skillFactory;
            _skillObjectFactory = skillObjectFactory;
        }

        /// <summary>
        /// 지정한 EnemyConfigTableEntry ID로 적을 비동기 생성합니다.
        /// </summary>
        public async UniTask<EntityBase> GetEnemyByConfigIdAsync(int configId, Vector3 position = default, Quaternion rotation = default, Transform parent = null, bool instantiateInWorldSpace = true)
        {
            var enemyConfig = _tableRepository.GetTableEntry<EnemyConfigSo>(configId);
            
            if (enemyConfig == null)
                return null;
            
            var entity = GetFromPool(enemyConfig, position, rotation, parent, instantiateInWorldSpace);
            
            if (entity == null)
                entity = await CreateAsync(enemyConfig, position, rotation, parent, instantiateInWorldSpace);

            return entity;
        }

        /// <summary>
        /// 지정한 EnemyConfigTableEntry ID로 적을 동기 생성합니다.
        /// </summary>
        public EntityBase GetEnemyByConfigIdSync(int configId,  Vector3 position = default, Quaternion rotation = default, Transform parent = null,  bool instantiateInWorldSpace = true)
        {
            var enemyConfig = _tableRepository.GetTableEntry<EnemyConfigSo>(configId);
            
            if (enemyConfig == null)
                return null;
            
            var entity = GetFromPool(enemyConfig, position, rotation, parent, instantiateInWorldSpace);
            
            if (entity == null)
                entity = CreateSync(enemyConfig, position, rotation, parent, instantiateInWorldSpace);

            return entity;
        }

        private EntityBase GetFromPool(EnemyConfigSo entry, Vector3 position = default, Quaternion rotation = default, Transform parent = null,  bool instantiateInWorldSpace = true)
        {
            var address = GetAddressOrNull(entry.GameObjectRef);
            var poolKey = GetPoolKey(address);
            var go = _pool.Get(poolKey);
            
            if (go != null)
            {
                ApplyTransform(go.transform, position, rotation, parent, instantiateInWorldSpace);
                go.SetActive(true);
                return InitializeEnemyInstance(go, entry);
            }
            else
            {
                return null;
            }
        }

        private EntityBase CreateSync(EnemyConfigSo entry, Vector3 position = default, Quaternion rotation = default, Transform parent = null,  bool instantiateInWorldSpace = true)
        {
            var address = GetAddressOrNull(entry.GameObjectRef);
            if (string.IsNullOrEmpty(address))
                return null;

            var go = _resourceProvider.GetInstanceSync(address, position, rotation, parent, instantiateInWorldSpace);
            var poolKey = GetPoolKey(address);
            EnsurePoolable(go, poolKey);
            return InitializeEnemyInstance(go, entry);
        }
        
        private async UniTask<EntityBase> CreateAsync(EnemyConfigSo entry, Vector3 position = default, Quaternion rotation = default, Transform parent = null,  bool instantiateInWorldSpace = true)
        {
            var address = GetAddressOrNull(entry.GameObjectRef);
            if (string.IsNullOrEmpty(address))
                return null;

            var go = await _resourceProvider.GetInstanceAsync(address, position, rotation, parent, instantiateInWorldSpace);
            var poolKey = GetPoolKey(address);
            EnsurePoolable(go, poolKey);
            return InitializeEnemyInstance(go, entry);
        }
        
        /// <summary>
        /// EnemyKeys에서 랜덤 ID를 선택하고, 해당 EnemyConfigTableEntry의 GameObjectKey(어드레스)를 반환합니다.
        /// </summary>
        private EnemyConfigSo GetRandomEnemyTableConfig()
        {
            if (_enemyIds == null || _enemyIds.Count == 0)
            {
                Debug.LogWarning($"[{nameof(EnemyFactory)}] 적 ID 목록이 비어있습니다.");
                return null;
            }

            var enemyId = _enemyIds[Random.Range(0, _enemyIds.Count)];
            var enemyEntry = _tableRepository.GetTableEntry<EnemyConfigSo>(enemyId);
            if (enemyEntry == null)
            {
                Debug.LogWarning($"[{nameof(EnemyFactory)}] EnemyConfigSo를 찾을 수 없습니다. ID: {enemyId}");
                return null;
            }

            return enemyEntry;
        }

        private static string GetPoolKey(string key)
        {
            return $"{nameof(EntityBase)}:{key}";
        }

        private void EnsurePoolable(GameObject go, string poolKey)
        {
            var poolable = go.AddOrGetComponent<PoolableComponent>();
            poolable.PoolKey = poolKey;
            poolable.OnReleased -= OnEnemyReleased;
            poolable.OnReleased += OnEnemyReleased;
        }
        
        private void OnEnemyReleased(PoolableComponent poolable)
        {
            if (poolable != null && !string.IsNullOrEmpty(poolable.PoolKey))
            {
                _pool.Return(poolable.PoolKey, poolable.gameObject);
            }
        }
        
        private static void ApplyTransform(Transform transform, Vector3 position, Quaternion rotation, Transform parent, bool instantiateInWorldSpace)
        {
            if (parent != null)
            {
                transform.SetParent(parent, instantiateInWorldSpace);
            }

            if (instantiateInWorldSpace)
            {
                transform.SetPositionAndRotation(position, rotation);
            }
            else
            {
                transform.localPosition = position;
                transform.localRotation = rotation;
            }
        }

        private EntityBase InitializeEnemyInstance(GameObject enemyInstance, EnemyConfigSo configTableEntry)
        {
            if (enemyInstance == null)
            {
                var address = configTableEntry != null ? GetAddressOrNull(configTableEntry.GameObjectRef) : string.Empty;
                Debug.LogWarning($"[{nameof(EnemyFactory)}] 적 인스턴스 생성 실패: {address}");
                return null;
            }

            enemyInstance.tag = GameTags.Enemy;
            enemyInstance.layer = PhysicalLayers.Enemy.LayerIndex;

            var enemy = enemyInstance.AddOrGetComponent<Enemy>();

            var entityStats = new EntityStats();
            entityStats.Initialize(configTableEntry.Stats);
            var entitySkills = new EntitySkills();
            var context = new EntityContext(
                new EntityInputContext(),
                entityStats,
                new EntityHealth(entityStats.GetStat(StatType.Hp)),
                entitySkills);
            enemy.SetContext(context);

            var activeSkills = new List<Skill>();
            foreach (var skillRef in configTableEntry.ActiveSkills)
            {
                var skill = _skillFactory.CreateSkillSync(skillRef);
                entitySkills.RegistSkill(skill);
                activeSkills.Add(skill);
            }

            var aiBT = _resourceProvider.GetAssetSync<AiBTBase>(GetAddressOrNull(configTableEntry.AIType));
            enemy.SetAI(new EnemyBTAI(enemy, aiBT));

            enemy.GetContext().HealthModel.OnDeath += () =>
            {
                var pos = enemy.transform.position;
                _skillObjectFactory.CreateSkillObjectAsync<ParticleSkillObject>(CommonAddresses.MonsterDeath_Particle, pos).Forget();
            };

            return enemy;
        }

        private static string GetAddressOrNull(AssetReference assetReference)
        {
            if (assetReference == null || !assetReference.RuntimeKeyIsValid())
                return null;
            var key = assetReference.RuntimeKey.ToString();
            return string.IsNullOrEmpty(key) ? null : key;
        }
    }
}
