using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace DungeonShooter
{
    /// <summary>
    /// 아이템 정보를 표시하는 팝업 윈도우 UI.
    /// </summary>
    public class ItemInfoWindow : MonoBehaviour
    {
        [Header("아이템 정보")]
        [SerializeField] private Image _iconImage;
        [SerializeField] private TextMeshProUGUI _textName;
        [SerializeField] private TextMeshProUGUI _textDescription;
        [SerializeField] private TextMeshProUGUI _textType;
        [SerializeField] private TextMeshProUGUI _textStats;
        
        private ISceneResourceProvider _resourceProvider;
        private ITableRepository _tableRepository;
        [Inject]
        public void Construct(ISceneResourceProvider resourceProvider, ITableRepository tableRepository)
        {
            _resourceProvider = resourceProvider;
            _tableRepository = tableRepository;
        }
        
        /// <summary>
        /// 아이템 테이블 엔트리로 표시 내용을 설정합니다.
        /// </summary>
        public async UniTask SetEntry(ItemTableEntry entry)
        {
            if (entry == null)
            {
                LogHandler.LogError<ItemInfoWindow>("아이템 정보가 올바르지 않습니다.");
                return;
            }

            SetItemInternal(entry);
            _iconImage.sprite = await _resourceProvider.GetAssetAsync<Sprite>(entry.ItemIcon, SpriteAtlasAddresses.ItemIconAtlas);
        }

        /// <summary>
        /// 아이템 인스턴스로 표시 내용을 설정합니다.
        /// </summary>
        public void SetItem(Item item)
        {
            if (item == null || item.ItemTableEntry == null)
            {
                LogHandler.LogError<ItemInfoWindow>("아이템 정보가 올바르지 않습니다.");
                return;
            }

            SetItemInternal(item.ItemTableEntry);
            _iconImage.sprite = item.Icon;
        }

        private void SetItemInternal(ItemTableEntry entry)
        {
            SetText(_textName, _tableRepository.GetStringText(entry.ItemNameId));
            SetText(_textDescription, _tableRepository.GetStringText(entry.ItemDescriptionId));
            SetText(_textType, GetItemTypeString(entry.ItemType));
            SetText(_textStats, BuildStatsString(entry));
        }
        
        public void Clear()
        {
            SetText(_textName, string.Empty);
            SetText(_textDescription, string.Empty);
            SetText(_textType, string.Empty);
            SetText(_textStats, string.Empty);
            _iconImage.sprite = null;
        }

        private void SetText(TextMeshProUGUI textUi, string value)
        {
            textUi.text = value ?? string.Empty;
        }

        private string GetItemTypeString(ItemType type)
        {
            return type switch
            {
                ItemType.Weapon => _tableRepository.GetStringText(19000001),
                ItemType.Passive => _tableRepository.GetStringText(19000002),
                ItemType.Consume => _tableRepository.GetStringText(19000003),
                _ => type.ToString()
            };
        }

        private string BuildStatsString(ItemTableEntry entry)
        {
            // TODO: 하드코딩된 텍스트 추후 분리 필요
            if (entry == null)
                return string.Empty;

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
