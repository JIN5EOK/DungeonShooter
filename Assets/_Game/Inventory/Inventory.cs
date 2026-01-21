using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DungeonShooter
{
    /// <summary>
    /// 플레이어의 인벤토리를 관리하는 클래스
    /// </summary>
    public class Inventory
    {
        private readonly List<ItemBase> _items = new(); // 통합 인벤토리 리스트
        private WeaponItem _equippedWeapon;
        private ActiveItem _equippedActive;

        /// <summary>
        /// 인벤토리 아이템 목록 (읽기 전용)
        /// </summary>
        public IReadOnlyList<ItemBase> Items => _items;

        /// <summary>
        /// 현재 장착된 무기
        /// </summary>
        public WeaponItem EquippedWeapon => _equippedWeapon;

        /// <summary>
        /// 현재 장착된 액티브 아이템
        /// </summary>
        public ActiveItem EquippedActive => _equippedActive;

        /// <summary>
        /// 아이템 추가
        /// </summary>
        public void AddItem(ItemBase item)
        {
            if (item == null)
            {
                Debug.LogWarning($"[{nameof(Inventory)}] 아이템이 null입니다.");
                return;
            }

            _items.Add(item);
            OnItemAdded?.Invoke(item);
        }

        /// <summary>
        /// 아이템 제거
        /// </summary>
        public bool RemoveItem(ItemBase item)
        {
            if (item == null)
                return false;

            // 장착된 아이템인지 확인
            if (item == _equippedWeapon || item == _equippedActive)
            {
                Debug.LogWarning($"[{nameof(Inventory)}] 장착된 아이템은 제거할 수 없습니다. 먼저 해제하세요.");
                return false;
            }

            var removed = _items.Remove(item);
            if (removed)
            {
                OnItemRemoved?.Invoke(item);
            }
            return removed;
        }

        /// <summary>
        /// 무기 장착 (기존 무기가 있으면 교체)
        /// </summary>
        public bool EquipWeapon(WeaponItem weapon)
        {
            if (weapon == null)
            {
                Debug.LogWarning($"[{nameof(Inventory)}] 무기가 null입니다.");
                return false;
            }

            // 인벤토리에 있는지 확인
            if (!_items.Contains(weapon))
            {
                Debug.LogWarning($"[{nameof(Inventory)}] 인벤토리에 없는 무기입니다.");
                return false;
            }

            var oldWeapon = _equippedWeapon;
            _equippedWeapon = weapon;
            weapon.Equip();

            if (oldWeapon != null)
            {
                OnWeaponUnequipped?.Invoke(oldWeapon);
            }

            OnWeaponEquipped?.Invoke(weapon);
            return true;
        }

        /// <summary>
        /// 액티브 아이템 장착 (기존 액티브 아이템이 있으면 교체)
        /// </summary>
        public bool EquipActive(ActiveItem activeItem)
        {
            if (activeItem == null)
            {
                Debug.LogWarning($"[{nameof(Inventory)}] 액티브 아이템이 null입니다.");
                return false;
            }

            // 인벤토리에 있는지 확인
            if (!_items.Contains(activeItem))
            {
                Debug.LogWarning($"[{nameof(Inventory)}] 인벤토리에 없는 액티브 아이템입니다.");
                return false;
            }

            var oldActive = _equippedActive;
            _equippedActive = activeItem;
            activeItem.Equip();

            if (oldActive != null)
            {
                OnActiveUnequipped?.Invoke(oldActive);
            }

            OnActiveEquipped?.Invoke(activeItem);
            return true;
        }

        /// <summary>
        /// 특정 아이템 데이터를 소지하고 있는지 확인
        /// </summary>
        public int GetItemCount(ItemData itemData)
        {
            if (itemData == null)
                return 0;

            return _items.Count(i => i.ItemData == itemData);
        }

        /// <summary>
        /// 특정 아이템 데이터를 소지하고 있는지 확인
        /// </summary>
        public bool HasItem(ItemData itemData)
        {
            return GetItemCount(itemData) > 0;
        }

        /// <summary>
        /// 특정 타입의 아이템 목록 조회
        /// </summary>
        public List<T> GetItemsOfType<T>() where T : ItemBase
        {
            return _items.OfType<T>().ToList();
        }

        // 이벤트들
        public event Action<ItemBase> OnItemAdded;
        public event Action<ItemBase> OnItemRemoved;
        public event Action<WeaponItem> OnWeaponEquipped;
        public event Action<WeaponItem> OnWeaponUnequipped;
        public event Action<ActiveItem> OnActiveEquipped;
        public event Action<ActiveItem> OnActiveUnequipped;
    }
}
