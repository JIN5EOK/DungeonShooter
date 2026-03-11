using Cysharp.Threading.Tasks;
using VContainer;

namespace DungeonShooter
{
    public class MainMenuSceneInitializer : SceneInitializerBase
    {
        private ISceneUIManager _uiManager;
        private GameStartUI _gameStartUI;

        [Inject]
        public void Construct(ISceneUIManager uiManager)
        {
            _uiManager = uiManager;
        }

        public async UniTaskVoid Start()
        {
            _gameStartUI = await _uiManager.CreateUIAsync<GameStartUI>(UIAddresses.GameStartUIAddress);
            IsSceneInitialized = true;
        }

        private void OnDestroy()
        {
            _uiManager.RemoveUI(_gameStartUI);
        }
    }

}