using UnityEngine;
using UnityEngine.AddressableAssets;

/// <summary>
/// 아이템 데이터를 나타내는 ScriptableObject
/// </summary>
[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item Data")]
public class ItemData : ScriptableObject, IItemData
{
    [Header("기본 정보")]
    [SerializeField] private string itemName;
    [SerializeField, TextArea(3, 5)] private string description;
    [SerializeField] private AssetReferenceT<Sprite> icon;

    public string Name => itemName;
    public string Description => description;
    public AssetReferenceT<Sprite> Icon => icon;
}

