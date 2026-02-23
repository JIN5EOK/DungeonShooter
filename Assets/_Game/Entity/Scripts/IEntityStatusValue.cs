using System;

namespace DungeonShooter
{
    /// <summary>
    /// 개별 상태 수치(현재 체력 등). Modifier 없이 현재값만 보관.
    /// </summary>
    public interface IEntityStatusValue
    {
        int GetValue();
        void SetValue(int value);
        event Action<int> OnValueChanged;
    }
}
