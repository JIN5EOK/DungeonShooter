using System;

namespace DungeonShooter
{
    public interface IGameResultService
    {
        public event Action<GameResultModel> OnGameResult;
        public void ExecuteGameResult(GameResult result);
    }
}
