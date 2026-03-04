using System;
using VContainer;

namespace DungeonShooter
{
    public class GameResultViewModel : IDisposable
    {
        private readonly IGameResultService _gameResultService;
        private readonly IGameExitService _gameExitService;
        private readonly ITableRepository _tableRepository;

        public string ResultMessage { get; private set; }
        public string EnemyKillCountMessage { get; private set; }
        public string PlayTimeMessage { get; private set; }
        public string ExitButtonMessage { get; private set; }

        public event Action OnResultUpdated;

        [Inject]
        public GameResultViewModel(IGameResultService gameResultService, IGameExitService gameExitService,
            ITableRepository tableRepository)
        {
            _gameResultService = gameResultService;
            _gameExitService = gameExitService;
            _tableRepository = tableRepository;

            _gameResultService.OnGameResult += HandleGameResult;
        }

        private void HandleGameResult(GameResultModel model)
        {
            ResultMessage = _tableRepository.GetStringText(
                model.Result == GameResult.Clear 
                ? 19200065 : 19200066);

            var killCountEntry = _tableRepository.GetTableEntry<StringTextTableEntry>(19200067);
            EnemyKillCountMessage = killCountEntry.Format(model.EnemyKillCount);

            var playTimeEntry = _tableRepository.GetTableEntry<StringTextTableEntry>(19200068);
            PlayTimeMessage = playTimeEntry.Format(model.PlayTimeSecond);

            ExitButtonMessage = _tableRepository.GetStringText(19200069);

            OnResultUpdated?.Invoke();
        }

        public void ExitGame()
        {
            _gameExitService.ExitToMainMenu();
        }

        public void Dispose()
        {
            if (_gameResultService != null)
                _gameResultService.OnGameResult -= HandleGameResult;
        }
    }
}
