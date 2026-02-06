using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

namespace DungeonShooter
{
    /// <summary>
    /// 인벤토리 아이템 목록을 슬롯으로 표시하고, 슬롯 클릭 시 아이템 정보 요청을 전달하는 HUD.
    /// </summary>
    public class InventoryUI : PopupUI
    {
        [Header("레이아웃")]
        [SerializeField] private RectTransform _content;
        [Header("슬롯 프리팹")]
        [SerializeField] private InventorySlotUIElement _slotPrefab;
        [Header("정보 패널")]
        [SerializeField] private ItemInfoWindow _itemInfoPanel;

        private Inventory _inventory;
        private ISceneResourceProvider _resourceProvider;
        private ITableRepository _tableRepository;
        
        private Dictionary<Item, InventorySlotUIElement> _slotsDict = new();
        
        [Inject]
        public void Construct(ISceneResourceProvider resourceProvider, ITableRepository tableRepository, Inventory inventory)
        {
            _resourceProvider = resourceProvider;
            _tableRepository = tableRepository;
            _inventory = inventory;
        }

        public override void Show()
        {
            base.Show();
            RefreshSlots();
        }
        
        private void OnDestroy()
        {
            if (_inventory != null)
            {
                _inventory.OnItemAdded -= HandleItemAdded;
                _inventory.OnItemRemoved -= HandleItemRemoved;
            }
        }

        private void HandleItemAdded(Item item)
        {
            var slot = Instantiate(_slotPrefab, _content, false);
            slot.SetItem(item);
            _slotsDict.Add(item, slot);
            slot.OnSlotClicked += _itemInfoPanel.SetItem;
        }

        private void HandleItemRemoved(Item item)
        {
            if (_slotsDict.ContainsKey(item))
            {
                var slot = _slotsDict[item];
                _slotsDict.Remove(item);
                Destroy(slot.gameObject);
            }
        }
        
        private void RefreshSlots()
        {
            _itemInfoPanel.Clear();
            if (_content == null || _slotPrefab == null || _inventory == null)
                return;

            var items = _inventory.Items;
            foreach (var item in items)
            {
                if (!_slotsDict.ContainsKey(item))
                {
                    HandleItemAdded(item);
                }
            }
            
            foreach (var itemSlot in _slotsDict.Values)
            {
                // TODO: 리스트 기반 조회라 성능 낮음, 개선필요
                if (!items.Contains(itemSlot.BoundItem))
                {
                    HandleItemRemoved(itemSlot.BoundItem);
                }
            }
        }
    }
}
