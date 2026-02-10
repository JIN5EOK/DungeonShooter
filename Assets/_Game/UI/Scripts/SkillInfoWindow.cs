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
        
        /// <summary>
        /// 스킬 테이블 엔트리로 표시 내용을 설정합니다.
        /// </summary>
        public void SetInfo(string skillName, string skillDescription, float cooldown, Sprite icon)
        {
            _textName.text = skillName;
            _textDescription.text = skillDescription;
            _textCooldown.text = FormatCooldown(cooldown);
            _iconImage.sprite = icon;
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
