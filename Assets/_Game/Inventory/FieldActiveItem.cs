using UnityEngine;
using DungeonShooter;

/// <summary>
/// 필드에 떨어져 있는 액티브 아이템
/// </summary>
public class FieldActiveItem : FieldItemBase
{
    public override void Interact()
    {
        if (!CanInteract)
            return;

        if (_inventory == null)
        {
            Debug.LogWarning("[FieldActiveItem] Inventory가 주입되지 않았습니다.");
            return;
        }

        AddToInventory();
    }

    protected override void AddToInventory()
    {
        if (itemData == null)
        {
            Debug.LogWarning("[FieldActiveItem] 아이템 데이터가 설정되지 않았습니다.");
            return;
        }

        var activeItem = new ActiveItem(itemData);
        _inventory.AddActiveItem(activeItem);
        OnCollected();
    }
}

