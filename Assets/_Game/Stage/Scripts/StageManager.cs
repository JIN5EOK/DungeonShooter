using System.Collections.Generic;
using UnityEngine;

namespace DungeonShooter
{
    public class StageManager : MonoBehaviour
    {
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
            var roomDataRepository = GetRoomDataRepository();
            _stage = await StageGenerator.GenerateStage(roomDataRepository, 15);
            var stageObj = await StageInstantiator.InstantiateStage(_stage);
            _stageComponent = stageObj != null ? stageObj.GetComponent<StageComponent>() : null;
            
        }

        private IRoomDataRepository GetRoomDataRepository()
        {
            var startRoomAddresses = new string[] { "Stage001_0001" };
            var normalRoomAddresses = new string[] { "Stage001_0001",
                "Stage001_0002" };
            var bossRoomAddresses = new string[] { "Stage001_0001" };
            return new RoomDataRepository(startRoomAddresses, normalRoomAddresses, bossRoomAddresses);
        }

        private IStageResourceProvider GetResourceProvider()
        {
            //return new StageResourceProvider();
            return null;
        }
    }
}