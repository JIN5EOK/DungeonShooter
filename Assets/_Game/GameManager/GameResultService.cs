using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace DungeonShooter
{
    public class GameResultService : IGameResultService, IDisposable, ITickable
    {
        public event Action<GameResultModel> OnGameResult;

        private readonly IPauseManager _pauseManager;

        private int _enemyKillCount;
        private float _playTime;
        private bool _isGameOver;

        [Inject]
        public GameResultService(IPauseManager pauseManager)
        {
            _pauseManager = pauseManager;
        }

        internal void OnEnemyDead(EnemyDeadEvent ev)
        {
            if (_isGameOver) return;
            _enemyKillCount++;
        }

        public void Tick()
        {
            if (_isGameOver || _pauseManager.IsPaused) return;

            _playTime += Time.deltaTime;
        }

        public void ExecuteGameResult(GameResult result)
        {
            if (_isGameOver) return;
            _isGameOver = true;

            _pauseManager.PauseRequest(this);

            var model = new GameResultModel
            {
                Result = result,
                EnemyKillCount = _enemyKillCount,
                PlayTimeSecond = Mathf.FloorToInt(_playTime)
            };

            OnGameResult?.Invoke(model);
        }

        public void Dispose()
        {
            _pauseManager?.ResumeRequest(this);
        }
    }
}
