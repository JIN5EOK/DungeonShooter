using UnityEngine;
using UnityEngine.AddressableAssets;

/// <summary>
/// 아이템의 기본 데이터를 나타내는 인터페이스
/// </summary>
public interface IItemData
{
    /// <summary>
    /// 아이템 이름
    /// </summary>
    string Name { get; }
    
    /// <summary>
    /// 아이템 설명
    /// </summary>
    string Description { get; }
    
    /// <summary>
    /// 아이템 아이콘 (Addressable Asset)
    /// </summary>
    AssetReferenceT<Sprite> Icon { get; }
}

