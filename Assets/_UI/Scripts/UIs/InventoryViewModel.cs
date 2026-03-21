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
        event Action<InventorySlotViewModel> OnSlotAdded;
        event Action<InventorySlotViewModel> OnSlotRemoved;
        event Action<InventorySlotViewModel> OnSlotChanged;
        event Action<InventorySlotViewModel> OnSlotUsed;
        event Action<InventorySlotViewModel> OnSelectionChanged;
        event Action<InventorySlotViewModel> OnEquippedWeaponChanged;
        event Action OnOpened;
        event Action OnClosed;

        InventorySlotViewModel SelectedSlot { get; }
        InventorySlotViewModel EquippedWeaponSlot { get; }
        bool CanEquipSelected { get; }
        bool CanUseSelected { get; }
        bool CanRemoveSelected { get; }

        void SelectSlot(InventorySlotViewModel slot);
        void EquipSelected();
        void UseSelected();
        void RemoveSelected();
        void Open();
        void Close();

        IReadOnlyCollection<InventorySlotViewModel> GetSlots();
    }

    public class InventorySlotViewModel
    {
        private readonly Item _item;

        public ItemTableEntry TableEntry => _item.ItemTableEntry;
        public UnityEngine.Sprite Icon => _item.Icon;
        public int StackCount => _item.StackCount;
        public int MaxStackCount => _item.ItemTableEntry.MaxStackCount;
        public ItemType ItemType => _item.ItemTableEntry.ItemType;

        internal Item GetItem() => _item;

        public InventorySlotViewModel(Item item)
        {
            _item = item;
        }
    }
    
    /// <summary>
    /// IInventory 상태를 구독해 인벤토리 뷰에 노출하고, 선택/장착·사용·제거 명령을 처리한다.
    /// </summary>
    public class InventoryViewModel : IInventoryViewModel
    {
        public event Action<InventorySlotViewModel> OnSlotAdded;
        public event Action<InventorySlotViewModel> OnSlotRemoved;
        public event Action<InventorySlotViewModel> OnSlotChanged;
        public event Action<InventorySlotViewModel> OnSlotUsed;
        public event Action<InventorySlotViewModel> OnSelectionChanged;
        public event Action<InventorySlotViewModel> OnEquippedWeaponChanged;
        public event Action OnOpened;
        public event Action OnClosed;

        public InventorySlotViewModel SelectedSlot => _selectedSlot;
        public InventorySlotViewModel EquippedWeaponSlot => GetSlot(_inventory.EquippedWeapon);

        public bool CanEquipSelected =>
            _selectedSlot != null &&
            _selectedSlot.ItemType == ItemType.Weapon &&
            _selectedSlot.GetItem() != _inventory.EquippedWeapon;

        public bool CanUseSelected =>
            _selectedSlot != null &&
            _selectedSlot.ItemType == ItemType.Consume;

        public bool CanRemoveSelected =>
            _selectedSlot != null &&
            _selectedSlot.GetItem() != _inventory.EquippedWeapon;

        private readonly IInventory _inventory;
        private InventorySlotViewModel _selectedSlot;
        private readonly Dictionary<Item, InventorySlotViewModel> _slotMap = new();

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
            _inventory.OnOpened += InventoryOnOpened;
            _inventory.OnClosed += InventoryOnClosed;
        }

        private InventorySlotViewModel GetOrCreateSlot(Item item)
        {
            if (item == null) return null;
            if (!_slotMap.TryGetValue(item, out var slot))
            {
                slot = new InventorySlotViewModel(item);
                _slotMap[item] = slot;
            }
            return slot;
        }

        private InventorySlotViewModel GetSlot(Item item)
        {
            if (item == null) return null;
            return _slotMap.GetValueOrDefault(item);
        }

        private void InventoryOnOpened() => OnOpened?.Invoke();
        private void InventoryOnClosed() => OnClosed?.Invoke();

        private void InventoryOnItemUse(Item item)
        {
            OnSlotUsed?.Invoke(GetSlot(item));
        }

        private void InventoryOnItemStackChanged(Item item)
        {
            OnSlotChanged?.Invoke(GetSlot(item));
        }

        private void InventoryOnItemAdded(Item item)
        {
            OnSlotAdded?.Invoke(GetOrCreateSlot(item));
        }

        private void InventoryOnItemRemoved(Item item)
        {
            if (_slotMap.TryGetValue(item, out var slot))
            {
                if (_selectedSlot == slot)
                {
                    _selectedSlot = null;
                    OnSelectionChanged?.Invoke(null);
                }

                _slotMap.Remove(item);
                OnSlotRemoved?.Invoke(slot);
            }
        }

        private void InventoryOnWeaponEquipped(Item item)
        {
            OnEquippedWeaponChanged?.Invoke(GetSlot(item));
            OnSelectionChanged?.Invoke(_selectedSlot);
        }

        private void InventoryOnWeaponUnequipped(Item item)
        {
            OnEquippedWeaponChanged?.Invoke(null);
            OnSelectionChanged?.Invoke(_selectedSlot);
        }

        public void SelectSlot(InventorySlotViewModel slot)
        {
            _selectedSlot = slot;
            OnSelectionChanged?.Invoke(slot);
        }

        public void EquipSelected()
        {
            if (_selectedSlot == null)
                return;
            _inventory.EquipItem(_selectedSlot.GetItem());
        }

        public void UseSelected()
        {
            if (_selectedSlot == null)
                return;
            _inventory.UseItem(_selectedSlot.GetItem());
        }

        public void RemoveSelected()
        {
            if (_selectedSlot == null)
                return;
            _inventory.RemoveItem(_selectedSlot.GetItem());
        }

        public void Open() => _inventory.Open();
        public void Close() => _inventory.Close();

        public IReadOnlyCollection<InventorySlotViewModel> GetSlots()
        {
            var items = _inventory.GetItems();
            var slots = new List<InventorySlotViewModel>(items.Count);
            foreach (var item in items)
            {
                slots.Add(GetOrCreateSlot(item));
            }
            return slots;
        }
    }
}
