using System;

namespace DungeonShooter
{
    /// <summary>
    /// 개별 상태 수치(현재 체력 등). Modifier 없이 현재값만 보관.
    /// </summary>
    public interface IEntityStatusValue
    {
        public int GetValue();
        public void SetValue(int value);
        public event Action<int> OnValueChanged;
    }
}
