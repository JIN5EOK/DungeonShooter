using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

namespace DungeonShooter
{
    /// <summary>
    /// 게임 전용 UI 매니저. 
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        private StageManager _stageManager;
        private UIManager _uiManager;
        
        [Inject]
        public void Construct(StageManager stageManager, UIManager uiManager)
        {
            _stageManager = stageManager;
            _uiManager =  uiManager;
        }

        public async UniTaskVoid Start()
        {
            var hpHudUI = await _uiManager.CreateUIAsync<HealthBarUI>("UI_HpHud");
            await _stageManager.CreateStageAsync();
        }
    }
}
