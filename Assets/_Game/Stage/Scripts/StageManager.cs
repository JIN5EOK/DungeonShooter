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
        private IStageResourceProvider _stageResourceProvider;

        [Inject]
        public void Construct(StageContext context, IRoomDataRepository roomDataRepository, IStageResourceProvider stageResourceProvider)
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
            await StageInstantiator.InstantiateStage(_stageResourceProvider, _stage);
        }
        
        private void OnDestroy()
        {
            _context?.Dispose();
        }
    }
}