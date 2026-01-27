using UnityEngine;
using VContainer;

namespace DungeonShooter
{
    public class StageManager : MonoBehaviour
    {
        private Stage _stage;

        public Stage Stage => _stage;
        
        private IRoomDataRepository _roomDataRepository;
        private IPlayerFactory _playerFactory;
        private IEnemyFactory _enemyFactory;
        private ISceneResourceProvider _sceneResourceProvider;
        private StageConfigTableEntry _stageConfigEntry;

        [Inject]
        public void Construct(IRoomDataRepository roomDataRepository, IPlayerFactory playerFactory, IEnemyFactory enemyFactory, ISceneResourceProvider sceneResourceProvider, StageConfigTableEntry stageConfigEntry)
        {
            _roomDataRepository = roomDataRepository;
            _playerFactory = playerFactory;
            _enemyFactory = enemyFactory;
            _sceneResourceProvider = sceneResourceProvider;
            _stageConfigEntry = stageConfigEntry;
        }
        
        public void Start()
        {
            CreateStageAsync();
        }
        
        /// <summary>
        /// 스테이지 구조와 인스턴스를 생성합니다
        /// </summary>
        private async void CreateStageAsync()
        {
            _stage = await StageGenerator.GenerateStage(_roomDataRepository);
            await StageInstantiator.InstantiateStage(_stageConfigEntry, _playerFactory, _enemyFactory, _sceneResourceProvider, _stage);
        }
    }
}