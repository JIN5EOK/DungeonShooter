using System.Collections.Generic;

namespace DungeonShooter
{
    /// <summary>
    /// Entity의 현재 상태(현재 체력 등)를 관리하는 Pure C# 객체.
    /// </summary>
    public class EntityStatus : IEntityStatus
    {
        private readonly Dictionary<StatusType, EntityStatusValue> _statusValues = new Dictionary<StatusType, EntityStatusValue>();

        public EntityStatus(EntityStatsTableEntry entry = null)
        {
            if (entry != null)
                GetOrAddStatus(StatusType.Hp).SetValue(entry.MaxHp);
        }

        public void Initialize(EntityStatsTableEntry entry)
        {
            if (entry == null)
                return;
            GetOrAddStatus(StatusType.Hp).SetValue(entry.MaxHp);
        }

        public IEntityStatusValue GetStatus(StatusType type)
        {
            return GetOrAddStatus(type);
        }

        private EntityStatusValue GetOrAddStatus(StatusType type)
        {
            if (_statusValues.TryGetValue(type, out var statusValue))
                return statusValue;

            var value = new EntityStatusValue();
            _statusValues[type] = value;
            return value;
        }
    }
}
