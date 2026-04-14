using UnityEngine;

namespace DungeonShooter
{
    public interface ISerializeSODto<TSo> where TSo : ScriptableObject
    {
        int Id { get; set; }
        void PopulateFromSo(TSo so);
        void ApplyTo(TSo so);
    }
}

