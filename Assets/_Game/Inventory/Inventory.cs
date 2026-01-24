using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using VContainer;

namespace DungeonShooter
{
    /// <summary>
    /// 아이템 장착/소지 관리 인벤토리
    /// </summary>
    public class Inventory
    {
        public event Action<Item> OnItemAdded;
        public event Action<Item> OnItemRemoved;
        public event Action<Item> OnWeaponEquipped;
        public event Action<Item> OnWeaponUnequipped;
        
        // 스킬 등록 이벤트 (skillEntryId)
        public event Action<int> OnItemSkillRegister;
        
        // 스킬 해제 이벤트 (skillEntryId)
        public event Action<int> OnItemSkillUnregister;
        
        private readonly List<Item> _items = new List<Item>();
        private Item _equippedWeapon;
        private readonly ITableRepository _tableRepository;

        // 인벤토리 아이템 목록 (읽기 전용)
        public IReadOnlyList<Item> Items => _items;

        // 현재 장착된 무기
        public Item EquippedWeapon => _equippedWeapon;

        [Inject]
        public Inventory(ITableRepository tableRepository)
        {
            _tableRepository = tableRepository;
        }

        /// <summary>
        /// 아이템 추가
        /// </summary>
        /// <param name="item">추가할 아이템</param>
        public UniTask AddItem(Item item)
        {
            if (item == null)
            {
                LogHandler.LogWarning<Inventory>("아이템이 null입니다.");
                return UniTask.CompletedTask;
            }

            // 스택 가능한 아이템인지 확인
            var existingItem = FindStackableItem(item.ItemTableEntry.Id);
            if (existingItem != null && existingItem.CanAddStack(item.StackCount))
            {
                // 기존 아이템에 스택 추가
                var remaining = item.StackCount - existingItem.AddStack(item.StackCount);
                if (remaining > 0)
                {
                    // 스택이 넘치면 새 아이템 생성
                    var newItem = new Item(item.ItemTableEntry, remaining);
                    _items.Add(newItem);
                    NotifySkillRegistration(newItem);
                }
            }
            else
            {
                // 새 아이템 추가
                _items.Add(item);
                NotifySkillRegistration(item);
            }

            OnItemAdded?.Invoke(item);
            return UniTask.CompletedTask;
        }

        /// <summary>
        /// 아이템 장착
        /// </summary>
        /// <param name="item">장착할 아이템</param>
        public UniTask EquipItem(Item item)
        {
            if (item == null)
            {
                LogHandler.LogWarning<Inventory>("아이템이 null입니다.");
                return UniTask.CompletedTask;
            }

            // 무기 타입인지 확인
            if (item.ItemTableEntry.ItemType != ItemType.Weapon)
            {
                LogHandler.LogWarning<Inventory>("무기 타입의 아이템만 장착할 수 있습니다.");
                return UniTask.CompletedTask;
            }

            // 인벤토리에 있는지 확인
            if (!_items.Contains(item))
            {
                LogHandler.LogWarning<Inventory>("인벤토리에 없는 아이템입니다.");
                return UniTask.CompletedTask;
            }

            // 기존 무기 해제
            if (_equippedWeapon != null)
            {
                NotifySkillUnregistration(_equippedWeapon, true);
                OnWeaponUnequipped?.Invoke(_equippedWeapon);
            }

            // 새 무기 장착
            _equippedWeapon = item;
            NotifySkillRegistration(item, true);

            OnWeaponEquipped?.Invoke(item);
            return UniTask.CompletedTask;
        }

        /// <summary>
        /// 아이템 제거
        /// </summary>
        /// <param name="item">제거할 아이템</param>
        public void RemoveItem(Item item)
        {
            if (item == null)
            {
                return;
            }

            // 장착된 아이템인지 확인
            if (item == _equippedWeapon)
            {
                NotifySkillUnregistration(item, isEquipped: true);
                _equippedWeapon = null;
            }
            else
            {
                NotifySkillUnregistration(item, isEquipped: false);
            }

            _items.Remove(item);
            OnItemRemoved?.Invoke(item);
        }

        /// <summary>
        /// 스택 가능한 아이템 찾기
        /// </summary>
        private Item FindStackableItem(int itemEntryId)
        {
            return _items.Find(i => i.ItemTableEntry.Id == itemEntryId && 
                                   i.StackCount < i.ItemTableEntry.MaxStackCount);
        }

        /// <summary>
        /// 아이템의 스킬 등록 이벤트 발생
        /// </summary>
        private void NotifySkillRegistration(Item item, bool isEquipped = false)
        {
            var entry = item.ItemTableEntry;

            // PassiveEffect 등록 (인벤토리에 들어온 모든 아이템)
            if (entry.PassiveEffect > 0)
            {
                OnItemSkillRegister?.Invoke(entry.PassiveEffect);
            }

            // EquipEffect, ActiveEffect 등록 (무기 장착 시)
            if (isEquipped && entry.ItemType == ItemType.Weapon)
            {
                if (entry.EquipEffect > 0)
                {
                    OnItemSkillRegister?.Invoke(entry.EquipEffect);
                }

                if (entry.ActiveEffect > 0)
                {
                    OnItemSkillRegister?.Invoke(entry.ActiveEffect);
                }
            }
        }

        /// <summary>
        /// 아이템의 스킬 해제 이벤트 발생
        /// </summary>
        private void NotifySkillUnregistration(Item item, bool isEquipped = false)
        {
            var entry = item.ItemTableEntry;

            // PassiveEffect 해제
            if (entry.PassiveEffect > 0)
            {
                OnItemSkillUnregister?.Invoke(entry.PassiveEffect);
            }

            // EquipEffect, ActiveEffect 해제 (무기 해제 시)
            if (isEquipped && entry.ItemType == ItemType.Weapon)
            {
                if (entry.EquipEffect > 0)
                {
                    OnItemSkillUnregister?.Invoke(entry.EquipEffect);
                }

                if (entry.ActiveEffect > 0)
                {
                    OnItemSkillUnregister?.Invoke(entry.ActiveEffect);
                }
            }
        }

    }
}
