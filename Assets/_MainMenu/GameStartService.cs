using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DungeonShooter;
using VContainer;

namespace _MainMenu
{
    public interface IGameStartService
    {
        PlayerConfigSo SelectedPlayer { get; set; }
        IReadOnlyList<PlayerConfigSo> GetSelectablePlayers();
        UniTask GameStart();
    }

    public class GameStartService : IGameStartService
    {
        private readonly ITableRepository _tableRepository;
        private readonly SceneLoader _sceneLoader;

        public PlayerConfigSo SelectedPlayer { get; set; }

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

        public async UniTask GameStart()
        {
            if (SelectedPlayer == null)
                return;

            // 스테이지 시스템 제거로 인해 게임 시작 시 별도의 스테이지 씬 전환을 하지 않습니다.
            await UniTask.CompletedTask;
        }
    }
}
