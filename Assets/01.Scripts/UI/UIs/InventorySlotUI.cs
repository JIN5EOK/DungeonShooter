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
        [SerializeField] private Image _iconImage;
        [SerializeField] private TextMeshProUGUI _textStack;
        [SerializeField] private GameObject _equipMark;
        
        [SerializeField] private Button _button;

        private IItemViewModel _boundSlot;

        /// <summary>현재 바인딩된 슬롯 뷰모델</summary>
        public IItemViewModel BoundSlot => _boundSlot;

        /// <summary>슬롯 클릭 시 호출 (바인딩된 슬롯 전달, 빈 슬롯이면 null)</summary>
        public event Action<IItemViewModel> OnSlotClicked;

        private void Awake()
        {
            if (_button != null)
                _button.onClick.AddListener(HandleClick);
        }

        public void SetEquipped(bool equipped)
        {
            _equipMark.gameObject.SetActive(equipped);
        }
        
        /// <summary>
        /// 슬롯에 표시할 뷰모델을 설정합니다.
        /// </summary>
        public void SetSlot(IItemViewModel slot)
        {
            _boundSlot = slot;
            
            SetIcon(slot.Icon);
            SetStackText(slot.StackCount, slot.MaxStackCount);
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
            OnSlotClicked?.Invoke(_boundSlot);
        }
        
        private void OnDestroy()
        {
            if (_button != null)
                _button.onClick.RemoveListener(HandleClick);
        }
    }
}
