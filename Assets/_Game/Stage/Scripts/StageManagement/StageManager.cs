using UnityEngine;
using VContainer;

namespace DungeonShooter
{
    public class StageManager : MonoBehaviour
    {
        private StageContext _context;
        private Stage _stage;

        public Stage Stage => _stage;
        
        private IRoomDataRepository _roomDataRepository;
        private ITileRepository _tileRepository;
        private IPlayerFactory _playerFactory;
        private IEnemyFactory _enemyFactory;
        private ISceneResourceProvider _sceneResourceProvider;

        [Inject]
        public void Construct(StageContext context, IRoomDataRepository roomDataRepository, ITileRepository tileRepository, IPlayerFactory playerFactory, IEnemyFactory enemyFactory, ISceneResourceProvider sceneResourceProvider)
        {
            _context = context;
            _roomDataRepository = roomDataRepository;
            _tileRepository = tileRepository;
            _playerFactory = playerFactory;
            _enemyFactory = enemyFactory;
            _sceneResourceProvider = sceneResourceProvider;
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
            await StageInstantiator.InstantiateStage(_tileRepository, _playerFactory, _enemyFactory, _sceneResourceProvider, _stage);
        }
        
        private void OnDestroy()
        {
            _context?.Dispose();
        }
    }
}