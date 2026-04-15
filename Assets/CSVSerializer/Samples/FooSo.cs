using System.Collections.Generic;
using UnityEngine;

namespace DungeonShooter
{
    [CreateAssetMenu(menuName = "DungeonShooter/CSVSerializer/FooSampleSo")]
    public class FooSo : ScriptableObject, ISerializableObject<SerializedFoo>
    {
        [SerializeField] private int _id;
        [SerializeField] private int _intValue;
        [SerializeField] private List<int> _intList = new();
        [SerializeField] private Bar _bar = new();

        public int Id
        {
            get => _id;
            set => _id = value;
        }

        public int IntValue
        {
            get => _intValue;
            set => _intValue = value;
        }

        public List<int> IntList
        {
            get => _intList;
            set => _intList = value;
        }

        public Bar BarValue
        {
            get => _bar;
            set => _bar = value;
        }

        [System.Serializable]
        public sealed class Bar
        {
            public int BarA;
            public float BarB;
        }
        
        
        // 직렬화 데이터 생성
        public List<SerializedFoo> CreateSerializedDto()
        {
            var list = new List<SerializedFoo>();
            
            // 루트
            list.Add(new SerializedFoo (_id, _intValue, null, _bar.BarA, _bar.BarB));
            
            for (int i = 0; i < _intList.Count; i++)
            {
                var sFoo = new SerializedFoo(_id, null, _intList[i], null,null);
                list.Add(sFoo);
            }

            return list;
        }

        public void ApplyFromSerializedDto(List<SerializedFoo> serializedDto)
        {
            _intList.Clear();
            foreach (var serializedFoo in serializedDto)
            {
                _id = serializedFoo.Id;
                _intValue = serializedFoo.IntValue ?? _intValue;
                if(serializedFoo.IntListElement != null)
                    _intList.Add(serializedFoo.IntListElement.Value);
                _bar.BarA = serializedFoo.BarA ?? _bar.BarA;
                _bar.BarB = serializedFoo.BarB ?? _bar.BarB;
            }
        }
    }
}

