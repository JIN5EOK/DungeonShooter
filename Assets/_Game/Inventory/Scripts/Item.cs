using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

namespace DungeonShooter
{
    /// <summary>
    /// 인벤토리의 아이템 인스턴스
    /// </summary>
    [Serializable]
    public class Item
    {
        /// <summary> 아이템 테이블 엔트리 </summary>
        public ItemTableEntry ItemTableEntry { get; private set; }

        /// <summary> 스택 개수 </summary>
        public int StackCount { get; set; }

        private readonly ISkillFactory _skillFactory;
        private readonly ISceneResourceProvider _sceneResourceProvider;

        //// <summary> 아이템 아이콘 </summary>
        public Sprite Icon { get; private set; }
        /// <summary> 소비 아이템 사용 스킬 (Consume 전용) </summary>
        public Skill UseSkill { get; private set; }

        /// <summary> 패시브 효과 스킬 (Passive 전용)</summary>
        public Skill PassiveSkill { get; private set; }

        /// <summary> 장착 효과 스킬 (Weapon 전용) </summary>
        public Skill EquipSkill { get; private set; }

        /// <summary> 액티브 효과 스킬 (Weapon 전용) </summary>
        public Skill ActiveSkill { get; private set; }

        /// <summary>
        /// 아이템 생성자
        /// </summary>
        /// <param name="itemTableEntry">아이템 테이블 엔트리</param>
        /// <param name="skillFactory">스킬 팩토리</param>
        [Inject]
        public Item(ItemTableEntry itemTableEntry, ISkillFactory skillFactory, ISceneResourceProvider sceneResourceProvider)
        {
            if (itemTableEntry == null)
            {
                LogHandler.LogError<Item>("ItemTableEntry가 null입니다.");
                return;
            }

            ItemTableEntry = itemTableEntry;
            StackCount = 1;
            _skillFactory = skillFactory;
            _sceneResourceProvider = sceneResourceProvider;
        }

        /// <summary>
        /// 스택을 추가할 수 있는지 확인
        /// </summary>
        /// <param name="amount">추가할 개수</param>
        /// <returns>추가 가능 여부</returns>
        public bool CanAddStack(int amount)
        {
            return StackCount + amount <= ItemTableEntry.MaxStackCount;
        }

        /// <summary>
        /// 스택 추가
        /// </summary>
        /// <param name="amount">추가할 개수</param>
        /// <returns>실제 추가된 개수</returns>
        public int AddStack(int amount)
        {
            var availableSpace = ItemTableEntry.MaxStackCount - StackCount;
            var actualAmount = Math.Min(amount, availableSpace);
            StackCount += actualAmount;
            return actualAmount;
        }

        /// <summary>
        /// 아이템이 초기화되었는지 확인합니다.
        /// </summary>
        public bool IsInitialized()
        {
            return Icon != null || UseSkill != null || PassiveSkill != null || EquipSkill != null || ActiveSkill != null;
        }

        /// <summary>
        /// ItemTableEntry를 참고하여 스킬을 초기화합니다.
        /// </summary>
        public async UniTask InitializeAsync()
        {
            if (ItemTableEntry == null || _skillFactory == null)
            {
                LogHandler.LogError<Item>("스킬 초기화에 필요한 데이터가 null입니다.");
                return;
            }

            // 이미 초기화된 경우 스킵
            if (IsInitialized())
            {
                return;
            }

            var entry = ItemTableEntry;

            try
            {
                Icon = await _sceneResourceProvider.GetAssetAsync<Sprite>(entry.ItemIcon, SpriteAtlasAddresses.ItemIconAtlas);
                // UseSkill 생성 (Consume 전용)
                if (entry.UseSkillId > 0)
                {
                    UseSkill = await _skillFactory.CreateSkillAsync(entry.UseSkillId);
                }

                // PassiveSkill 생성 (Passive 전용)
                if (entry.PassiveSkillId > 0)
                {
                    PassiveSkill = await _skillFactory.CreateSkillAsync(entry.PassiveSkillId);
                }

                // EquipSkill 생성 (Weapon 전용)
                if (entry.ItemType == ItemType.Weapon && entry.EquipSkillId > 0)
                {
                    EquipSkill = await _skillFactory.CreateSkillAsync(entry.EquipSkillId);
                }

                // ActiveSkill 생성 (Weapon 전용)
                if (entry.ItemType == ItemType.Weapon && entry.ActiveSkillId > 0)
                {
                    ActiveSkill = await _skillFactory.CreateSkillAsync(entry.ActiveSkillId);
                }
            }
            catch (Exception ex)
            {
                LogHandler.LogException<Item>(ex, $"아이템 스킬 초기화 실패: {entry.Id}");
            }
        }

        /// <summary>
        /// 소비 아이템 사용 스킬을 실행합니다.
        /// </summary>
        /// <param name="caster">스킬 시전자</param>
        /// <returns>실행 성공 여부</returns>
        public async UniTask<bool> ExecuteUseSkill(EntityBase caster)
        {
            if (UseSkill == null)
            {
                LogHandler.LogWarning<Item>($"UseSkill이 null입니다. (ItemId: {ItemTableEntry.Id})");
                return false;
            }

            return await UseSkill.Execute(caster);
        }

        /// <summary>
        /// 무기 액티브 스킬을 실행합니다.
        /// </summary>
        /// <param name="caster">스킬 시전자</param>
        /// <returns>실행 성공 여부</returns>
        public async UniTask<bool> ExecuteActiveSkill(EntityBase caster)
        {
            if (ActiveSkill == null)
            {
                LogHandler.LogWarning<Item>($"ActiveSkill이 null입니다. (ItemId: {ItemTableEntry.Id})");
                return false;
            }

            return await ActiveSkill.Execute(caster);
        }

        /// <summary>
        /// 패시브 스킬을 활성화합니다.
        /// </summary>
        /// <param name="owner">스킬 소유자</param>
        public void ActivatePassiveSkill(EntityBase owner)
        {
            if (PassiveSkill != null)
            {
                PassiveSkill.Activate(owner);
            }
        }

        /// <summary>
        /// 패시브 스킬을 비활성화합니다.
        /// </summary>
        /// <param name="owner">스킬 소유자</param>
        public void DeactivatePassiveSkill(EntityBase owner)
        {
            if (PassiveSkill != null)
            {
                PassiveSkill.Deactivate(owner);
            }
        }

        /// <summary>
        /// 장착 스킬을 활성화합니다.
        /// </summary>
        /// <param name="owner">스킬 소유자</param>
        public void ActivateEquipSkill(EntityBase owner)
        {
            if (EquipSkill != null)
            {
                EquipSkill.Activate(owner);
            }
        }

        /// <summary>
        /// 장착 스킬을 비활성화합니다.
        /// </summary>
        /// <param name="owner">스킬 소유자</param>
        public void DeactivateEquipSkill(EntityBase owner)
        {
            if (EquipSkill != null)
            {
                EquipSkill.Deactivate(owner);
            }
        }
    }
}
