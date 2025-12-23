using UnityEngine;
using DungeonShooter;

/// <summary>
/// 필드에 떨어져 있는 패시브 아이템
/// </summary>
public class FieldPassiveItem : FieldItemBase
{
    public override void Interact()
    {
        if (!CanInteract)
            return;

        if (_inventory == null)
        {
            Debug.LogWarning("[FieldPassiveItem] Inventory가 주입되지 않았습니다.");
            return;
        }

        AddToInventory();
    }

    protected override void AddToInventory()
    {
        if (itemData == null)
        {
            Debug.LogWarning("[FieldPassiveItem] 아이템 데이터가 설정되지 않았습니다.");
            return;
        }

        var passiveItem = new PassiveItem(itemData);
        _inventory.AddPassiveItem(passiveItem);
        OnCollected();
    }
}

