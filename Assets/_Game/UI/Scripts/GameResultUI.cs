using Cysharp.Threading.Tasks;
using DungeonShooter;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameResultUI : PopupUI
{
    [SerializeField]
    private TMP_Text _resultText;
    [SerializeField]
    private Button _restartButton;
    [SerializeField]
    private Button _mainMenuButton;
    
    private GameResultData _gameResultData;

    protected void Awake()
    {
        _restartButton.onClick.AddListener(RestartClicked);
        _mainMenuButton.onClick.AddListener(MainMenuClicked);
    }

    public void Initialize(GameResultData resultData)
    {
        _gameResultData = resultData;
        _resultText.text = _gameResultData.isClear ? "Game Clear!" : "You Are Dead..";
    }

    private void RestartClicked()
    {
        var sceneLoader = new SceneLoader();
        sceneLoader.LoadScene(SceneNames.MainMenuScene).Forget();
    }
    
    private void MainMenuClicked()
    {
        var sceneLoader = new SceneLoader();
        sceneLoader.LoadScene(SceneNames.MainMenuScene).Forget();
    }
}
