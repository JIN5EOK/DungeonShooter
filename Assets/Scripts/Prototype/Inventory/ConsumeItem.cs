using System;

/// <summary>
/// 소모 아이템. 게임 중 단축키를 눌러 사용할 수 있습니다.
/// 개수는 Inventory에서 관리합니다.
/// </summary>
public class ConsumeItem : ItemBase, IUseable
{
    private readonly Action _onUse;

    /// <summary>
    /// 소모 아이템 생성자
    /// </summary>
    /// <param name="itemData">아이템 데이터</param>
    /// <param name="onUse">사용 시 실행할 액션 (선택 사항)</param>
    public ConsumeItem(ItemData itemData, Action onUse = null) : base(itemData)
    {
        _onUse = onUse;
    }

    public void Use()
    {
        _onUse?.Invoke();
    }
}

