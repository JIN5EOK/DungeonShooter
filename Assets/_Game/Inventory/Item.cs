using System;

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
    }
}
