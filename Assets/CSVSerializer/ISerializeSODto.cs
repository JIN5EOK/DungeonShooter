using UnityEngine;

namespace DungeonShooter
{
    public interface IIntId
    {
        int Id { get; set; }
    }
    public interface ISerializeSODto<TSo> where TSo : ScriptableObject, IIntId
    {
        int Id { get; set; }
        void PopulateFrom(TSo so);
        void ApplyTo(TSo so);
    }
}

