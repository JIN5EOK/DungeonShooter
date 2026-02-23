using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace DungeonShooter
{
    /// <summary>
    /// 인벤토리 UI
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
        
        private IInventory _inventory;
        private readonly Dictionary<Item, InventorySlotUIElement> _slotsDict = new();

        private Item _selectedItem;

        [Inject]
        public void Construct(IInventory inventory)
        {
            _inventory = inventory;
            
            RefreshSlots();
            
            _closeButton.onClick.AddListener(Hide);
            _useButton.onClick.AddListener(OnClickUseButton);
            _equipButton.onClick.AddListener(OnClickEquipButton);
            _removeButton.onClick.AddListener(OnClickRemoveButton);
            
            _useButton.interactable = false;
            _removeButton.interactable = false;
            _equipButton.interactable = false;

            _inventory.OnItemAdded += HandleItemAdded;
            _inventory.OnItemRemoved += HandleItemRemoved;
        }

        public void OnClickEquipButton()
        {
            if (_inventory.EquippedWeapon != null)
            {
                _slotsDict[_inventory.EquippedWeapon].SetEquipped(false);
            }
                
            _inventory.EquipItem(_selectedItem);
            _slotsDict[_selectedItem].SetEquipped(true);
            SetSelectedItem(_selectedItem);
        }
        
        public void OnClickUseButton()
        {
            _inventory.UseItem(_selectedItem);
        }
        
        public void OnClickRemoveButton()
        {
            _inventory.RemoveItem(_selectedItem);
        }

        private void OnDestroy()
        {
            if (_inventory != null)
            {
                _inventory.OnItemAdded -= HandleItemAdded;
                _inventory.OnItemRemoved -= HandleItemRemoved;
            }
        }

        private void HandleItemAdded(Item item)
        {
            var slot = Instantiate(_slotPrefab, _content, false);
            slot.SetItem(item);
            _slotsDict.Add(item, slot);
            slot.OnSlotClicked += _itemInfoPanel.SetItem;
            slot.OnSlotClicked += SetSelectedItem;
        }

        private void SetSelectedItem(Item item)
        {
            _selectedItem = item;
            _equipButton.interactable = _selectedItem.ItemTableEntry.ItemType == ItemType.Weapon && _inventory.EquippedWeapon != _selectedItem;
            _useButton.interactable = _selectedItem.ItemTableEntry.ItemType == ItemType.Consume;
            _removeButton.interactable = _inventory.EquippedWeapon != _selectedItem;
        }
        
        private void HandleItemRemoved(Item item)
        {
            if (_slotsDict.ContainsKey(item))
            {
                var slot = _slotsDict[item];
                _slotsDict.Remove(item);
                Destroy(slot.gameObject);
            }
        }
        
        private void RefreshSlots()
        {
            _itemInfoPanel.Clear();
            if (_content == null || _slotPrefab == null || _inventory == null)
            {
                LogHandler.LogError<InventoryUI>("초기화가 완료되지 않았습니다.");
                return;
            }
                

            var items = _inventory.GetItems();
            foreach (var item in items)
            {
                if (!_slotsDict.ContainsKey(item))
                {
                    HandleItemAdded(item);
                }
            }
            
            foreach (var itemSlot in _slotsDict.Values)
            {
                // TODO: 리스트 기반 조회라 성능 낮음, 개선필요
                if (!items.Contains(itemSlot.BoundItem))
                {
                    HandleItemRemoved(itemSlot.BoundItem);
                }
                
                itemSlot.SetEquipped(_inventory.EquippedWeapon == itemSlot.BoundItem);
            }
        }
    }
}
