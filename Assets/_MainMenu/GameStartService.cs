using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DungeonShooter;
using VContainer;

namespace _MainMenu
{
    public interface IGameStartService
    {
        PlayerConfigTableEntry SelectedPlayer { get; set; }
        StageConfigTableEntry SelectedStage { get; set; }
        IReadOnlyList<PlayerConfigTableEntry> GetSelectablePlayers();
        IReadOnlyList<StageConfigTableEntry> GetSelectableStages();
        UniTask GameStart();
    }

    public class GameStartService : IGameStartService
    {
        private readonly ITableRepository _tableRepository;

        public PlayerConfigTableEntry SelectedPlayer { get; set; }
        public StageConfigTableEntry SelectedStage { get; set; }

        public GameStartService(ITableRepository tableRepository)
        {
            _tableRepository = tableRepository;
        }

        public IReadOnlyList<PlayerConfigTableEntry> GetSelectablePlayers()
        {
            return _tableRepository?.GetAllTableEntries<PlayerConfigTableEntry>() ?? new List<PlayerConfigTableEntry>();
        }

        public IReadOnlyList<StageConfigTableEntry> GetSelectableStages()
        {
            return _tableRepository?.GetAllTableEntries<StageConfigTableEntry>() ?? new List<StageConfigTableEntry>();
        }

        public async UniTask GameStart()
        {
            if (SelectedPlayer == null || SelectedStage == null)
                return;

            var loader = new SceneLoader();
            var context = new StageContext(SelectedPlayer.Id, SelectedStage.Id);
            await loader.AddContext(context).LoadScene(SceneNames.StageScene);
        }
    }
}
