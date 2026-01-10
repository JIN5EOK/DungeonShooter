using System.Collections.Generic;
using UnityEngine;

namespace DungeonShooter
{
    public class StageManager : MonoBehaviour
    {
        [SerializeField] private StageConfig _testStageConfig;
        private Stage _stage;
        private StageComponent _stageComponent;

        public Stage Stage => _stage;
        public StageComponent StageComponent => _stageComponent;

        // 스테이지 생성 테스트를 위한 임시코드들
        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                CreateStageAsync();
            }
        }

        private async void CreateStageAsync()
        {
            _stage = await StageGenerator.GenerateStage(await GetRoomDataRepository(_testStageConfig), 15);
            var stageObj = await StageInstantiator.InstantiateStage(await GetResourceProvider(_testStageConfig), _stage);
            _stageComponent = stageObj != null ? stageObj.GetComponent<StageComponent>() : null;
        }

        private async Awaitable<IRoomDataRepository> GetRoomDataRepository(StageConfig config)
        {
            var roomDataRepository = new RoomDataRepository(config);
            await roomDataRepository.InitializeAsync();
            return roomDataRepository;
        }

        private async Awaitable<IStageResourceProvider> GetResourceProvider(StageConfig config)
        {
            var stageResourceProvider = new StageResourceProvider(config);
            return stageResourceProvider;
        }
    }
}