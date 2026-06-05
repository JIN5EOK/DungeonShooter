using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DungeonShooter
{
    /// <summary>
    /// 아이템 정보를 표시하는 팝업 윈도우 UI.
    /// </summary>
    public class ItemInfoWindow : MonoBehaviour
    {
        [Header("아이템 정보")]
        [SerializeField] private Image _iconImage;
        [SerializeField] private TextMeshProUGUI _textName;
        [SerializeField] private TextMeshProUGUI _textDescription;
        [SerializeField] private TextMeshProUGUI _textType;
        [SerializeField] private TextMeshProUGUI _textStats;
        
        /// <summary>
        /// 슬롯 뷰모델로 표시 내용을 설정합니다.
        /// </summary>
        public void SetSlot(IItemViewModel slot)
        {
            if (slot == null)
            {
                Clear();
                return;
            }

            SetText(_textName, slot.ItemNameText);
            SetText(_textDescription, slot.ItemDescriptionText);
            SetText(_textType, slot.ItemTypeText);
            SetText(_textStats, slot.ItemEffectsText);
            
            if (_iconImage != null)
            {
                _iconImage.sprite = slot.Icon;
                _iconImage.enabled = slot.Icon != null;
            }
        }

        public void Clear()
        {
            SetText(_textName, string.Empty);
            SetText(_textDescription, string.Empty);
            SetText(_textType, string.Empty);
            SetText(_textStats, string.Empty);

            if (_iconImage != null)
            {
                _iconImage.sprite = null;
                _iconImage.enabled = false;
            }
        }

        private void SetText(TextMeshProUGUI textUi, string value)
        {
            if (textUi != null)
                textUi.text = value ?? string.Empty;
        }
    }
}
