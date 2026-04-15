using System.Runtime.Serialization;
using UnityEngine;

namespace DungeonShooter
{
    public sealed class SerializedFoo : ISerializeSODto<FooSo>
    {
        public int Id { get; set; }
        public int IntValue { get; set; }
        public string IntList { get; set; }
        public int BarA { get; set; }
        public float BarB { get; set; }
        
        public void PopulateFrom(FooSo so)
        {
            Id = so.Id;
            IntValue = so.IntValue;
            IntList = DataSerializeHelper.ListToString(so.IntList);
            BarA = so.BarValue?.BarA ?? 0;
            BarB = so.BarValue?.BarB ?? 0f;
        }

        public void ApplyTo(FooSo so)
        {
            so.IntValue = IntValue;
            so.IntList = DataSerializeHelper.StringToIntList(IntList);

            var bar = so.BarValue ?? new FooSo.Bar();
            bar.BarA = BarA;
            bar.BarB = BarB;
            so.BarValue = bar;
        }
    }
}

