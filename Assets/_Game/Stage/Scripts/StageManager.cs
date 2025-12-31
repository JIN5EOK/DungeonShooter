using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Jin5eok;
using Mono.Cecil.Cil;
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
            _stage = StageGenerator.GenerateStage(15, new List<string> { "Assets/_Game/Stage/Rooms/Stage001/Stage001_0001.json", "Assets/_Game/Stage/Rooms/Stage001/Stage001_0002.json" });
            var stageObj = await StageInstantiator.InstantiateStage(_stage);
            _stageComponent = stageObj != null ? stageObj.GetComponent<StageComponent>() : null;
        }
    }
}