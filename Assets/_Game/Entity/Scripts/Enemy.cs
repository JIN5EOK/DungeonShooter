using Cysharp.Threading.Tasks;
using Jin5eok;
using UnityEngine;
using VContainer;

namespace DungeonShooter
{

    public class Enemy : EntityBase
    {
        private AiBTBase _aiBT;

        private Transform _playerTransform;
        private HealthComponent _healthComponent;
        private MovementComponent _movementComponent;
        private ISceneResourceProvider _resourceProvider;
        private ITableRepository _tableRepository;
        private EnemyConfigTableEntry _enemyConfigTableEntry;
        [Inject]
        private void Construct(ISceneResourceProvider resourceProvider, ITableRepository tableRepository)
        {
            _resourceProvider = resourceProvider;
            _tableRepository = tableRepository;
        }
        
        public void Initialize(EnemyConfigTableEntry enemyConfigTableEntry)
        {
            _enemyConfigTableEntry = enemyConfigTableEntry;
            var statsEntry = _tableRepository.GetTableEntry<EntityStatsTableEntry>(_enemyConfigTableEntry.StatsId);
            var statGroup = new EntityStatGroup();
            statGroup.Initialize(statsEntry);
            SetStatGroup(statGroup);
            _movementComponent = gameObject.AddOrGetComponent<MovementComponent>();
            _healthComponent = gameObject.AddOrGetComponent<HealthComponent>();
            _healthComponent.OnDeath += () => CoroutineManager.Delay(0.5f, () => Destroy(gameObject));
            
            gameObject.AddOrGetComponent<AIComponent>().SetBT(_resourceProvider.GetAssetSync<AiBTBase>(enemyConfigTableEntry.AIType));
        }
    }
}
