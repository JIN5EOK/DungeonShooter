using System;
using System.Collections.Generic;
using VContainer;

namespace DungeonShooter
{
    /// <summary>
    /// 인벤토리 UI가 구독하는 뷰모델. 선택/버튼 가능 여부/명령을 담당한다.
    /// </summary>
    public interface IInventoryViewModel
    {
        event Action<Item> OnItemAdded;
        event Action<Item> OnItemRemoved;
        event Action<Item> OnItemStackChanged;
        event Action<Item> OnItemUse;
        event Action<Item> OnSelectionChanged;
        event Action<Item> OnEquippedWeaponChanged;

        Item SelectedItem { get; }
        Item EquippedWeapon { get; }
        bool CanEquipSelected { get; }
        bool CanUseSelected { get; }
        bool CanRemoveSelected { get; }

        void SelectItem(Item item);
        void EquipSelected();
        void UseSelected();
        void RemoveSelected();

        IReadOnlyCollection<Item> GetItems();
    }

    /// <summary>
    /// IInventory 상태를 구독해 인벤토리 뷰에 노출하고, 선택/장착·사용·제거 명령을 처리한다.
    /// </summary>
    public class InventoryViewModel : IInventoryViewModel
    {
        public event Action<Item> OnItemAdded;
        public event Action<Item> OnItemRemoved;
        public event Action<Item> OnItemStackChanged;
        public event Action<Item> OnItemUse;
        public event Action<Item> OnSelectionChanged;
        public event Action<Item> OnEquippedWeaponChanged;

        public Item SelectedItem => _selectedItem;
        public Item EquippedWeapon => _inventory.EquippedWeapon;

        public bool CanEquipSelected =>
            _selectedItem != null &&
            _selectedItem.ItemTableEntry.ItemType == ItemType.Weapon &&
            _selectedItem != _inventory.EquippedWeapon;

        public bool CanUseSelected =>
            _selectedItem != null &&
            _selectedItem.ItemTableEntry.ItemType == ItemType.Consume;

        public bool CanRemoveSelected =>
            _selectedItem != null &&
            _selectedItem != _inventory.EquippedWeapon;

        private readonly IInventory _inventory;
        private Item _selectedItem;

        [Inject]
        public InventoryViewModel(IInventory inventory)
        {
            _inventory = inventory;
            _inventory.OnItemAdded += InventoryOnItemAdded;
            _inventory.OnItemRemoved += InventoryOnItemRemoved;
            _inventory.OnItemStackChanged += InventoryOnItemStackChanged;
            _inventory.OnItemUse += InventoryOnItemUse;
            _inventory.OnWeaponEquipped += InventoryOnWeaponEquipped;
            _inventory.OnWeaponUnequipped += InventoryOnWeaponUnequipped;
        }

        private void InventoryOnItemUse(Item item)
        {
            OnItemUse?.Invoke(item);
        }

        private void InventoryOnItemStackChanged(Item item)
        {
            OnItemStackChanged?.Invoke(item);
        }

        private void InventoryOnItemAdded(Item item)
        {
            OnItemAdded?.Invoke(item);
        }

        private void InventoryOnItemRemoved(Item item)
        {
            if (_selectedItem == item)
            {
                _selectedItem = null;
                OnSelectionChanged?.Invoke(null);
            }

            OnItemRemoved?.Invoke(item);
        }

        private void InventoryOnWeaponEquipped(Item item)
        {
            OnEquippedWeaponChanged?.Invoke(item);
            OnSelectionChanged?.Invoke(_selectedItem);
        }

        private void InventoryOnWeaponUnequipped(Item item)
        {
            OnEquippedWeaponChanged?.Invoke(null);
            OnSelectionChanged?.Invoke(_selectedItem);
        }

        public void SelectItem(Item item)
        {
            _selectedItem = item;
            OnSelectionChanged?.Invoke(item);
        }

        public void EquipSelected()
        {
            if (_selectedItem == null)
                return;
            _inventory.EquipItem(_selectedItem);
        }

        public void UseSelected()
        {
            if (_selectedItem == null)
                return;
            _inventory.UseItem(_selectedItem);
        }

        public void RemoveSelected()
        {
            if (_selectedItem == null)
                return;
            _inventory.RemoveItem(_selectedItem);
        }

        public IReadOnlyCollection<Item> GetItems() => _inventory.GetItems();
    }
}
