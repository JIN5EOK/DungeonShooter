// using System;
// using Cysharp.Threading.Tasks;
// using UnityEngine;
// using UnityEngine.UI;
// using VContainer;
//
// namespace DungeonShooter
// {
//     /// <summary>
//     /// 일시정지 메뉴 UI. 스테이지 재시작, 메인메뉴로 이동을 제공한다.
//     /// </summary>
//     public class PauseMenuUI : PopupUI
//     {
//         [SerializeField] private RectTransform _windowRoot;
//
//         [SerializeField] private Button _resumeButton;
//         [SerializeField] private Button _restartButton;
//         [SerializeField] private Button _mainMenuButton;
//
//         private StageContext _stageContext;
//         private IEventBus _eventBus;
//         
//         [Inject]
//         public void Construct(IEventBus eventBus, StageContext stageContext)
//         {
//             _stageContext = stageContext;
//         }
//
//         private void Awake()
//         {
//             if (_resumeButton != null)
//                 _resumeButton.gameObject.SetActive(false);
//             RegisterButtonEvents();
//             base.Hide();
//         }
//
//         public override void Show()
//         {
//             base.Show();
//             _eventBus.Publish(new PauseRequestEvent(this, true));
//         }
//
//         public override void Hide()
//         {
//             _eventBus.Publish(new PauseRequestEvent(this, false));
//             base.Hide();
//         }
//
//         private void OnRestartClicked()
//         {
//             var loader = new SceneLoader();
//             loader.AddContext(_stageContext).LoadScene(SceneNames.StageScene).Forget();
//         }
//
//         private void OnMainMenuClicked()
//         {
//             var loader = new SceneLoader();
//             loader.LoadScene(SceneNames.MainMenuScene).Forget();
//         }
//
//         private void RegisterButtonEvents()
//         {
//             if (_resumeButton != null)
//                 _resumeButton.onClick.AddListener(() => Hide());
//             if (_restartButton != null)
//                 _restartButton.onClick.AddListener(OnRestartClicked);
//             if (_mainMenuButton != null)
//                 _mainMenuButton.onClick.AddListener(OnMainMenuClicked);
//         }
//
//         private void UnregisterButtonEvents()
//         {
//             if (_resumeButton != null)
//                 _resumeButton.onClick.RemoveAllListeners();
//             if (_restartButton != null)
//                 _restartButton.onClick.RemoveListener(OnRestartClicked);
//             if (_mainMenuButton != null)
//                 _mainMenuButton.onClick.RemoveListener(OnMainMenuClicked);
//         }
//         
//         protected override void OnDestroy()
//         {
//             base.OnDestroy();
//             UnregisterButtonEvents();
//             if (gameObject.activeSelf)
//                 _eventBus.Publish(new PauseRequestEvent(this, false));
//         }
//     }
// }
