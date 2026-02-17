using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Jin5eok;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace DungeonShooter
{
    /// <summary>
    /// 적 캐릭터를 생성하는 팩토리 인터페이스
    /// </summary>
    public interface IEnemyFactory
    {
        UniTask<EntityBase> GetRandomEnemyAsync(Vector3 position = default, Quaternion rotation = default, Transform parent = null, bool instantiateInWorldSpace = true);
        EntityBase GetRandomEnemySync(Vector3 position = default, Quaternion rotation = default, Transform parent = null, bool instantiateInWorldSpace = true);
        UniTask<EntityBase> GetEnemyByConfigIdAsync(int configId, Vector3 position = default, Quaternion rotation = default, Transform parent = null, bool instantiateInWorldSpace = true);
        EntityBase GetEnemyByConfigIdSync(int configId, Vector3 position = default, Quaternion rotation = default, Transform parent = null, bool instantiateInWorldSpace = true);
    }
    
    /// <summary>
    /// 적 캐릭터를 생성하는 팩토리
    /// </summary>
    public class EnemyFactory : IEnemyFactory
    {
        private LifetimeScope _sceneLifetimeScope;
        
        private readonly ITableRepository _tableRepository;
        private readonly StageContext _stageContext;
        private readonly ISceneResourceProvider _sceneResourceProvider;
        private readonly IEventBus _eventBus;
        private readonly ISkillFactory _skillFactory;
        private readonly ISkillObjectFactory _skillObjectFactory;
        private readonly GameObjectPool _pool = new();
        
        private List<int> _enemyIds;
        
        
        [Inject]
        public EnemyFactory(ITableRepository tableRepository, StageContext stageContext, ISceneResourceProvider sceneResourceProvider, IEventBus eventBus, LifetimeScope sceneLifeTimeScope, ISkillFactory skillFactory, ISkillObjectFactory skillObjectFactory)
        {
            _tableRepository = tableRepository;
            _stageContext = stageContext;
            _sceneResourceProvider = sceneResourceProvider;
            _eventBus = eventBus;
            _sceneLifetimeScope = sceneLifeTimeScope;
            _skillFactory = skillFactory;
            _skillObjectFactory = skillObjectFactory;
            Initialize();
        }

        /// <summary>
        /// StageConfigTableEntry의 EnemyKeys(EnemyConfigTableEntry Id 목록)를 로드하여 저장합니다.
        /// </summary>
        private void Initialize()
        {
            var stageConfigEntry = _tableRepository.GetTableEntry<StageConfigTableEntry>(_stageContext.StageConfigTableId);
            if (stageConfigEntry == null)
            {
                Debug.LogWarning($"[{nameof(EnemyFactory)}] StageConfigTableEntry를 찾을 수 없습니다. ID: {_stageContext.StageConfigTableId}");
                return;
            }

            if (stageConfigEntry.EnemyKeys != null && stageConfigEntry.EnemyKeys.Count > 0)
            {
                _enemyIds = new List<int>(stageConfigEntry.EnemyKeys);
            }
        }

        /// <summary>
        /// 스테이지에 맞는 랜덤 적을 가져옵니다.
        /// </summary>
        public async UniTask<EntityBase> GetRandomEnemyAsync(Vector3 position = default, Quaternion rotation = default, Transform parent = null, bool instantiateInWorldSpace = true)
        {
            var enemyConfig = GetRandomEnemyTableConfig();
            
            if (enemyConfig == null)
                return null;
            
            var entity = GetFromPool(enemyConfig, position, rotation, parent, instantiateInWorldSpace);
            
            if (entity == null)
                entity = await CreateAsync(enemyConfig, position, rotation, parent, instantiateInWorldSpace);

            return entity;
        }

        /// <summary>
        /// 스테이지에 맞는 랜덤 적을 동기적으로 가져옵니다.
        /// </summary>
        public EntityBase GetRandomEnemySync(Vector3 position = default, Quaternion rotation = default, Transform parent = null, bool instantiateInWorldSpace = true)
        {
            var enemyConfig = GetRandomEnemyTableConfig();
            
            if (enemyConfig == null)
                return null;
            
            var entity = GetFromPool(enemyConfig, position, rotation, parent, instantiateInWorldSpace);
            
            if (entity == null)
                entity = CreateSync(enemyConfig, position, rotation, parent, instantiateInWorldSpace);

            return entity;
        }

        /// <summary>
        /// 지정한 EnemyConfigTableEntry ID로 적을 비동기 생성합니다.
        /// </summary>
        public async UniTask<EntityBase> GetEnemyByConfigIdAsync(int configId, Vector3 position = default, Quaternion rotation = default, Transform parent = null, bool instantiateInWorldSpace = true)
        {
            var enemyConfig = _tableRepository.GetTableEntry<EnemyConfigTableEntry>(configId);
            
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
            var enemyConfig = _tableRepository.GetTableEntry<EnemyConfigTableEntry>(configId);
            
            if (enemyConfig == null)
                return null;
            
            var entity = GetFromPool(enemyConfig, position, rotation, parent, instantiateInWorldSpace);
            
            if (entity == null)
                entity = CreateSync(enemyConfig, position, rotation, parent, instantiateInWorldSpace);

            return entity;
        }

        private EntityBase GetFromPool(EnemyConfigTableEntry entry, Vector3 position = default, Quaternion rotation = default, Transform parent = null,  bool instantiateInWorldSpace = true)
        {
            var poolKey = GetPoolKey(entry.GameObjectKey);
            var go = _pool.Get(poolKey);
            
            if (go != null)
            {
                ApplyTransform(go.transform, position, rotation, parent, instantiateInWorldSpace);
                go.SetActive(true);
                return InitializeEnemyInstance(go, entry,false);
            }
            else
            {
                return null;
            }
        }

        private EntityBase CreateSync(EnemyConfigTableEntry entry, Vector3 position = default, Quaternion rotation = default, Transform parent = null,  bool instantiateInWorldSpace = true)
        {
            var go = _sceneResourceProvider.GetInstanceSync(entry.GameObjectKey, position, rotation, parent, instantiateInWorldSpace);
            var poolKey = GetPoolKey(entry.GameObjectKey);
            EnsurePoolable(go, poolKey);
            return InitializeEnemyInstance(go, entry, true);
        }
        
        private async UniTask<EntityBase> CreateAsync(EnemyConfigTableEntry entry, Vector3 position = default, Quaternion rotation = default, Transform parent = null,  bool instantiateInWorldSpace = true)
        {
            var go = await _sceneResourceProvider.GetInstanceAsync(entry.GameObjectKey, position, rotation, parent, instantiateInWorldSpace);
            var poolKey = GetPoolKey(entry.GameObjectKey);
            EnsurePoolable(go, poolKey);
            return InitializeEnemyInstance(go, entry,true);
        }
        
        /// <summary>
        /// EnemyKeys에서 랜덤 ID를 선택하고, 해당 EnemyConfigTableEntry의 GameObjectKey(어드레스)를 반환합니다.
        /// </summary>
        private EnemyConfigTableEntry GetRandomEnemyTableConfig()
        {
            if (_enemyIds == null || _enemyIds.Count == 0)
            {
                Debug.LogWarning($"[{nameof(EnemyFactory)}] 적 ID 목록이 비어있습니다.");
                return null;
            }

            var enemyId = _enemyIds[Random.Range(0, _enemyIds.Count)];
            var enemyEntry = _tableRepository.GetTableEntry<EnemyConfigTableEntry>(enemyId);
            if (enemyEntry == null)
            {
                Debug.LogWarning($"[{nameof(EnemyFactory)}] EnemyConfigTableEntry를 찾을 수 없습니다. ID: {enemyId}");
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

        /// <summary>
        /// 인스턴스에 필요한 컴포넌트를 붙이고 초기화합니다
        /// </summary>
        private EntityBase InitializeEnemyInstance(GameObject enemyInstance, EnemyConfigTableEntry configTableEntry, bool isFirstInit)
        {
            if (enemyInstance == null)
            {
                Debug.LogWarning($"[{nameof(EnemyFactory)}] 적 인스턴스 생성 실패: {configTableEntry.GameObjectKey}");
                return null;
            }

            enemyInstance.tag = GameTags.Enemy;
            enemyInstance.layer = PhysicalLayers.Enemy.LayerIndex;

            EntityLifeTimeScope entityLifeTimeScope = null;
            using (LifetimeScope.EnqueueParent(_sceneLifetimeScope))
            {
                entityLifeTimeScope = enemyInstance.AddOrGetComponent<EntityLifeTimeScope>();
            }

            var entity = entityLifeTimeScope.Container.Resolve<EntityBase>();
            var statsEntry = _tableRepository.GetTableEntry<EntityStatsTableEntry>(configTableEntry.StatsId);
            
            if (statsEntry != null)
            {
                var statGroup = new EntityStatContainer();
                statGroup.Initialize(statsEntry);
                entity.SetStatGroup(statGroup);
            }
            
            var healthComponent = entityLifeTimeScope.Container.Resolve<HealthComponent>();    
            if (isFirstInit == true)
            {
                var moveComponent = entityLifeTimeScope.Container.Resolve<MovementComponent>();
                healthComponent.OnDeath += () =>
                {
                    var destroyEffectSpawnPos = entity.transform.position;
                    _skillObjectFactory.CreateSkillObjectAsync<ParticleSkillObject>(CommonAddresses.MonsterDeath_Particle, destroyEffectSpawnPos).Forget();
                    _eventBus.Publish(new EnemyDeadEvent { enemy = entity, enemyConfigTableEntry = configTableEntry });
                    entity.Release();
                };
            }
            var stateMachine = entityLifeTimeScope.Container.Resolve<IEntityStateMachine>();
            healthComponent.ResetState();

            var skillContainer = entityLifeTimeScope.Container.Resolve<EntitySkillContainer>();
            entity.SetSkillGroup(skillContainer);

            var activeSkills = new List<Skill>();
            
            foreach (var skillId in configTableEntry.ActiveSkills)
            {
                var skill = _skillFactory.CreateSkillSync(skillId);
                skillContainer.Regist(skill);
                activeSkills.Add(skill);
            }

            var aiBT = _sceneResourceProvider.GetAssetSync<AiBTBase>(configTableEntry.AIType);
            entityLifeTimeScope.Container.Resolve<AIComponent>().Initialize(aiBT, activeSkills);

            return entity;
        }
    }
}
