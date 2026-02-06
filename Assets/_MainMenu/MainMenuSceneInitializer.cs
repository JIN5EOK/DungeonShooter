using Cysharp.Threading.Tasks;
using DungeonShooter;
using UnityEngine;
using VContainer;

public class MainMenuSceneInitializer : MonoBehaviour
{
    private readonly string GameStartUIAddress = "CharacterSelectUI";
    private readonly string StageScene = "StageScene";
    
    private UIManager _uiManager;
    private GameStartUI _gameStartUI;

    [Inject]
    public void Construct(UIManager uiManager)
    {
        _uiManager = uiManager;
    }

    public async UniTaskVoid Start()
    {
        _gameStartUI = await _uiManager.CreateUIAsync<GameStartUI>(GameStartUIAddress);
        _gameStartUI.OnGameStartRequested += HandleGameStartRequested;
    }

    private void OnDestroy()
    {
        _uiManager.RemoveUI(_gameStartUI);
    }

    private async void HandleGameStartRequested()
    {
        if (_gameStartUI == null)
            return;

        var playerEntry = _gameStartUI.SelectedPlayerConfigEntry;
        var stageEntry = _gameStartUI.SelectedStageConfigEntry;
        if (playerEntry == null || stageEntry == null)
            return;

        var loader = new SceneLoader();
        var context = new StageContext(playerEntry.Id, stageEntry.Id);
        await loader.AddContext(context).LoadScene(StageScene);
    }
}
