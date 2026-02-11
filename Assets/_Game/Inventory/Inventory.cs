using System;
using System.Collections.Generic;
using System.Linq;
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
        public event Action<Item> OnItemUse;
        public IReadOnlyCollection<Item> Items => _items;
        public Item EquippedWeapon => _equippedWeapon;
        private readonly HashSet<Item> _items = new HashSet<Item>();
        private Item _equippedWeapon;
        private EntityStatGroup _statGroup;
        private EntitySkillGroup _skillGroup;
        private EntityBase _ownerEntity;

        public bool IsContainsItem(Item item) => item != null && _items.Contains(item);

        /// <summary>
        /// 플레이어 인스턴스 바인딩
        /// </summary>
        public void BindPlayerInstance(EntityBase entity)
        {
            UnbindOwner(_ownerEntity);
            _ownerEntity = entity;
            if (entity != null)
                entity.OnDestroyed += UnbindOwner;
        }

        private void UnbindOwner(EntityBase entity)
        {
            if (entity == null || _ownerEntity != entity)
                return;

            _ownerEntity = null;
            entity.OnDestroyed -= UnbindOwner;
        }

        /// <summary>
        /// 스탯 적용 대상 그룹을 설정합니다. (EntityBase가 없어도 스탯 보너스 적용 가능)
        /// </summary>
        public void SetStatGroup(EntityStatGroup statGroup)
        {
            _statGroup = statGroup;
        }

        /// <summary>
        /// 스킬 그룹을 설정합니다.
        /// </summary>
        public void SetSkillGroup(EntitySkillGroup skillGroup)
        {
            _skillGroup = skillGroup;
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

                    if (item.PassiveSkill != null)
                        _skillGroup?.Regist(item.PassiveSkill);

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

                if (item.PassiveSkill != null)
                    _skillGroup?.Regist(item.PassiveSkill);

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
                if (_equippedWeapon.EquipSkill != null)
                    _skillGroup?.Unregist(_equippedWeapon.EquipSkill, false);
                RemoveItemStatBonus(_equippedWeapon);
                OnWeaponUnequipped?.Invoke(_equippedWeapon);
            }

            // 새 무기 장착
            _equippedWeapon = item;
            if (item.EquipSkill != null)
                _skillGroup?.Regist(item.EquipSkill);

            // Weapon: 장착하면 스탯 보너스 적용
            ApplyItemStatBonus(item);

            OnWeaponEquipped?.Invoke(item);
            return UniTask.CompletedTask;
        }

        /// <summary>
        /// 소지·장착 중인 모든 아이템을 제거합니다.
        /// </summary>
        public void Clear()
        {
            var copy = _items.ToList();
            foreach (var item in copy)
                RemoveItem(item);
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
                if (item.EquipSkill != null)
                    _skillGroup?.Unregist(item.EquipSkill, false);
                RemoveItemStatBonus(item);
                _equippedWeapon = null;
            }

            if (item.ItemTableEntry.ItemType == ItemType.Passive)
                RemoveItemStatBonus(item);

            if (item.PassiveSkill != null)
                _skillGroup?.Unregist(item.PassiveSkill, false);

            item.DisposeSkills();

            _items.Remove(item);
            OnItemRemoved?.Invoke(item);
        }

        /// <summary>
        /// 스택 가능한 아이템 찾기
        /// </summary>
        private Item FindStackableItem(int itemEntryId)
        {
            // TODO: 소지 아이템이 매우매우 많아지게 된다면 로직 개선 필요
            return _items.FirstOrDefault(i => i.ItemTableEntry.Id == itemEntryId &&
                                              i.StackCount < i.ItemTableEntry.MaxStackCount);
        }

        private void ApplyItemStatBonus(Item item)
        {
            if (_statGroup == null) return;
            _statGroup.ApplyStatBonus(item, StatBonus.From(item.ItemTableEntry));
        }

        private void RemoveItemStatBonus(Item item)
        {
            if (_statGroup == null) return;
            _statGroup.RemoveStatBonus(item);
        }


        /// <summary> 소비 아이템 사용후 이벤트 실행, 갯수 차감 /// </summary>
        public void UseItem(Item item)
        {
            if (item == null)
            {
                LogHandler.LogWarning<Inventory>("아이템이 null입니다.");
                return;
            }

            if (item.ItemTableEntry.ItemType != ItemType.Consume)
            {
                LogHandler.LogWarning<Inventory>("소비 아이템만 사용할 수 있습니다.");
                return;
            }

            if (!_items.Contains(item))
            {
                LogHandler.LogWarning<Inventory>("인벤토리에 없는 아이템입니다.");
                return;
            }

            item.StackCount--;
            if (item.StackCount <= 0)
                RemoveItem(item);

            if (_ownerEntity != null)
                item.ExecuteUseSkill(_ownerEntity).Forget();

            OnItemUse?.Invoke(item);
        }
    }
}
