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
    public class InventoryView : PopupUI
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
        private readonly Dictionary<IItemViewModel, InventorySlotUIElement> _slotsDict = new();

        [Inject]
        public void Construct(IInventoryViewModel viewModel)
        {
            _viewModel = viewModel;

            _closeButton.onClick.AddListener(_viewModel.Close);
            _useButton.onClick.AddListener(_viewModel.UseSelected);
            _equipButton.onClick.AddListener(_viewModel.EquipSelected);
            _removeButton.onClick.AddListener(_viewModel.RemoveSelected);

            _viewModel.OnSlotAdded += HandleSlotAdded;
            _viewModel.OnSlotRemoved += HandleSlotRemoved;
            _viewModel.OnSlotChanged += HandleSlotChanged;
            _viewModel.OnSlotUsed += HandleSlotUsed;
            _viewModel.OnSelectionChanged += HandleSelectionChanged;
            _viewModel.OnEquippedWeaponChanged += HandleEquippedWeaponChanged;

            _viewModel.OnOpened += Show;
            _viewModel.OnClosed += Hide;

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

        public override void Hide()
        {
            base.Hide();
        }

        protected override void OnDestroy()
        {
            if (_viewModel != null)
            {
                _viewModel.OnSlotAdded -= HandleSlotAdded;
                _viewModel.OnSlotRemoved -= HandleSlotRemoved;
                _viewModel.OnSlotChanged -= HandleSlotChanged;
                _viewModel.OnSlotUsed -= HandleSlotUsed;
                _viewModel.OnSelectionChanged -= HandleSelectionChanged;
                _viewModel.OnEquippedWeaponChanged -= HandleEquippedWeaponChanged;
                _viewModel.OnOpened -= Show;
                _viewModel.OnClosed -= Hide;
            }

            base.OnDestroy();
        }

        private void HandleSlotAdded(IItemViewModel slotVM)
        {
            var slot = Instantiate(_slotPrefab, _content, false);
            slot.SetSlot(slotVM);
            _slotsDict.Add(slotVM, slot);
            slot.OnSlotClicked += OnSlotClicked;
        }

        private void OnSlotClicked(IItemViewModel slotVM)
        {
            _viewModel.SelectSlot(slotVM);
            _itemInfoPanel.SetSlot(slotVM);
        }

        private void HandleSelectionChanged(IItemViewModel selected)
        {
            var isSelected = selected != null;

            _equipButton.gameObject.SetActive(isSelected);
            _removeButton.gameObject.SetActive(isSelected);
            _useButton.gameObject.SetActive(isSelected);
            _itemInfoPanel.gameObject.SetActive(isSelected);
            
            if (isSelected)
            {
                _itemInfoPanel.SetSlot(selected);
            }
            
            ApplyButtonState();
        }

        private void HandleSlotChanged(IItemViewModel slotVM)
        {
            if (_slotsDict.TryGetValue(slotVM, out var slot))
                slot.SetSlot(slotVM);
        }

        private void HandleSlotUsed(IItemViewModel slotVM)
        {
            if (_slotsDict.TryGetValue(slotVM, out var slot))
                slot.SetSlot(slotVM);

            RefreshSlots();
            ApplyButtonState();
            _viewModel.Close();
        }

        private void HandleSlotRemoved(IItemViewModel slotVM)
        {
            if (!_slotsDict.TryGetValue(slotVM, out var slot))
                return;

            if (_viewModel.SelectedSlot == slotVM)
                _viewModel.SelectSlot(null);

            _slotsDict.Remove(slotVM);
            slot.OnSlotClicked -= OnSlotClicked;
            Destroy(slot.gameObject);
            RefreshSlots();
        }

        private void HandleEquippedWeaponChanged(IItemViewModel equipped)
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
            _itemInfoPanel.gameObject.SetActive(_viewModel.SelectedSlot != null);
            
            if (_content == null || _slotPrefab == null || _viewModel == null)
            {
                LogHandler.LogError<InventoryView>("초기화가 완료되지 않았습니다.");
                return;
            }

            var slots = _viewModel.GetSlots();
            foreach (var slotVM in slots)
            {
                if (!_slotsDict.ContainsKey(slotVM))
                    HandleSlotAdded(slotVM);
                
                _slotsDict[slotVM].SetSlot(slotVM);
            }

            var toRemove = _slotsDict.Keys.Where(slotVM => !slots.Contains(slotVM)).ToList();
            foreach (var slotVM in toRemove)
                HandleSlotRemoved(slotVM);

            HandleEquippedWeaponChanged(_viewModel.EquippedWeaponSlot);
        }
    }
}
