using System;
using UnityEngine;
using UnityEngine.UI;

namespace DungeonShooter
{
    /// <summary>
    /// 게임시작 버튼만 있는 간단한 팝업 UI. 버튼 클릭 시 콜백을 등록해 사용할 수 있다.
    /// </summary>
    public class PressToStartUI : PopupUI
    {
        [Header("게임 시작")]
        [SerializeField] private Button _gameStartButton;

        /// <summary> 게임시작 버튼 클릭 시 발생하는 이벤트 </summary>
        public event Action OnGameStartClicked;

        private void Awake()
        {
            if (_gameStartButton != null)
                _gameStartButton.onClick.AddListener(HandleGameStartClicked);
        }

        private void HandleGameStartClicked()
        {
            OnGameStartClicked?.Invoke();
        }

        private void OnDestroy()
        {
            if (_gameStartButton != null)
                _gameStartButton.onClick.RemoveListener(HandleGameStartClicked);
        }
    }
}
