using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DungeonShooter;
using VContainer;

namespace _MainMenu
{
    public interface IGameStartService
    {
        PlayerConfigSo SelectedPlayer { get; set; }
        StageConfigTableEntry SelectedStage { get; set; }
        IReadOnlyList<PlayerConfigSo> GetSelectablePlayers();
        IReadOnlyList<StageConfigTableEntry> GetSelectableStages();
        UniTask GameStart();
    }

    public class GameStartService : IGameStartService
    {
        private readonly ITableRepository _tableRepository;
        private readonly SceneLoader _sceneLoader;

        public PlayerConfigSo SelectedPlayer { get; set; }
        public StageConfigTableEntry SelectedStage { get; set; }

        [Inject]
        public GameStartService(ITableRepository tableRepository, SceneLoader sceneLoader)
        {
            _tableRepository = tableRepository;
            _sceneLoader = sceneLoader;
        }

        public IReadOnlyList<PlayerConfigSo> GetSelectablePlayers()
        {
            return _tableRepository?.GetAllTableEntries<PlayerConfigSo>() ?? new List<PlayerConfigSo>();
        }

        public IReadOnlyList<StageConfigTableEntry> GetSelectableStages()
        {
            return _tableRepository?.GetAllTableEntries<StageConfigTableEntry>() ?? new List<StageConfigTableEntry>();
        }

        public async UniTask GameStart()
        {
            if (SelectedPlayer == null || SelectedStage == null)
                return;

            var context = new StageContext(SelectedPlayer.Id, SelectedStage.Id);
            await _sceneLoader.LoadScene(SceneNames.StageScene, context);
        }
    }
}
