using System;
using VContainer;

namespace DungeonShooter
{
    public class GamePausePresenter : IDisposable
    {
        private readonly IPauseManager _pauseManager;
        private readonly IGameExitService _gameExitService;

        private readonly GamePauseView _view;

        [Inject]
        public GamePausePresenter(IPauseManager pauseManager, IGameExitService gameExitService, GamePauseView view)
        {
            _pauseManager = pauseManager;
            _gameExitService = gameExitService;
            _view = view;

            if (_view != null)
            {
                _view.OnResumeClickedEvent += ResumeGame;
                _view.OnExitClickedEvent += ExitGame;
            }
        }

        public void PauseGame()
        {
            _pauseManager.PauseRequest(this);
            _view?.Show();
        }

        public void ResumeGame()
        {
            _pauseManager.ResumeRequest(this);
            _view?.Hide();
        }

        public void ExitGame()
        {
            _pauseManager.ResumeRequest(this);
            _gameExitService.ExitToMainMenu();
        }

        public void Dispose()
        {
            if (_view != null)
            {
                _view.OnResumeClickedEvent -= ResumeGame;
                _view.OnExitClickedEvent -= ExitGame;
            }

            _pauseManager?.ResumeRequest(this);
        }
    }
}
