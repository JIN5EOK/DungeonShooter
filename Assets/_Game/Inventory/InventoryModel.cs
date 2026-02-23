using System;
using System.Collections.Generic;
using System.Linq;

namespace DungeonShooter
{
    /// <summary>
    /// 인벤토리 컨테이너 및 아이템 추가/제거/장착 로직을 담당하는 모델.
    /// 데이터 변경 시 해당 이벤트를 발생시킨다. 스킬/스탯 등 서비스 연동은 하지 않는다.
    /// </summary>
    public class InventoryModel
    {
        internal IReadOnlyCollection<Item> Items => _items;
        internal Item EquippedWeapon => _equippedWeapon;

        public event Action<Item> OnItemAdded;
        public event Action<Item> OnItemRemoved;
        public event Action<Item> OnWeaponEquipped;
        public event Action<Item> OnWeaponUnequipped;

        private readonly HashSet<Item> _items = new HashSet<Item>();
        private Item _equippedWeapon;

        /// <summary>
        /// 아이템을 컨테이너에 추가한다. 스택 가능하면 기존 아이템에 합친다.
        /// </summary>
        /// <param name="item">추가할 아이템</param>
        /// <returns>추가 또는 스택 반영 여부</returns>
        public bool AddItem(Item item)
        {
            if (item == null)
                return false;

            var existingItem = FindStackableItem(item.ItemTableEntry.Id);
            if (existingItem != null && existingItem.CanAddStack(item.StackCount))
            {
                var remaining = item.StackCount - existingItem.AddStack(item.StackCount);
                if (remaining > 0)
                    item.StackCount = remaining;
                return true;
            }

            _items.Add(item);
            OnItemAdded?.Invoke(item);
            return true;
        }

        /// <summary>
        /// 컨테이너에서 아이템을 제거한다. 장착 중이었으면 해제한다.
        /// </summary>
        public void RemoveItem(Item item)
        {
            if (item == null)
                return;

            if (item == _equippedWeapon)
            {
                _equippedWeapon = null;
                OnWeaponUnequipped?.Invoke(item);
            }

            _items.Remove(item);
            OnItemRemoved?.Invoke(item);
        }

        /// <summary>
        /// 무기 아이템을 장착한다. 검증만 수행하며 스킬/스탯은 건드리지 않는다.
        /// </summary>
        /// <returns>장착 성공 여부</returns>
        public bool EquipItem(Item item)
        {
            if (item == null)
                return false;
            if (item.ItemTableEntry.ItemType != ItemType.Weapon)
                return false;
            if (!_items.Contains(item))
                return false;

            var previousWeapon = _equippedWeapon;
            if (previousWeapon != null)
                OnWeaponUnequipped?.Invoke(previousWeapon);

            _equippedWeapon = item;
            OnWeaponEquipped?.Invoke(item);
            return true;
        }

        /// <summary>
        /// 소지·장착 중인 모든 아이템을 컨테이너에서 제거한다.
        /// </summary>
        public void Clear()
        {
            _equippedWeapon = null;
            _items.Clear();
        }

        public bool Contains(Item item) => item != null && _items.Contains(item);

        private Item FindStackableItem(int itemEntryId)
        {
            return _items.FirstOrDefault(i => i.ItemTableEntry.Id == itemEntryId &&
                                              i.StackCount < i.ItemTableEntry.MaxStackCount);
        }
    }
}
