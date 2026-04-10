using System;
using System.Collections.Generic;

namespace DungeonShooter
{
    /// <summary>
    /// 아이템 장착/소지 관리 인벤토리 인터페이스
    /// </summary>
    public interface IInventory : IDisposable
    {
        event Action<Item> OnItemAdded;
        event Action<Item> OnItemRemoved;
        event Action<Item> OnItemStackChanged;
        event Action<Item> OnWeaponEquipped;
        event Action<Item> OnWeaponUnequipped;
        event Action<Item> OnItemUse;
        event Action OnOpened;
        event Action OnClosed;

        Item EquippedWeapon { get; }

        IReadOnlyCollection<Item> GetItems();

        bool AddItem(Item item);
        bool EquipItem(Item item);
        void Clear();
        void RemoveItem(Item item);
        void UseItem(Item item);
        void Open();
        void Close();

        /// <summary>소비 아이템 사용 시 스킬을 적용할 대상 엔티티를 지정합니다.</summary>
        void BindItemUserEntity(EntityBase entity);

        /// <summary><see cref="BindItemUserEntity"/>로 연결한 엔티티를 해제합니다.</summary>
        void UnbindItemUserEntity();
    }
}
