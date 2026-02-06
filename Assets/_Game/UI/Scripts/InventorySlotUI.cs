using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DungeonShooter
{
    /// <summary>
    /// 인벤토리 슬롯 하나를 표시하는 UI. 아이콘, 스택 수, 클릭 시 선택 이벤트.
    /// </summary>
    public class InventorySlotUIElement : MonoBehaviour
    {
        [Header("표시")]
        [SerializeField] private Image _iconImage;
        [SerializeField] private TextMeshProUGUI _textStack;

        [Header("버튼")]
        [SerializeField] private Button _button;

        private Item _boundItem;

        /// <summary>현재 바인딩된 아이템</summary>
        public Item BoundItem => _boundItem;

        /// <summary>슬롯 클릭 시 호출 (바인딩된 아이템 전달, 빈 슬롯이면 null)</summary>
        public event Action<Item> OnSlotClicked;

        private void Awake()
        {
            if (_button != null)
                _button.onClick.AddListener(HandleClick);
        }

        /// <summary>
        /// 슬롯에 표시할 아이템을 설정합니다.
        /// </summary>
        public void SetItem(Item item)
        {
            _boundItem = item;
            
            var entry = item.ItemTableEntry;
            SetIcon(item.Icon);
            SetStackText(item.StackCount, entry.MaxStackCount);
        }

        /// <summary>
        /// 아이콘 스프라이트를 설정합니다.
        /// </summary>
        public void SetIcon(Sprite sprite)
        {
            if (_iconImage == null)
                return;
            _iconImage.sprite = sprite;
            _iconImage.enabled = sprite != null;
        }

        /// <summary>
        /// 스택 수 텍스트를 설정합니다. max가 1 이하면 스택 텍스트를 숨깁니다.
        /// </summary>
        public void SetStackText(int current, int max)
        {
            if (_textStack == null)
                return;
            if (max <= 1)
            {
                _textStack.text = string.Empty;
                _textStack.gameObject.SetActive(false);
                return;
            }
            _textStack.gameObject.SetActive(true);
            _textStack.text = $"{current}/{max}";
        }

        private void HandleClick()
        {
            OnSlotClicked?.Invoke(_boundItem);
        }
        
        private void OnDestroy()
        {
            if (_button != null)
                _button.onClick.RemoveListener(HandleClick);
        }
    }
}
