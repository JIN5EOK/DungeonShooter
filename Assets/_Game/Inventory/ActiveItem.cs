using System;

/// <summary>
/// 액티브 아이템. 게임 중 사용 버튼을 눌러 사용할 수 있습니다.
/// </summary>
public class ActiveItem : ItemBase, IUseable
{
    private readonly Action _onUse;

    /// <summary>
    /// 액티브 아이템 생성자
    /// </summary>
    /// <param name="itemData">아이템 데이터</param>
    /// <param name="onUse">사용 시 실행할 액션 (선택 사항)</param>
    public ActiveItem(ItemData itemData, Action onUse = null) : base(itemData)
    {
        _onUse = onUse;
    }

    public void Use()
    {
        _onUse?.Invoke();
    }
}

