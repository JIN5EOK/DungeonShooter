using System;
using VContainer;

namespace DungeonShooter
{
    public class GameResultService : IGameResultService, IDisposable
    {
        public event Action<GameResultModel> OnGameResult;
        
        private readonly IPauseManager _pauseManager;

        [Inject]
        public GameResultService(IPauseManager pauseManager)
        {
            _pauseManager = pauseManager;
        }

        public void ExecuteGameResult(GameResult result)
        {
            _pauseManager.PauseRequest(this);
            
            var model = new GameResultModel
            {
                Result = result,
                EnemyKillCount = 0, // TODO: 실제 값 적용
                PlayTimeSecond = 0 
            };
            
            OnGameResult?.Invoke(model);
        }

        public void Dispose()
        {
            _pauseManager?.ResumeRequest(this);
        }
    }
}
