using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

namespace DungeonShooter
{
    /// <summary>
    /// 인벤토리 명령 창구 및 서비스 로직,플레이어 게임 오브젝트 스킬/스탯 연동을 담당
    /// 아이템 추가/제거/소비 등 데이터 연산은 InventoryModel에서 수행
    /// </summary>
    public class Inventory : IInventory
    {
        private const int StringIdItemObtainedFormat = 19200064;

        public event Action<Item> OnItemAdded;
        public event Action<Item> OnItemRemoved;
        public event Action<Item> OnItemStackChanged;
        public event Action<Item> OnWeaponEquipped;
        public event Action<Item> OnWeaponUnequipped;
        public event Action<Item> OnItemUse;

        public Item EquippedWeapon => _model.EquippedWeapon;

        private readonly InventoryModel _model;
        private IPlayerContextManager _playerContextManager;
        private IEventBus _eventBus;
        private ITableRepository _tableRepository;
        private AlertMessageViewModel _alertMessageViewModel;

        private IEntityStats StatContainer => _playerContextManager?.EntityContext?.Stat;
        private IEntitySkills SkillContainer => _playerContextManager?.EntityContext?.Skill;

        private EntityBase _ownerEntity;

        [Inject]
        public Inventory(IEventBus eventBus, IPlayerContextManager playerContextManager, ITableRepository tableRepository, AlertMessageViewModel alertMessageViewModel)
        {
            _model = playerContextManager.InventoryModel;
            _playerContextManager = playerContextManager;
            _eventBus = eventBus;
            _tableRepository = tableRepository;
            _alertMessageViewModel = alertMessageViewModel;
            _model.OnItemAdded += OnItemAdded;
            _model.OnItemRemoved += OnItemRemoved;
            _model.OnItemStackChanged += OnItemStackChanged;
            _model.OnWeaponEquipped += OnWeaponEquipped;
            _model.OnWeaponUnequipped += OnWeaponUnequipped;
            _eventBus.Subscribe<PlayerObjectSpawnEvent>(PlayerObjectSpawned);
            _eventBus.Subscribe<PlayerObjectDestroyEvent>(PlayerObjectDespawned);
        }

        private void PlayerObjectSpawned(PlayerObjectSpawnEvent playerObjectSpawnEvent)
        {
            _ownerEntity = playerObjectSpawnEvent.player;
        }

        private void PlayerObjectDespawned(PlayerObjectDestroyEvent playerObjectDestroyEvent)
        {
            _ownerEntity = null;
        }

        public IReadOnlyCollection<Item> GetItems() => _model.Items.ToList().AsReadOnly();

        /// <summary>
        /// 아이템 추가
        /// </summary>
        public bool AddItem(Item item)
        {
            if (item == null)
            {
                LogHandler.LogWarning<Inventory>("아이템이 null입니다.");
                return false;
            }

            var amountAdded = item.StackCount;
            if (!_model.AddItem(item))
                return false;

            var format = _tableRepository?.GetStringText(StringIdItemObtainedFormat);
            if (!string.IsNullOrEmpty(format) && _alertMessageViewModel != null)
            {
                var itemName = _tableRepository?.GetStringText(item.ItemTableEntry.ItemNameId) ?? item.ItemTableEntry.ItemNameId.ToString();
                var message = string.Format(format, itemName, amountAdded);
                _alertMessageViewModel.SetMessage(message);
            }

            if (item.PassiveSkill != null)
                SkillContainer?.Regist(item.PassiveSkill);

            return true;
        }

        /// <summary>
        /// 아이템 장착
        /// </summary>
        public bool EquipItem(Item item)
        {
            if (item == null)
            {
                LogHandler.LogWarning<Inventory>("아이템이 null입니다.");
                return false;
            }
            if (item.ItemTableEntry.ItemType != ItemType.Weapon)
            {
                LogHandler.LogWarning<Inventory>("무기 타입의 아이템만 장착할 수 있습니다.");
                return false;
            }
            if (!_model.Contains(item))
            {
                LogHandler.LogWarning<Inventory>("인벤토리에 없는 아이템입니다.");
                return false;
            }

            var previousWeapon = _model.EquippedWeapon;
            if (previousWeapon != null)
            {
                if (previousWeapon.EquipSkill != null)
                    SkillContainer?.Unregist(previousWeapon.EquipSkill);
                RemoveItemStatBonus(previousWeapon);
            }

            if (!_model.EquipItem(item))
                return false;

            if (item.EquipSkill != null)
                SkillContainer?.Regist(item.EquipSkill);
            ApplyItemStatBonus(item);

            return true;
        }

        /// <summary>
        /// 소지/장착 중인 모든 아이템을 제거
        /// </summary>
        public void Clear()
        {
            var copy = _model.Items.ToList();
            foreach (var item in copy)
                RemoveItem(item);
        }

        /// <summary>
        /// 아이템 제거 (스킬/스탯 해제 후 모델 위임·이벤트 발행)
        /// </summary>
        public void RemoveItem(Item item)
        {
            if (item == null)
            {
                LogHandler.LogError<Inventory>("제거하려는 아이템이 인벤토리에 없습니다.");
                return;
            }

            if (item == _model.EquippedWeapon)
            {
                if (item.EquipSkill != null)
                    SkillContainer?.Unregist(item.EquipSkill);
                RemoveItemStatBonus(item);
            }

            if (item.ItemTableEntry.ItemType == ItemType.Passive)
                RemoveItemStatBonus(item);

            if (item.PassiveSkill != null)
                SkillContainer?.Unregist(item.PassiveSkill);

            _model.RemoveItem(item);
        }

        /// <summary>
        /// 소비 아이템 사용
        /// </summary>
        public void UseItem(Item item)
        {
            if (item == null)
            {
                LogHandler.LogWarning<Inventory>("아이템이 null입니다.");
                return;
            }
            if (item.ItemTableEntry.ItemType != ItemType.Consume)
            {
                LogHandler.LogWarning<Inventory>("소비 아이템만 사용할 수 있습니다.");
                return;
            }
            if (!_model.Contains(item))
            {
                LogHandler.LogWarning<Inventory>("인벤토리에 없는 아이템입니다.");
                return;
            }

            item.StackCount--;
            if (item.StackCount <= 0)
                RemoveItem(item);

            if (_ownerEntity != null)
                item.ExecuteUseSkill(_ownerEntity).Forget();

            OnItemUse?.Invoke(item);
        }

        private void ApplyItemStatBonus(Item item)
        {
            StatContainer?.ApplyStatBonus(item, StatBonus.From(item.ItemTableEntry));
        }

        private void RemoveItemStatBonus(Item item)
        {
            StatContainer?.RemoveStatBonus(item);
        }

        public void Dispose()
        {
            _model.OnItemAdded -= OnItemAdded;
            _model.OnItemRemoved -= OnItemRemoved;
            _model.OnItemStackChanged -= OnItemStackChanged;
            _model.OnWeaponEquipped -= OnWeaponEquipped;
            _model.OnWeaponUnequipped -= OnWeaponUnequipped;
            _eventBus.Unsubscribe<PlayerObjectSpawnEvent>(PlayerObjectSpawned);
            _eventBus.Unsubscribe<PlayerObjectDestroyEvent>(PlayerObjectDespawned);
        }
    }
}
