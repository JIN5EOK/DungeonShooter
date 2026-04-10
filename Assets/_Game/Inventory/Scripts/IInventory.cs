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

        /// <summary>스테이지에서 플레이어 엔티티 스폰 시 소유자 연동 (StageSceneInteractionMediator에서 연결)</summary>
        void OnPlayerObjectSpawned(PlayerObjectSpawnEvent playerObjectSpawnEvent);

        /// <summary>스테이지에서 플레이어 엔티티 제거 시 소유자 해제 (StageSceneInteractionMediator에서 연결)</summary>
        void OnPlayerObjectDespawned(PlayerObjectDestroyEvent playerObjectDestroyEvent);
    }
}
