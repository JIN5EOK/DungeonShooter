using Cysharp.Threading.Tasks;
using DungeonShooter;
using UnityEngine;
using VContainer;

public class MainMenuSceneInitializer : MonoBehaviour
{
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
    }

    private void OnDestroy()
    {
        _uiManager.RemoveUI(_gameStartUI);
    }
}
