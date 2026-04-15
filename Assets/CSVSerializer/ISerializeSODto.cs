using System.Collections.Generic;
using UnityEngine;

namespace DungeonShooter
{
    public interface IIntId
    {
        int Id { get; set; }
    }

    public interface ISerializableObject<TObject, TSerialized>
        where TObject : ScriptableObject, ISerializableObject<TObject, TSerialized>, IIntId
        where TSerialized : class, IIntId
    {
#if UNITY_EDITOR
        List<TSerialized> CreateSerializedDto();
        void ApplyFromSerializedDto(List<TSerialized> serializedDto);
#endif
    }
}

