using System;

/// <summary>
/// 소모 아이템. 게임 중 단축키를 눌러 사용할 수 있습니다.
/// 개수는 Inventory에서 관리합니다.
/// </summary>
public class ConsumeItem : IItem, IUseable
{
    private readonly IItemData _itemData;
    private readonly Action _onUse;

    public IItemData ItemData => _itemData;

    /// <summary>
    /// 소모 아이템 생성자
    /// </summary>
    /// <param name="itemData">아이템 데이터</param>
    /// <param name="onUse">사용 시 실행할 액션 (선택 사항)</param>
    public ConsumeItem(IItemData itemData, Action onUse = null)
    {
        _itemData = itemData;
        _onUse = onUse;
    }

    public void Use()
    {
        _onUse?.Invoke();
    }
}

