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
        
        private RoomDataRepository _roomDataRepository;
        private StageResourceProvider _stageResourceProvider;

        [Inject]
        public void Construct(StageContext context, RoomDataRepository roomDataRepository, StageResourceProvider stageResourceProvider)
        {
            _context = context;
            _roomDataRepository = roomDataRepository;
            _stageResourceProvider = stageResourceProvider;
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
            _stage = await StageGenerator.GenerateStage(_roomDataRepository, 15);
            var stageObj = await StageInstantiator.InstantiateStage(_stageResourceProvider, _stage);
            _stageComponent = stageObj.GetComponent<StageComponent>();
        }
        
        private void OnDestroy()
        {
            _context?.Dispose();
        }
    }
}