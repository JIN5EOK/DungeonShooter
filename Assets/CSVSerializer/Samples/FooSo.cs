using System.Collections.Generic;
using UnityEngine;

namespace DungeonShooter
{
    [CreateAssetMenu(menuName = "DungeonShooter/CSVSerializer/FooSampleSo")]
    public sealed class FooSo : ScriptableObject
    {
        [SerializeField] private int _id;
        [SerializeField] private int _intValue;
        [SerializeField] private List<int> _intList = new();
        [SerializeField] private Bar _bar = new();

        public int Id => _id;

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
    }
}

