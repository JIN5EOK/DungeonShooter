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
        event Action<IItemViewModel> OnSlotAdded;
        event Action<IItemViewModel> OnSlotRemoved;
        event Action<IItemViewModel> OnSlotChanged;
        event Action<IItemViewModel> OnSlotUsed;
        event Action<IItemViewModel> OnSelectionChanged;
        event Action<IItemViewModel> OnEquippedWeaponChanged;
        event Action OnOpened;
        event Action OnClosed;

        IItemViewModel SelectedSlot { get; }
        IItemViewModel EquippedWeaponSlot { get; }
        bool CanEquipSelected { get; }
        bool CanUseSelected { get; }
        bool CanRemoveSelected { get; }

        void SelectSlot(IItemViewModel slot);
        void EquipSelected();
        void UseSelected();
        void RemoveSelected();
        void Open();
        void Close();

        IReadOnlyCollection<IItemViewModel> GetSlots();
    }

    public class ItemViewModel : IItemViewModel
    {
        private readonly Item _item;

        public UnityEngine.Sprite Icon => _item.Icon;
        public int StackCount => _item.StackCount;
        public int MaxStackCount => _item.ItemTableEntry.MaxStackCount;
        public ItemType ItemType => _item.ItemTableEntry.ItemType;

        public string ItemNameText { get; }
        public string ItemDescriptionText { get; }
        public string ItemTypeText { get; }
        public string ItemEffectsText { get; }

        public Item GetItem() => _item;

        public ItemViewModel(Item item, IItemFormatter formatter)
        {
            _item = item;
            if (item != null && formatter != null)
            {
                ItemNameText = formatter.GetFormattedItemName(item.ItemTableEntry);
                ItemDescriptionText = formatter.GetFormattedItemDescription(item.ItemTableEntry);
                ItemTypeText = formatter.GetFormattedItemType(item.ItemTableEntry.ItemType);
                ItemEffectsText = formatter.GetFormattedItemEffects(item.ItemTableEntry);
            }
        }
    }

    public interface IItemViewModel
    {        
        public UnityEngine.Sprite Icon { get; }
        public int StackCount { get; }
        public int MaxStackCount { get; }
        public ItemType ItemType { get; }
        public Item GetItem();
        public string ItemNameText { get; }
        public string ItemDescriptionText { get; }
        public string ItemTypeText { get; }
        public string ItemEffectsText { get; }
    }

    /// <summary>
    /// IInventory 상태를 구독해 인벤토리 뷰에 노출하고, 선택/장착·사용·제거 명령을 처리한다.
    /// </summary>
    public class InventoryViewModel : IInventoryViewModel
    {
        public event Action<IItemViewModel> OnSlotAdded;
        public event Action<IItemViewModel> OnSlotRemoved;
        public event Action<IItemViewModel> OnSlotChanged;
        public event Action<IItemViewModel> OnSlotUsed;
        public event Action<IItemViewModel> OnSelectionChanged;
        public event Action<IItemViewModel> OnEquippedWeaponChanged;
        public event Action OnOpened;
        public event Action OnClosed;

        public IItemViewModel SelectedSlot => _selectedSlot;
        public IItemViewModel EquippedWeaponSlot => GetSlot(_inventory.EquippedWeapon);

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
        private readonly IPauseManager _pauseManager;
        private readonly IItemFormatter _itemFormatter;

        private IItemViewModel _selectedSlot;
        private readonly Dictionary<Item, IItemViewModel> _slotMap = new();

        [Inject]
        public InventoryViewModel(IInventory inventory, IPauseManager pauseManager, IItemFormatter itemFormatter)
        {
            _inventory = inventory;
            _pauseManager = pauseManager;
            _itemFormatter = itemFormatter;
            
            _inventory.OnItemAdded += InventoryOnItemAdded;
            _inventory.OnItemRemoved += InventoryOnItemRemoved;
            _inventory.OnItemStackChanged += InventoryOnItemStackChanged;
            _inventory.OnItemUse += InventoryOnItemUse;
            _inventory.OnWeaponEquipped += InventoryOnWeaponEquipped;
            _inventory.OnWeaponUnequipped += InventoryOnWeaponUnequipped;
            _inventory.OnOpened += InventoryOnOpened;
            _inventory.OnClosed += InventoryOnClosed;
        }

        private IItemViewModel GetOrCreateSlot(Item item)
        {
            if (item == null) return null;
            if (!_slotMap.TryGetValue(item, out var slot))
            {
                slot = new ItemViewModel(item, _itemFormatter);
                _slotMap[item] = slot;
            }
            return slot;
        }

        private IItemViewModel GetSlot(Item item)
        {
            if (item == null) return null;
            return _slotMap.GetValueOrDefault(item);
        }

        private void InventoryOnOpened()
        {
            _pauseManager.PauseRequest(this);
            OnOpened?.Invoke();
        }

        private void InventoryOnClosed()
        {
            _pauseManager.ResumeRequest(this);
            OnClosed?.Invoke();
        }

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

        public void SelectSlot(IItemViewModel slot)
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

        public IReadOnlyCollection<IItemViewModel> GetSlots()
        {
            var items = _inventory.GetItems();
            var slots = new List<IItemViewModel>(items.Count);
            foreach (var item in items)
            {
                slots.Add(GetOrCreateSlot(item));
            }
            return slots;
        }
    }
}
