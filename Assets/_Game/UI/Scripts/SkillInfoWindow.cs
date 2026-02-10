using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using VContainer;

namespace DungeonShooter
{
    /// <summary>
    /// 스킬 정보(아이콘, 이름, 설명, 쿨타임)를 표시하는 윈도우.
    /// </summary>
    public class SkillInfoWindow : MonoBehaviour
    {
        [Header("스킬 정보")]
        [SerializeField] private Image _iconImage;
        [SerializeField] private TextMeshProUGUI _textName;
        [SerializeField] private TextMeshProUGUI _textDescription;
        [SerializeField] private TextMeshProUGUI _textCooldown;

        private ISceneResourceProvider _resourceProvider;

        [Inject]
        public void Construct(ISceneResourceProvider resourceProvider, ITableRepository tableRepository)
        {
            _resourceProvider = resourceProvider;
        }

        /// <summary>
        /// 스킬 테이블 엔트리로 표시 내용을 설정합니다.
        /// </summary>
        public async UniTask SetEntry(SkillTableEntry entry)
        {
            if (entry == null)
            {
                LogHandler.LogError<SkillInfoWindow>("스킬 정보가 올바르지 않습니다.");
                return;
            }

            _textName.text = entry.SkillName;
            _textDescription.text = entry.SkillDescription;
            _textCooldown.text = FormatCooldown(entry.Cooldown);

            var sprite = await _resourceProvider.GetAssetAsync<Sprite>(entry.SkillIconKey, SpriteAtlasAddresses.SkillIconAtlas);
            _iconImage.sprite = sprite == null ? null : sprite;
        }

        /// <summary>
        /// 표시 내용을 비웁니다.
        /// </summary>
        public void Clear()
        {
            _textName.text = string.Empty;
            _textDescription.text = string.Empty;
            _textCooldown.text = string.Empty;
            _iconImage.sprite = null;
        }

        private static string FormatCooldown(float cooldown)
        {
            if (cooldown <= 0f)
                return string.Empty;
            return $"쿨다운: {cooldown}초";
        }
    }
}
