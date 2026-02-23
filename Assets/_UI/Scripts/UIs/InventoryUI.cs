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
            _useButton.onClick.AddListener(() => _viewModel.UseSelected());
            _equipButton.onClick.AddListener(() => _viewModel.EquipSelected());
            _removeButton.onClick.AddListener(() => _viewModel.RemoveSelected());

            _viewModel.OnItemAdded += HandleItemAdded;
            _viewModel.OnItemRemoved += HandleItemRemoved;
            _viewModel.OnSelectionChanged += HandleSelectionChanged;
            _viewModel.OnEquippedWeaponChanged += HandleEquippedWeaponChanged;

            RefreshSlots();
            ApplyButtonState();
        }

        protected override void OnDestroy()
        {
            if (_viewModel != null)
            {
                _viewModel.OnItemAdded -= HandleItemAdded;
                _viewModel.OnItemRemoved -= HandleItemRemoved;
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
            if (selected != null)
                _itemInfoPanel.SetItem(selected);
            else
                _itemInfoPanel.Clear();

            ApplyButtonState();
        }

        private void HandleItemRemoved(Item item)
        {
            if (!_slotsDict.TryGetValue(item, out var slot))
                return;

            _slotsDict.Remove(item);
            slot.OnSlotClicked -= OnSlotClicked;
            Destroy(slot.gameObject);
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
            _itemInfoPanel.Clear();
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
            }

            var toRemove = _slotsDict.Keys.Where(item => !items.Contains(item)).ToList();
            foreach (var item in toRemove)
                HandleItemRemoved(item);

            HandleEquippedWeaponChanged(_viewModel.EquippedWeapon);
        }
    }
}
