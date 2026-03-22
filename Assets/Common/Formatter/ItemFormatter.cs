using System.Collections.Generic;

namespace DungeonShooter
{
    public interface IItemFormatter
    {
        string GetFormattedItemName(ItemTableEntry entry);
        string GetFormattedItemDescription(ItemTableEntry entry);
        string GetFormattedItemType(ItemType type);
        string GetFormattedItemEffects(ItemTableEntry entry);
    }


    public class ItemFormatter : IItemFormatter
    {
        private readonly ITableRepository _tableRepository;

        public ItemFormatter(ITableRepository tableRepository)
        {
            _tableRepository = tableRepository;
        }

        public string GetFormattedItemName(ItemTableEntry entry)
        {
            if (entry == null) return string.Empty;
            return _tableRepository.GetStringText(entry.ItemNameId);
        }

        public string GetFormattedItemDescription(ItemTableEntry entry)
        {
            if (entry == null) return string.Empty;
            return _tableRepository.GetStringText(entry.ItemDescriptionId);
        }

        public string GetFormattedItemType(ItemType type)
        {
            return type switch
            {
                ItemType.Weapon => _tableRepository.GetStringText(19000001),
                ItemType.Passive => _tableRepository.GetStringText(19000002),
                ItemType.Consume => _tableRepository.GetStringText(19000003),
                _ => type.ToString()
            };
        }

        public string GetFormattedItemEffects(ItemTableEntry entry)
        {
            if (entry == null) return string.Empty;

            var parts = new List<string>();

            var hpText = _tableRepository.GetStringText(19000004);
            var atkText = _tableRepository.GetStringText(19000005);
            var defText = _tableRepository.GetStringText(19000006);
            var moveSpeedText = _tableRepository.GetStringText(19000007);
            
            if (entry.HpAdd != 0)
                parts.Add($"{hpText} +{entry.HpAdd}");
            if (entry.HpMultiply != 100)
                parts.Add($"{hpText} {entry.HpMultiply}%");

            if (entry.AttackAdd != 0)
                parts.Add($"{atkText} +{entry.AttackAdd}");
            if (entry.AttackMultiply != 100)
                parts.Add($"{atkText} {entry.AttackMultiply}%");

            if (entry.DefenseAdd != 0)
                parts.Add($"{defText} +{entry.DefenseAdd}");
            if (entry.DefenseMultiply != 100)
                parts.Add($"{defText} {entry.DefenseMultiply}%");

            if (entry.MoveSpeedAdd != 0)
                parts.Add($"{moveSpeedText} +{entry.MoveSpeedAdd}");
            if (entry.MoveSpeedMultiply != 100)
                parts.Add($"{moveSpeedText} {entry.MoveSpeedMultiply}%");

            return parts.Count > 0 ? string.Join("  ", parts) : string.Empty;
        }
    }
}
