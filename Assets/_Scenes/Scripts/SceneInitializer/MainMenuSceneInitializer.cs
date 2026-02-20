using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

namespace DungeonShooter
{
    public class MainMenuSceneInitializer : MonoBehaviour, ISceneInitializer
    {
        public bool IsSceneInitialized { get; private set; }
        private UIManager _uiManager;
        private GameStartUI _gameStartUI;

        [Inject]
        public void Construct(UIManager uiManager)
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