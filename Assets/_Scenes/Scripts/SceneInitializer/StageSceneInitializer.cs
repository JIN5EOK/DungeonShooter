using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

namespace DungeonShooter
{
    public class StageSceneInitializer : SceneInitializerBase
    {
        [SerializeField] private StageConfig _stageConfig;

        private IPlayerFactory _playerFactory;
        private StageEventMediator _eventMediator;
        private IStageTimer _stageTimer;
        private EnemySpawner _enemySpawner;

        [Inject]
        public void Construct(
            IPlayerFactory playerFactory,
            StageEventMediator eventMediator,
            IStageTimer stageTimer,
            EnemySpawner enemySpawner)
        {
            _playerFactory = playerFactory;
            _eventMediator = eventMediator;
            _stageTimer = stageTimer;
            _enemySpawner = enemySpawner;
        }

        public async UniTaskVoid Start()
        {
            if (_stageConfig == null || _stageConfig.PlayerConfig == null)
            {
                Debug.LogWarning($"[{nameof(StageSceneInitializer)}] StageConfig 또는 PlayerConfig가 설정되지 않았습니다.");
                return;
            }

            var playerEntity = await _playerFactory.GetPlayerAsync(_stageConfig.PlayerConfig);
            var player = playerEntity as Player;

            _eventMediator.RegisterPlayer(player);
            _stageTimer.Start(_stageConfig.StageDuration);
            if (_stageConfig.SpawnInfos != null && _stageConfig.SpawnInfos.Count > 0)
                _enemySpawner.StartSpawning(_stageConfig.SpawnInfos, player?.transform);
            IsSceneInitialized = true;
        }
    }
}
