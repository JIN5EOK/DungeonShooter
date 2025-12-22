/// <summary>
/// 아이템을 나타내는 기본 인터페이스
/// </summary>
public interface IItem
{
    /// <summary>
    /// 아이템 데이터
    /// </summary>
    IItemData ItemData { get; }
}

