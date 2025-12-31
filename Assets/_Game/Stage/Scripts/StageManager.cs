using System;
using System.Collections.Generic;
using Jin5eok;
using Mono.Cecil.Cil;
using UnityEngine;

namespace DungeonShooter
{
    public class StageManager : MonoBehaviour
    {
        // 스테이지 생성 테스트를 위한 임시코드들
        AddressablesScope stageScope = new AddressablesScope();
        
        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                var stage = StageGenerator.GenerateStage(15, new List<string> { "Assets/_Game/Stage/Rooms/Stage001/Stage001_0001.json", "Assets/_Game/Stage/Rooms/Stage001/Stage001_0002.json" });
                var stageObj = StageInstantiator.InstantiateStage(stage, stageScope);
            }
            
        }
    }
}