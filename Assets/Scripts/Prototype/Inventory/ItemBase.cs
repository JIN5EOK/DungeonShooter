using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 아이템의 기본 클래스
/// itemEffects는 초기화시 ItemData에서 복사해와 사용
/// </summary>
public abstract class ItemBase
{
    protected readonly ItemData _itemData;
    protected readonly List<ItemEffect> _itemEffects;

    public ItemData ItemData => _itemData;
    
    /// <summary>
    /// 아이템 효과 목록 (읽기 전용)
    /// </summary>
    public IReadOnlyList<ItemEffect> ItemEffects => _itemEffects;

    protected ItemBase(ItemData itemData)
    {
        if (itemData == null)
        {
            Debug.LogError("[ItemBase] ItemData가 null입니다.");
            _itemEffects = new List<ItemEffect>();
            return;
        }

        _itemData = itemData;
        
        // ItemData에서 효과를 복사해와 사용 (각 인스턴스마다 독립적)
        _itemEffects = itemData.ItemEffects != null 
            ? itemData.ItemEffects.ToList() 
            : new List<ItemEffect>();
    }

    /// <summary>
    /// 추가 효과 부여
    /// </summary>
    public void AddEffect(ItemEffect effect)
    {
        if (effect != null)
        {
            _itemEffects.Add(effect);
        }
    }

    /// <summary>
    /// 효과 제거
    /// </summary>
    public bool RemoveEffect(ItemEffect effect)
    {
        return _itemEffects.Remove(effect);
    }

    /// <summary>
    /// 모든 효과를 플레이어에 적용
    /// </summary>
    public void ApplyEffects(Player player)
    {
        if (player == null) return;

        foreach (var effect in _itemEffects)
        {
            if (effect != null)
            {
                effect.Apply(player);
            }
        }
    }

    /// <summary>
    /// 모든 효과를 플레이어에서 제거
    /// </summary>
    public void RemoveEffects(Player player)
    {
        if (player == null) return;

        foreach (var effect in _itemEffects)
        {
            if (effect != null)
            {
                effect.Remove(player);
            }
        }
    }
}

