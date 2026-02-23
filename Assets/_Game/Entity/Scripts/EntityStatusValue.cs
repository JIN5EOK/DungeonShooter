using System;

namespace DungeonShooter
{
    /// <summary>
    /// 개별 상태 수치. 현재값만 보관하며 변경 시 OnValueChanged를 발생시킨다.
    /// </summary>
    public class EntityStatusValue : IEntityStatusValue
    {
        private int _value;

        public event Action<int> OnValueChanged;

        public int GetValue()
        {
            return _value;
        }

        public void SetValue(int value)
        {
            if (_value == value)
                return;
            _value = value;
            OnValueChanged?.Invoke(_value);
        }
    }
}
