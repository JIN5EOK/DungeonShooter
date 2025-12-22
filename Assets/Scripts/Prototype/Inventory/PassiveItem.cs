/// <summary>
/// 패시브 아이템. 소지하면 지속적으로 효과가 부여됩니다.
/// </summary>
public class PassiveItem : IItem
{
    private readonly IItemData _itemData;

    public IItemData ItemData => _itemData;

    public PassiveItem(IItemData itemData)
    {
        _itemData = itemData;
    }
}

