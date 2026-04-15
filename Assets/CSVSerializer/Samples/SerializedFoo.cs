namespace DungeonShooter
{
    public sealed class SerializedFoo : IIntId
    {
        public int Id { get; set; }
        public int? IntValue { get; set; }
        public int? IntListElement { get; set; }
        public int? BarA { get; set; }
        public float? BarB { get; set; }

        public SerializedFoo(int id, int? intValue, int? intListElement, int? barA, float? barB)
        {
            Id = id;
            IntValue = intValue;
            IntListElement = intListElement;
            BarA = barA;
            BarB = barB;
        }

        public SerializedFoo()
        {
        }
    }
}

