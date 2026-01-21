using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DungeonShooter
{
    /// <summary>
    /// ESC 입력 또는 버튼을 통해 게임 일시정지 상태를 토글하는 UI 컨트롤러
    /// </summary>
    public class PauseMenuUI : MonoBehaviour
    {
        [Header("UI 참조")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform windowRoot;

        [Header("버튼 참조 (비워두면 자동 검색)")]
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button quitButton;

        [Header("입력 설정")]
        [SerializeField] private KeyCode toggleKey = KeyCode.Escape;

        private bool _isPaused;
        private float _cachedTimeScale = 1f;

        private void Awake()
        {
            CacheReferences();
            RegisterButtonEvents();
            HideImmediate();
        }

        private void OnDestroy()
        {
            UnregisterButtonEvents();
            RestoreTimeScale();
        }

        private void Update()
        {
            if (Input.GetKeyDown(toggleKey))
            {
                TogglePause();
            }
        }

        public void TogglePause()
        {
            if (_isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }

        public void PauseGame()
        {
            if (_isPaused) return;

            _cachedTimeScale = Time.timeScale;
            SetTimeScale(0f);
            _isPaused = true;
            ApplyCanvasVisibility(1f, true);
        }

        public void ResumeGame()
        {
            if (!_isPaused) return;

            _isPaused = false;
            SetTimeScale(Mathf.Approximately(_cachedTimeScale, 0f) ? 1f : _cachedTimeScale);
            ApplyCanvasVisibility(0f, false);
        }

        private void RestartGame()
        {
            ResumeGame();
            var activeScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(activeScene.buildIndex);
        }

        private void QuitGame()
        {
            ResumeGame();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void CacheReferences()
        {
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }

            if (windowRoot == null && transform.childCount > 0)
            {
                windowRoot = transform.GetChild(0) as RectTransform;
            }

            var buttons = GetComponentsInChildren<Button>(true);
            if (resumeButton == null)
            {
                resumeButton = Array.Find(buttons, b => b.name.IndexOf("Resume", StringComparison.OrdinalIgnoreCase) >= 0);
            }
            if (restartButton == null)
            {
                restartButton = Array.Find(buttons, b => b.name.IndexOf("Restart", StringComparison.OrdinalIgnoreCase) >= 0);
            }
            if (quitButton == null)
            {
                quitButton = Array.Find(buttons, b => b.name.IndexOf("Quit", StringComparison.OrdinalIgnoreCase) >= 0
                                                       || b.name.IndexOf("Exit", StringComparison.OrdinalIgnoreCase) >= 0
                                                       || b.name.IndexOf("Leave", StringComparison.OrdinalIgnoreCase) >= 0);
            }
        }

        private void RegisterButtonEvents()
        {
            if (resumeButton != null)
            {
                resumeButton.onClick.AddListener(ResumeGame);
            }

            if (restartButton != null)
            {
                restartButton.onClick.AddListener(RestartGame);
            }

            if (quitButton != null)
            {
                quitButton.onClick.AddListener(QuitGame);
            }
        }

        private void UnregisterButtonEvents()
        {
            if (resumeButton != null)
            {
                resumeButton.onClick.RemoveListener(ResumeGame);
            }

            if (restartButton != null)
            {
                restartButton.onClick.RemoveListener(RestartGame);
            }

            if (quitButton != null)
            {
                quitButton.onClick.RemoveListener(QuitGame);
            }
        }

        private void HideImmediate()
        {
            _isPaused = false;
            ApplyCanvasVisibility(0f, false);
        }

        private void ApplyCanvasVisibility(float alpha, bool enableInteraction)
        {
            if (canvasGroup == null) return;

            canvasGroup.alpha = alpha;
            canvasGroup.blocksRaycasts = enableInteraction;
            canvasGroup.interactable = enableInteraction;

            if (windowRoot != null)
            {
                windowRoot.gameObject.SetActive(enableInteraction);
            }
        }

        private void SetTimeScale(float value)
        {
            Time.timeScale = value;
            AudioListener.pause = value == 0f;
        }

        private void RestoreTimeScale()
        {
            if (Mathf.Approximately(Time.timeScale, 0f))
            {
                Time.timeScale = 1f;
                AudioListener.pause = false;
            }
        }
    }
}

