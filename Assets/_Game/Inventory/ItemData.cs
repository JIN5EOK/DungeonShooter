using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

/// <summary>
/// 아이템 타입
/// </summary>
public enum ItemType
{
    Passive,
    Active,
    Weapon
}

/// <summary>
/// 아이템 데이터를 나타내는 ScriptableObject
/// 원본 데이터 참조용, 여기있는 이펙트를 실제로 사용하지 않음
/// </summary>
[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("기본 정보")]
    [SerializeField] private string itemName;
    [SerializeField, TextArea(3, 5)] private string description;
    [SerializeField] private AssetReferenceT<Sprite> icon;
    [SerializeField] private ItemType itemType;

    [Header("효과 (템플릿, 참조용)")]
    [SerializeField] private List<ItemEffect> itemEffects = new List<ItemEffect>();

    public string Name => itemName;
    public string Description => description;
    public AssetReferenceT<Sprite> Icon => icon;
    public ItemType ItemType => itemType;
    
    /// <summary>
    /// 아이템 효과 목록 (읽기 전용, 참조용)
    /// 실제 사용은 ItemBase에서 복사된 효과를 사용
    /// </summary>
    public IReadOnlyList<ItemEffect> ItemEffects => itemEffects;
}

