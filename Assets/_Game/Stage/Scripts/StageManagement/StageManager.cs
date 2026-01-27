using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace DungeonShooter
{
    public class StageManager : IStartable
    {
        private Stage _stage;

        public Stage Stage => _stage;
        
        private IRoomDataRepository _roomDataRepository;
        private IPlayerFactory _playerFactory;
        private IEnemyFactory _enemyFactory;
        private ISceneResourceProvider _sceneResourceProvider;
        private ITableRepository _tableRepository;
        private StageContext _stageContext;

        [Inject]
        public void Construct(IRoomDataRepository roomDataRepository, IPlayerFactory playerFactory, IEnemyFactory enemyFactory, ISceneResourceProvider sceneResourceProvider, ITableRepository tableRepository, StageContext stageContext)
        {
            _roomDataRepository = roomDataRepository;
            _playerFactory = playerFactory;
            _enemyFactory = enemyFactory;
            _sceneResourceProvider = sceneResourceProvider;
            _tableRepository = tableRepository;
            _stageContext = stageContext;
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
            var stageConfigEntry = _tableRepository.GetTableEntry<StageConfigTableEntry>(_stageContext.StageConfigTableId);
            if (stageConfigEntry == null)
            {
                Debug.LogError($"[{nameof(StageManager)}] StageConfigTableEntry를 찾을 수 없습니다. ID: {_stageContext.StageConfigTableId}");
                return;
            }
            await StageInstantiator.InstantiateStage(stageConfigEntry, _playerFactory, _enemyFactory, _sceneResourceProvider, _stage);
        }
    }
}