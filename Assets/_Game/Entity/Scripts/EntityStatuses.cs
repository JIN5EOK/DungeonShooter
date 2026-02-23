using System.Collections.Generic;

namespace DungeonShooter
{
    /// <summary>
    /// Entity의 현재 상태(현재 체력 등) 수치 관련 인터페이스.
    /// StatType과 대응되는 StatusType별 현재값을 보관한다. Modifier 없음.
    /// </summary>
    public interface IEntityStatuses
    {
        public void Initialize(EntityStatsTableEntry entry);
        public IEntityStatus GetStatus(StatusType type);
    }
    
    /// <summary>
    /// Entity의 현재 상태(현재 체력 등)를 관리하는 Pure C# 객체.
    /// </summary>
    public class EntityStatuses : IEntityStatuses
    {
        private readonly Dictionary<StatusType, EntityStatus> _statusValues = new Dictionary<StatusType, EntityStatus>();

        public EntityStatuses(EntityStatsTableEntry entry = null)
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

        public IEntityStatus GetStatus(StatusType type)
        {
            return GetOrAddStatus(type);
        }

        private EntityStatus GetOrAddStatus(StatusType type)
        {
            if (_statusValues.TryGetValue(type, out var statusValue))
                return statusValue;

            var value = new EntityStatus();
            _statusValues[type] = value;
            return value;
        }
    }
}
