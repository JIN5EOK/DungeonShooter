using System;

namespace DungeonShooter
{
    /// <summary>
    /// 무기 아이템. 장착하면 고유 능력(필살기)을 사용할 수 있습니다.
    /// </summary>
    public class WeaponItem : ItemBase, IEquipable, IUseable
    {
        private readonly Action _onUse;

        /// <summary>
        /// 무기 아이템 생성자
        /// </summary>
        /// <param name="itemData">아이템 데이터</param>
        /// <param name="onUse">고유 능력 사용 시 실행할 액션</param>
        public WeaponItem(ItemData itemData, Action onUse = null) : base(itemData)
        {
            _onUse = onUse;
        }

        public void Equip()
        {
            // 장착 시 처리 (필요시 구현)
        }

        public void Use()
        {
            // 고유 능력 사용
            _onUse?.Invoke();
        }
    }
}

