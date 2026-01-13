using UnityEngine;
using VContainer;

namespace DungeonShooter
{
    public class StageManager : MonoBehaviour
    {
        private StageContext _context;
        private Stage _stage;
        private StageComponent _stageComponent;

        public Stage Stage => _stage;
        public StageComponent StageComponent => _stageComponent;
        
        [Inject]
        public void Construct(StageContext context)
        {
            _context = context;
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
            // 방 데이터 제공
            var roomDataRepository = new RoomDataRepository(_context.StageConfig);
            await roomDataRepository.InitializeAsync();
            // 스테이지 리소스 제공
            var stageResourceProvider = new StageResourceProvider(_context.StageConfig);
            await stageResourceProvider.InitializeAsync();
            
            _stage = await StageGenerator.GenerateStage(roomDataRepository, 15);
            var stageObj = await StageInstantiator.InstantiateStage(stageResourceProvider, _stage);
            _stageComponent = stageObj.GetComponent<StageComponent>();
        }
        
        private void OnDestroy()
        {
            _context?.Dispose();
        }
    }
}