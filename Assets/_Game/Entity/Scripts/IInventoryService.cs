using System.Collections.Generic;

namespace DungeonShooter
{
    /// <summary>
    /// 엔티티의 소지 아이템·장착 관련 기능을 제공합니다.
    /// </summary>
    public interface IInventoryService
    {
        public IReadOnlyCollection<Item> GetItems();
        public Item GetEquippedWeapon();
        public bool AddItem(Item item);
        public bool RemoveItem(Item item);
    }
}
