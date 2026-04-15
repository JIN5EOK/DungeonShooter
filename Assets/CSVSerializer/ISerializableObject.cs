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
        public List<TSerialized> CreateSerializedDto();
        public void ApplyFromSerializedDto(List<TSerialized> serializedDto);
    }
}

