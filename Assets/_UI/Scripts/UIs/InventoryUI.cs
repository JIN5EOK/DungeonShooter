using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace DungeonShooter
{
    /// <summary>
    /// 인벤토리 뷰. ViewModel 이벤트를 구독해 슬롯/버튼/정보창만 표시한다.
    /// </summary>
    public class InventoryUI : PopupUI
    {
        [SerializeField]
        private RectTransform _content;
        [SerializeField]
        private InventorySlotUIElement _slotPrefab;
        [SerializeField]
        private ItemInfoWindow _itemInfoPanel;

        [SerializeField]
        private Button _closeButton;
        [SerializeField]
        private Button _removeButton;
        [SerializeField]
        private Button _useButton;
        [SerializeField]
        private Button _equipButton;

        private IInventoryViewModel _viewModel;
        private readonly Dictionary<Item, InventorySlotUIElement> _slotsDict = new();

        [Inject]
        public void Construct(IInventoryViewModel viewModel)
        {
            _viewModel = viewModel;

            _closeButton.onClick.AddListener(Hide);
            _useButton.onClick.AddListener(_viewModel.UseSelected);
            _equipButton.onClick.AddListener(_viewModel.EquipSelected);
            _removeButton.onClick.AddListener(_viewModel.RemoveSelected);

            _viewModel.OnItemAdded += HandleItemAdded;
            _viewModel.OnItemRemoved += HandleItemRemoved;
            _viewModel.OnItemStackChanged += HandleItemStackChanged;
            _viewModel.OnItemUse += HandleItemUse;
            _viewModel.OnSelectionChanged += HandleSelectionChanged;
            _viewModel.OnEquippedWeaponChanged += HandleEquippedWeaponChanged;

            HandleSelectionChanged(null);
            
            RefreshSlots();
            ApplyButtonState();
        }

        public override void Show()
        {
            base.Show();
            RefreshSlots();
            ApplyButtonState();
        }

        protected override void OnDestroy()
        {
            if (_viewModel != null)
            {
                _viewModel.OnItemAdded -= HandleItemAdded;
                _viewModel.OnItemRemoved -= HandleItemRemoved;
                _viewModel.OnItemStackChanged -= HandleItemStackChanged;
                _viewModel.OnItemUse -= HandleItemUse;
                _viewModel.OnSelectionChanged -= HandleSelectionChanged;
                _viewModel.OnEquippedWeaponChanged -= HandleEquippedWeaponChanged;
            }

            base.OnDestroy();
        }

        private void HandleItemAdded(Item item)
        {
            var slot = Instantiate(_slotPrefab, _content, false);
            slot.SetItem(item);
            _slotsDict.Add(item, slot);
            slot.OnSlotClicked += OnSlotClicked;
        }

        private void OnSlotClicked(Item item)
        {
            _viewModel.SelectItem(item);
            _itemInfoPanel.SetItem(item);
        }

        private void HandleSelectionChanged(Item selected)
        {
            var isSelected = selected != null;

            _closeButton.gameObject.SetActive(isSelected);;
            _equipButton.gameObject.SetActive(isSelected);
            _removeButton.gameObject.SetActive(isSelected);
            _useButton.gameObject.SetActive(isSelected);
            _itemInfoPanel.gameObject.SetActive(isSelected);
            
            if (isSelected)
            {
                _itemInfoPanel.SetItem(selected);
            }
            
            ApplyButtonState();
        }

        private void HandleItemStackChanged(Item item)
        {
            if (_slotsDict.TryGetValue(item, out var slot))
                slot.SetItem(item);
        }

        private void HandleItemUse(Item item)
        {
            if (_slotsDict.TryGetValue(item, out var slot))
                slot.SetItem(item);

            RefreshSlots();
            ApplyButtonState();
        }

        private void HandleItemRemoved(Item item)
        {
            if (!_slotsDict.TryGetValue(item, out var slot))
                return;

            if (_viewModel.SelectedItem == item)
                _viewModel.SelectItem(null);

            _slotsDict.Remove(item);
            slot.OnSlotClicked -= OnSlotClicked;
            Destroy(slot.gameObject);
            RefreshSlots();
        }

        private void HandleEquippedWeaponChanged(Item equipped)
        {
            foreach (var kv in _slotsDict)
                kv.Value.SetEquipped(kv.Key == equipped);
        }

        private void ApplyButtonState()
        {
            _equipButton.interactable = _viewModel.CanEquipSelected;
            _useButton.interactable = _viewModel.CanUseSelected;
            _removeButton.interactable = _viewModel.CanRemoveSelected;
        }

        private void RefreshSlots()
        {
            _itemInfoPanel.gameObject.SetActive(_viewModel.SelectedItem != null);
            
            if (_content == null || _slotPrefab == null || _viewModel == null)
            {
                LogHandler.LogError<InventoryUI>("초기화가 완료되지 않았습니다.");
                return;
            }

            var items = _viewModel.GetItems();
            foreach (var item in items)
            {
                if (!_slotsDict.ContainsKey(item))
                    HandleItemAdded(item);
                
                _slotsDict[item].SetItem(item);
            }

            var toRemove = _slotsDict.Keys.Where(item => !items.Contains(item)).ToList();
            foreach (var item in toRemove)
                HandleItemRemoved(item);

            HandleEquippedWeaponChanged(_viewModel.EquippedWeapon);
        }
    }
}
