using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

namespace DungeonShooter
{
    /// <summary>
    /// 게임 매니저. 
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        private StageManager _stageManager;
        private PlayerManager _playerManager;
        [Inject]
        public void Construct(StageManager stageManager, PlayerManager playerManager)
        {
            _stageManager = stageManager;
            _playerManager = playerManager;
        }
        
        private async UniTaskVoid Start()
        {
            await _stageManager.CreateStageAsync();
        }
    }
}
