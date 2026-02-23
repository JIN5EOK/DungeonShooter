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
        event Action<Item> OnWeaponEquipped;
        event Action<Item> OnWeaponUnequipped;
        event Action<Item> OnItemUse;

        IReadOnlyCollection<Item> Items { get; }
        Item EquippedWeapon { get; }

        bool AddItem(Item item);
        bool EquipItem(Item item);
        void Clear();
        void RemoveItem(Item item);
        void UseItem(Item item);
    }
}
