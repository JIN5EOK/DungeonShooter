using System;
using Cysharp.Threading.Tasks;

namespace DungeonShooter
{
    /// <summary>
    /// 인벤토리의 아이템 인스턴스
    /// </summary>
    [Serializable]
    public class Item
    {
        /// <summary>
        /// 아이템 테이블 엔트리
        /// </summary>
        public ItemTableEntry ItemTableEntry { get; private set; }

        /// <summary>
        /// 스택 개수
        /// </summary>
        public int StackCount { get; set; }

        /// <summary>
        /// 소비 아이템 사용 스킬 (Consume 전용)
        /// </summary>
        public Skill UseSkill { get; private set; }

        /// <summary>
        /// 패시브 효과 스킬 (Passive 전용)
        /// </summary>
        public Skill PassiveSkill { get; private set; }

        /// <summary>
        /// 장착 효과 스킬 (Weapon 전용)
        /// </summary>
        public Skill EquipSkill { get; private set; }

        /// <summary>
        /// 액티브 효과 스킬 (Weapon 전용)
        /// </summary>
        public Skill ActiveSkill { get; private set; }

        /// <summary>
        /// 아이템 생성자
        /// </summary>
        /// <param name="itemTableEntry">아이템 테이블 엔트리</param>
        /// <param name="stackCount">초기 스택 개수 (기본값: 1)</param>
        public Item(ItemTableEntry itemTableEntry, int stackCount = 1)
        {
            if (itemTableEntry == null)
            {
                LogHandler.LogError<Item>("ItemTableEntry가 null입니다.");
                return;
            }

            ItemTableEntry = itemTableEntry;
            StackCount = Math.Max(1, Math.Min(stackCount, itemTableEntry.MaxStackCount));
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
        /// ItemTableEntry를 참고하여 스킬을 초기화합니다.
        /// </summary>
        /// <param name="tableRepository">테이블 리포지토리</param>
        /// <param name="resourceProvider">리소스 프로바이더</param>
        public async UniTask InitializeSkillsAsync(ITableRepository tableRepository, IStageResourceProvider resourceProvider)
        {
            if (ItemTableEntry == null || tableRepository == null || resourceProvider == null)
            {
                LogHandler.LogError<Item>("스킬 초기화에 필요한 데이터가 null입니다.");
                return;
            }

            var entry = ItemTableEntry;

            try
            {
                // UseSkill 생성 (Consume 전용)
                if (entry.UseEffect > 0)
                {
                    UseSkill = await CreateSkill(entry.UseEffect, tableRepository, resourceProvider);
                }

                // PassiveSkill 생성 (Passive 전용)
                if (entry.PassiveEffect > 0)
                {
                    PassiveSkill = await CreateSkill(entry.PassiveEffect, tableRepository, resourceProvider);
                }

                // EquipSkill 생성 (Weapon 전용)
                if (entry.ItemType == ItemType.Weapon && entry.EquipEffect > 0)
                {
                    EquipSkill = await CreateSkill(entry.EquipEffect, tableRepository, resourceProvider);
                }

                // ActiveSkill 생성 (Weapon 전용)
                if (entry.ItemType == ItemType.Weapon && entry.ActiveEffect > 0)
                {
                    ActiveSkill = await CreateSkill(entry.ActiveEffect, tableRepository, resourceProvider);
                }
            }
            catch (Exception ex)
            {
                LogHandler.LogError<Item>(ex, $"아이템 스킬 초기화 실패: {entry.Id}");
            }
        }

        /// <summary>
        /// 스킬을 생성합니다.
        /// </summary>
        private async UniTask<Skill> CreateSkill(int skillEntryId, ITableRepository tableRepository, IStageResourceProvider resourceProvider)
        {
            // SkillTableEntry 조회
            var skillTableEntry = tableRepository.GetTableEntry<SkillTableEntry>(skillEntryId);
            if (skillTableEntry == null)
            {
                LogHandler.LogError<Item>($"SkillTableEntry를 찾을 수 없습니다: {skillEntryId}");
                return null;
            }

            // Skill 인스턴스 생성
            var skill = new Skill(skillTableEntry);
            
            // SkillData 로드 및 초기화
            await skill.InitializeAsync(resourceProvider);
            
            if (!skill.IsInitialized)
            {
                LogHandler.LogError<Item>($"Skill 초기화 실패: {skillEntryId}");
                return null;
            }

            return skill;
        }

        /// <summary>
        /// 소비 아이템 사용 스킬을 실행합니다.
        /// </summary>
        /// <param name="target">스킬을 적용할 Entity</param>
        /// <returns>실행 성공 여부</returns>
        public async UniTask<bool> ExecuteUseSkill(EntityBase target)
        {
            if (UseSkill == null)
            {
                LogHandler.LogWarning<Item>($"UseSkill이 null입니다. (ItemId: {ItemTableEntry.Id})");
                return false;
            }

            return await UseSkill.Execute(target);
        }

        /// <summary>
        /// 무기 액티브 스킬을 실행합니다.
        /// </summary>
        /// <param name="target">스킬을 적용할 Entity</param>
        /// <returns>실행 성공 여부</returns>
        public async UniTask<bool> ExecuteActiveSkill(EntityBase target)
        {
            if (ActiveSkill == null)
            {
                LogHandler.LogWarning<Item>($"ActiveSkill이 null입니다. (ItemId: {ItemTableEntry.Id})");
                return false;
            }

            return await ActiveSkill.Execute(target);
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

        /// <summary>
        /// 모든 스킬 리소스를 정리합니다.
        /// </summary>
        public void DisposeSkills()
        {
            UseSkill?.Dispose();
            PassiveSkill?.Dispose();
            EquipSkill?.Dispose();
            ActiveSkill?.Dispose();

            UseSkill = null;
            PassiveSkill = null;
            EquipSkill = null;
            ActiveSkill = null;
        }
    }
}
