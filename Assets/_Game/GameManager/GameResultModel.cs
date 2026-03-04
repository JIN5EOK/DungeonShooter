namespace DungeonShooter
{
    public enum GameResult
    {
        None,
        Clear,
        Dead
    }

    public class GameResultModel
    {
        public GameResult Result { get; set; }
        public int EnemyKillCount { get; set; }
        public int PlayTimeSecond { get; set; }
    }
}
