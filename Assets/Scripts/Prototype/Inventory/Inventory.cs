using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 플레이어의 인벤토리를 관리하는 클래스
/// </summary>
public class Inventory
{
    private readonly List<PassiveItem> _passiveItems = new();
    private readonly List<ConsumeItem> _consumeItemSlots = new(); // 슬롯용 (각 슬롯은 하나의 아이템만)
    private ActiveItem _activeItem;
    
    /// <summary>
    /// 소모 아이템 슬롯 개수 (기본 2칸, 패시브 아이템으로 확장 가능)
    /// </summary>
    public int ConsumeItemSlotCount { get; private set; } = 2;

    /// <summary>
    /// 패시브 아이템 목록 (읽기 전용)
    /// </summary>
    public IReadOnlyList<PassiveItem> PassiveItems => _passiveItems;

    /// <summary>
    /// 소모 아이템 슬롯 목록 (읽기 전용)
    /// </summary>
    public IReadOnlyList<ConsumeItem> ConsumeItemSlots => _consumeItemSlots;

    /// <summary>
    /// 현재 액티브 아이템
    /// </summary>
    public ActiveItem ActiveItem => _activeItem;

    /// <summary>
    /// 패시브 아이템 추가
    /// </summary>
    public void AddPassiveItem(PassiveItem item)
    {
        if (item == null)
        {
            Debug.LogWarning("[Inventory] 패시브 아이템이 null입니다.");
            return;
        }

        _passiveItems.Add(item);
        OnPassiveItemAdded?.Invoke(item);
    }

    /// <summary>
    /// 패시브 아이템 제거
    /// </summary>
    public bool RemovePassiveItem(PassiveItem item)
    {
        if (item == null)
            return false;

        bool removed = _passiveItems.Remove(item);
        if (removed)
        {
            OnPassiveItemRemoved?.Invoke(item);
        }
        return removed;
    }

    /// <summary>
    /// 소모 아이템 추가 (슬롯당 하나만, 스택 불가)
    /// </summary>
    /// <param name="itemData">아이템 데이터</param>
    /// <returns>추가 성공 여부</returns>
    public bool AddConsumeItem(ItemData itemData)
    {
        if (itemData == null)
        {
            Debug.LogWarning("[Inventory] 아이템 데이터가 null입니다.");
            return false;
        }

        // 슬롯이 꽉 찼는지 확인
        if (_consumeItemSlots.Count >= ConsumeItemSlotCount)
        {
            Debug.LogWarning($"[Inventory] 소모 아이템 슬롯이 꽉 찼습니다. (최대 {ConsumeItemSlotCount}개)");
            return false;
        }

        // 새 슬롯 추가 (각 슬롯은 하나의 아이템만)
        var consumeItem = new ConsumeItem(itemData);
        _consumeItemSlots.Add(consumeItem);
        OnConsumeItemAdded?.Invoke(consumeItem);
        return true;
    }

    /// <summary>
    /// 소모 아이템 제거 (슬롯 인덱스로 제거)
    /// </summary>
    /// <param name="slotIndex">슬롯 인덱스</param>
    /// <returns>제거 성공 여부</returns>
    public bool RemoveConsumeItemAt(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= _consumeItemSlots.Count)
            return false;

