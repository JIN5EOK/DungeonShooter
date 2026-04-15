using System.Collections.Generic;
using UnityEngine;

namespace DungeonShooter
{
    public interface ISerializableObject<TSerialized> : 
        ITableEntry
        where TSerialized : class
    {
        public List<TSerialized> CreateSerializedDto();
        public void ApplyFromSerializedDto(List<TSerialized> serializedDto);
    }
}

