using System.Collections.Generic;
using UnityEngine;

namespace DungeonShooter
{
    public interface IIntId
    {
        int Id { get;}
    }

    public interface ISerializableObject<TSerialized> : 
        IIntId
        where TSerialized : class
    {
        List<TSerialized> CreateSerializedDto();
        void ApplyFromSerializedDto(List<TSerialized> serializedDto);
    }
}

