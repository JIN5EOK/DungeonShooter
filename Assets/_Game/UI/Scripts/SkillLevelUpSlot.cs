using System;
using Cysharp.Threading.Tasks;
using Unity.Android.Gradle.Manifest;
using UnityEngine;
using UnityEngine.UI;

namespace DungeonShooter
{
    /// <summary>
    /// 스킬 레벨업 선택 한 칸. 위에는 현재 레벨, 아래는 레벨업 시 스킬 정보를 표시하고 선택 버튼을 제공합니다.
    /// </summary>
    public class SkillLevelUpSlot : MonoBehaviour
    {
        [Header("현재 레벨 스킬 정보")]
        [SerializeField] public SkillInfoWindow _currentSkillInfo;

        [Header("레벨업 시 스킬 정보")]
        [SerializeField] public SkillInfoWindow _nextSkillInfo;

        [Header("선택버튼")]
        [SerializeField] private Button _selectButton;

        private Func<UniTask> _onSelect;
        private ISceneResourceProvider _sceneResourceProvider;
        
        private void Awake()
        {
            if (_selectButton != null)
                _selectButton.onClick.AddListener(HandleSelectClicked);
        }
        
        public void SetSelectHandler(Func<UniTask> onSelect)
        {
            _onSelect = onSelect;
        }
        
        private void HandleSelectClicked()
        {
            _onSelect?.Invoke();
        }
    }
}
