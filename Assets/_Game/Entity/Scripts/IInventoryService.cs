using System.Collections.Generic;

namespace DungeonShooter
{
    /// <summary>
    /// 엔티티의 소지 아이템·장착 관련 기능을 제공합니다.
    /// </summary>
    public interface IInventoryService
    {
        IReadOnlyCollection<Item> GetItems();
        Item GetEquippedWeapon();
        bool AddItem(Item item);
        bool RemoveItem(Item item);
    }
}
