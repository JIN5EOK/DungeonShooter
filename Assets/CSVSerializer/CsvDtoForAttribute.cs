using System;

namespace DungeonShooter
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class CsvDtoForAttribute : Attribute
    {
        public CsvDtoForAttribute(Type soType)
        {
            SoType = soType;
        }

        public Type SoType { get; }
    }
}

