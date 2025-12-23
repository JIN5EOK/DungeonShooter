using UnityEngine;
using DungeonShooter;

/// <summary>
/// 필드에 떨어져 있는 소모 아이템
/// </summary>
public class FieldConsumeItem : FieldItemBase
{
    [Header("소모 아이템 설정")]
    [Tooltip("아이템 개수")]
    [SerializeField, Min(1)] private int itemCount = 1;

    public override void Interact()
    {
        if (!CanInteract)
            return;

        if (_inventory == null)
        {
            Debug.LogWarning("[FieldConsumeItem] Inventory가 주입되지 않았습니다.");
            return;
        }

        AddToInventory();
    }

    protected override void AddToInventory()
    {
        if (itemData == null)
        {
            Debug.LogWarning("[FieldConsumeItem] 아이템 데이터가 설정되지 않았습니다.");
            return;
        }

        // 소모 아이템은 슬롯당 하나만 가능하므로, 개수만큼 슬롯에 추가
        bool added = true;
        for (int i = 0; i < itemCount; i++)
        {
            if (!_inventory.AddConsumeItem(itemData))
            {
                added = false;
                if (i == 0)
                {
                    // 첫 번째도 실패하면 완전 실패
                    break;
                }
                else
                {
                    // 일부만 추가된 경우
                    Debug.LogWarning($"[FieldConsumeItem] {i}개만 추가되었습니다. (요청: {itemCount}개)");
                    break;
                }
            }
        }
        if (added)
        {
            OnCollected();
        }
        else
        {
            Debug.LogWarning("[FieldConsumeItem] 인벤토리 슬롯이 꽉 찼습니다.");
            // TODO: 바닥에 뿌리기 로직 (강제 지급 이벤트의 경우)
        }
    }
}