        var item = _consumeItemSlots[slotIndex];
        _consumeItemSlots.RemoveAt(slotIndex);
        OnConsumeItemRemoved?.Invoke(item);
        return true;
    }

    /// <summary>
    /// 소모 아이템 제거 (아이템 데이터로 첫 번째 일치하는 슬롯 제거)
    /// </summary>
    /// <param name="itemData">아이템 데이터</param>
    /// <returns>제거 성공 여부</returns>
    public bool RemoveConsumeItem(ItemData itemData)
    {
        if (itemData == null)
            return false;

        var item = _consumeItemSlots.FirstOrDefault(i => i.ItemData == itemData);
        if (item == null)
            return false;

        _consumeItemSlots.Remove(item);
        OnConsumeItemRemoved?.Invoke(item);
        return true;
    }

    /// <summary>
    /// 소모 아이템 사용 (슬롯 인덱스로 사용)
    /// </summary>
    /// <param name="slotIndex">슬롯 인덱스</param>
    /// <returns>사용 성공 여부</returns>
    public bool UseConsumeItemAt(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= _consumeItemSlots.Count)
            return false;

        var item = _consumeItemSlots[slotIndex];
        if (item == null)
            return false;

        // 아이템 사용
        item.Use();

        // 사용 후 슬롯에서 제거 (소모 아이템이므로)
        _consumeItemSlots.RemoveAt(slotIndex);
        OnConsumeItemRemoved?.Invoke(item);
        return true;
    }

    /// <summary>
    /// 소모 아이템 사용 (아이템 데이터로 첫 번째 일치하는 슬롯 사용)
    /// </summary>
    /// <param name="itemData">아이템 데이터</param>
    /// <returns>사용 성공 여부</returns>
    public bool UseConsumeItem(ItemData itemData)
    {
        if (itemData == null)
            return false;

        var index = _consumeItemSlots.FindIndex(i => i.ItemData == itemData);
        if (index < 0)
            return false;

        return UseConsumeItemAt(index);
    }

    /// <summary>
    /// 특정 아이템 데이터의 소모 아이템 슬롯 개수 조회
    /// </summary>
    public int GetConsumeItemSlotCount(ItemData itemData)
    {
        if (itemData == null)
            return 0;

        return _consumeItemSlots.Count(i => i.ItemData == itemData);
    }

    /// <summary>
    /// 액티브 아이템 추가 (기존 아이템이 있으면 교체)
    /// </summary>
    public void AddActiveItem(ActiveItem item)
    {
        if (item == null)
        {
            Debug.LogWarning("[Inventory] 액티브 아이템이 null입니다.");
            return;
        }

        var oldItem = _activeItem;
        _activeItem = item;
        
        if (oldItem != null)
        {
            OnActiveItemRemoved?.Invoke(oldItem);
        }
        
        OnActiveItemAdded?.Invoke(item);
    }

    /// <summary>
    /// 액티브 아이템 제거
    /// </summary>
    public bool RemoveActiveItem()
    {
        if (_activeItem == null)
            return false;

        var item = _activeItem;
        _activeItem = null;
        OnActiveItemRemoved?.Invoke(item);
        return true;
    }

    /// <summary>
    /// 소모 아이템 슬롯 개수 증가
    /// </summary>
    public void IncreaseConsumeItemSlotCount(int amount = 1)
    {
        ConsumeItemSlotCount += amount;
        OnConsumeItemSlotCountChanged?.Invoke(ConsumeItemSlotCount);
    }

    /// <summary>
    /// 특정 아이템 데이터를 소지하고 있는지 확인
    /// </summary>
    /// <param name="itemData">확인할 아이템 데이터</param>
    /// <returns>소지 개수</returns>
    public int GetItemCount(ItemData itemData)
    {
        if (itemData == null)
            return 0;

        int count = 0;

        // 패시브 아이템 개수
        count += _passiveItems.Count(i => i.ItemData == itemData);

        // 소모 아이템 개수 (슬롯 개수)
        count += GetConsumeItemSlotCount(itemData);

        // 액티브 아이템 개수
        if (_activeItem != null && _activeItem.ItemData == itemData)
        {
            count += 1;
        }

        return count;
    }

    /// <summary>
    /// 특정 아이템 데이터를 소지하고 있는지 확인
    /// </summary>
    public bool HasItem(ItemData itemData)
    {
        return GetItemCount(itemData) > 0;
    }

    // 이벤트들
    public event Action<PassiveItem> OnPassiveItemAdded;
    public event Action<PassiveItem> OnPassiveItemRemoved;
    public event Action<ConsumeItem> OnConsumeItemAdded;
    public event Action<ConsumeItem> OnConsumeItemRemoved;
    public event Action<ActiveItem> OnActiveItemAdded;
    public event Action<ActiveItem> OnActiveItemRemoved;
    public event Action<int> OnConsumeItemSlotCountChanged;
}

