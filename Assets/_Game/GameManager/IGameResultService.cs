using System;

namespace DungeonShooter
{
    public interface IGameResultService
    {
        public event Action<GameResultModel> OnGameResult;
        public void ExecuteGameResult(GameResult result);

        /// <summary>적 처치 수 반영 (StageSceneInteractionMediator에서 연결)</summary>
        void OnEnemyDead(EnemyDeadEvent ev);
    }
}
