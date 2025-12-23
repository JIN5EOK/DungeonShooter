/// <summary>
/// 아이템 효과를 나타내는 클래스
/// </summary>
[System.Serializable]
public abstract class ItemEffect
{
    /// <summary>
    /// 효과를 플레이어에 적용
    /// </summary>
    public virtual void Apply(Player player)
    {
        // 하위 클래스에서 구현
    }

    /// <summary>
    /// 효과를 플레이어에서 제거
    /// </summary>
    public virtual void Remove(Player player)
    {
    }

    /// <summary>
    /// 효과 설명 문자열 반환
    /// </summary>
    public virtual string GetDescription()
    {
        return "";
    }
}

