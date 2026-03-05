using System;
using VContainer;

namespace DungeonShooter
{
    public class GameResultPresenter : IDisposable
    {
        private readonly IGameResultService _gameResultService;
        private readonly IGameExitService _gameExitService;
        private readonly ITableRepository _tableRepository;

        private readonly GameResultView _view;

        [Inject]
        public GameResultPresenter(IGameResultService gameResultService, IGameExitService gameExitService,
            ITableRepository tableRepository, GameResultView view)
        {
            _gameResultService = gameResultService;
            _gameExitService = gameExitService;
            _tableRepository = tableRepository;
            _view = view;

            _gameResultService.OnGameResult += HandleGameResult;

            if (_view != null)
            {
                _view.OnExitClickedEvent += ExitGame;
            }
        }

        private void HandleGameResult(GameResultModel model)
        {
            string resultMessage = _tableRepository.GetStringText(
                model.Result == GameResult.Clear 
                ? 19200065 : 19200066);

            var killCountEntry = _tableRepository.GetTableEntry<StringTextTableEntry>(19200067);
            string enemyKillCountMessage = killCountEntry.Format(model.EnemyKillCount);

            var playTimeEntry = _tableRepository.GetTableEntry<StringTextTableEntry>(19200068);
            string playTimeMessage = playTimeEntry.Format(model.PlayTimeSecond);

            string exitButtonMessage = _tableRepository.GetStringText(19200069);

            _view?.ShowResult(resultMessage, enemyKillCountMessage, playTimeMessage, exitButtonMessage);
        }

        public void ExitGame()
        {
            _gameExitService.ExitToMainMenu();
        }

        public void Dispose()
        {
            if (_gameResultService != null)
                _gameResultService.OnGameResult -= HandleGameResult;

            if (_view != null)
            {
                _view.OnExitClickedEvent -= ExitGame;
            }
        }
    }
}
