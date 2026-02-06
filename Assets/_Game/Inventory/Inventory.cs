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
        
        private readonly List<Item> _items = new List<Item>();
        private Item _equippedWeapon;
        private EntityBase _owner;

        // 인벤토리 아이템 목록 (읽기 전용)
        public IReadOnlyList<Item> Items => _items;

        // 현재 장착된 무기
        public Item EquippedWeapon => _equippedWeapon;


        /// <summary>
        /// 인벤토리 소유자를 설정합니다.
        /// </summary>
        /// <param name="owner">소유자 Entity</param>
        public void SetOwner(EntityBase owner)
        {
            _owner = owner;
        }

        /// <summary>
        /// 아이템 추가
        /// </summary>
        /// <param name="item">추가할 아이템</param>
        public async UniTask AddItem(Item item)
        {
            if (item == null)
            {
                LogHandler.LogWarning<Inventory>("아이템이 null입니다.");
                return;
            }

            // 스택 가능한 아이템인지 확인
            var existingItem = FindStackableItem(item.ItemTableEntry.Id);
            if (existingItem != null && existingItem.CanAddStack(item.StackCount))
            {
                // 기존 아이템에 스택 추가
                var remaining = item.StackCount - existingItem.AddStack(item.StackCount);
                if (remaining > 0)
                {
                    // 받은 아이템의 스택 개수를 남은 개수로 설정하고 사용
                    item.StackCount = remaining;
                    _items.Add(item);
                    
                    // 패시브 스킬 활성화
                    if (item.PassiveSkill != null)
                    {
                        item.ActivatePassiveSkill(_owner);
                    }

                    // Passive: 인벤토리에 들어오면 스탯 보너스 적용
                    if (item.ItemTableEntry.ItemType == ItemType.Passive)
                    {
                        ApplyItemStatBonus(item);
                    }
                }
            }
            else
            {
                // 새 아이템 추가 (이미 초기화되어 있다고 가정)
                // 초기화되지 않은 경우를 대비해 확인
                if (!item.IsInitialized())
                {
                    await item.InitializeAsync();
                }
                
                _items.Add(item);
                
                // 패시브 스킬 활성화
                if (item.PassiveSkill != null)
                {
                    item.ActivatePassiveSkill(_owner);
                }

                // Passive: 인벤토리에 들어오면 스탯 보너스 적용
                if (item.ItemTableEntry.ItemType == ItemType.Passive)
                {
                    ApplyItemStatBonus(item);
                }
            }

            OnItemAdded?.Invoke(item);
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
                _equippedWeapon.DeactivateEquipSkill(_owner);
                RemoveItemStatBonus(_equippedWeapon);
                OnWeaponUnequipped?.Invoke(_equippedWeapon);
            }

            // 새 무기 장착
            _equippedWeapon = item;
            if (item.EquipSkill != null)
            {
                item.ActivateEquipSkill(_owner);
            }

            // Weapon: 장착하면 스탯 보너스 적용
            ApplyItemStatBonus(item);

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
                item.DeactivateEquipSkill(_owner);
                RemoveItemStatBonus(item);
                _equippedWeapon = null;
            }

            // Passive: 인벤토리에서 나가면 스탯 보너스 제거
            if (item.ItemTableEntry.ItemType == ItemType.Passive)
            {
                RemoveItemStatBonus(item);
            }

            // 패시브 스킬 비활성화
            item.DeactivatePassiveSkill(_owner);

            // 스킬 리소스 정리
            item.DisposeSkills();

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

        private void ApplyItemStatBonus(Item item)
        {
            if (_owner == null) return;
            var comp = _owner.GetComponent<EntityStatsComponent>();
            if (comp == null) return;
            comp.ApplyStatBonus(item, StatBonus.From(item.ItemTableEntry));
        }

        private void RemoveItemStatBonus(Item item)
        {
            if (_owner == null) return;
            var comp = _owner.GetComponent<EntityStatsComponent>();
            if (comp == null) return;
            comp.RemoveStatBonus(item);
        }


        /// <summary>
        /// 소비 아이템을 사용합니다.
        /// </summary>
        /// <param name="item">사용할 아이템</param>
        /// <param name="target">스킬을 적용할 Entity</param>
        /// <returns>사용 성공 여부</returns>
        public async UniTask<bool> UseItem(Item item)
        {
            if (item == null)
            {
                LogHandler.LogWarning<Inventory>("아이템이 null입니다.");
                return false;
            }

            if (item.ItemTableEntry.ItemType != ItemType.Consume)
            {
                LogHandler.LogWarning<Inventory>("소비 아이템만 사용할 수 있습니다.");
                return false;
            }

            if (!_items.Contains(item))
            {
                LogHandler.LogWarning<Inventory>("인벤토리에 없는 아이템입니다.");
                return false;
            }

            // 스킬 실행 (시전자 = 인벤토리 소유자)
            var success = await item.ExecuteUseSkill(_owner);

            // 사용 성공 시 아이템 제거
            if (success)
            {
                item.StackCount--;
                if (item.StackCount <= 0)
                {
                    RemoveItem(item);
                }
            }

            return success;
        }

    }
}
