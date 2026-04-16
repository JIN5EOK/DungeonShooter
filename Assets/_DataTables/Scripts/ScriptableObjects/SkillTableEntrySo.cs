using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Localization;

namespace DungeonShooter
{
    [CreateAssetMenu(menuName = "DungeonShooter/DataTables/Skill")]
    public sealed class SkillTableEntrySo : ScriptableObject, ISerializableObject<SkillTableDto>
    {
        public int Id
        {
            get => _id;
            protected set => _id = value;
        }

        public string SkillName => _skillName.GetLocalizedString();
        public string SkillDescription => _skillDescription.GetLocalizedString();
        public AssetReferenceT<Sprite> SkillIconRef => _skillIconRef;
        public IReadOnlyList<SkillLevelData> SkillLevels => _skillLevels;

        [TextArea][SerializeField] private string _memo;

        [SerializeField] private int _id;
        [SerializeField] private LocalizedString _skillName;
        [SerializeField] private LocalizedString _skillDescription;
        [SerializeField] private AssetReferenceT<Sprite> _skillIconRef;
        [SerializeField] private List<SkillLevelData> _skillLevels;

        public List<SkillTableDto> CreateSerializedDto()
        {
            var list = new List<SkillTableDto>();
            list.Add(new SkillTableDto(
                _id
                , SoSerializeHelper.SerializeLocalizedString(_skillName)
                , SoSerializeHelper.SerializeLocalizedString(_skillDescription)
                , SoSerializeHelper.SerializeAssetReference(_skillIconRef)
                , null, null, null, null, null, null, _memo));

            for (var i = 0; i < _skillLevels.Count; i++)
            {
                var data = _skillLevels[i];
                list.Add(new SkillTableDto(
                    _id, null, null, null
                    , i + 1
                    , SoSerializeHelper.SerializeLocalizedString(data.SkillLevelName)
                    , SoSerializeHelper.SerializeLocalizedString(data.SkillLevelDescription)
                    , SoSerializeHelper.SerializeAssetReference(data.SkillDataRef)
                    , data.Amount
                    , data.Cooldown));
            }

            return list;
        }

        public void ApplyFromSerializedDto(List<SkillTableDto> serializedDto)
        {
            _skillLevels.Clear();
            foreach (var dto in serializedDto)
            {
                _memo = dto.Memo;
                _id = dto.Id;
                _skillName = !string.IsNullOrEmpty(dto.SkillName) ? SoSerializeHelper.DeserializeLocalizedString(dto.SkillName) : _skillName;
                _skillDescription = !string.IsNullOrEmpty(dto.SkillDescription) ? SoSerializeHelper.DeserializeLocalizedString(dto.SkillDescription) : _skillDescription;
                _skillIconRef = string.IsNullOrEmpty(dto.SkillIconKey) ? _skillIconRef : SoSerializeHelper.DeserializeAssetReference<Sprite>(dto.SkillIconKey);
                if (dto.Level != null)
                {
                    var skillRef = SoSerializeHelper.DeserializeAssetReference<SkillData>(dto.SkillDataKey);
                    _skillLevels.Add(new SkillLevelData(
                        skillRef
                        , dto.Amount ?? 0
                        , dto.Cooldown ?? 0.0f
                        , SoSerializeHelper.DeserializeLocalizedString(dto.SkillLevelName)
                        , SoSerializeHelper.DeserializeLocalizedString(dto.SkillLevelDescription)));
                }

            }
        }
    }
    [Serializable]
    public class SkillLevelData
    {
        public string SKillLevelName => _skillLevelName.GetLocalizedString();
        public LocalizedString SkillLevelName => _skillLevelName;
        [SerializeField] private LocalizedString _skillLevelName;
        public string SkillDescription => _skillLevelDescription.GetLocalizedString();
        public LocalizedString SkillLevelDescription => _skillLevelDescription;
        [SerializeField] private LocalizedString _skillLevelDescription;
        public AssetReferenceT<SkillData> SkillDataRef => _skillDataRef;
        [SerializeField] private AssetReferenceT<SkillData> _skillDataRef;
        public int Amount => _amount;
        [SerializeField] private int _amount;
        public float Cooldown => _cooldown;
        [SerializeField] private float _cooldown;

        public SkillLevelData(AssetReferenceT<SkillData> skillDataRef, int amount, float cooldown, LocalizedString skillLevelName, LocalizedString skillLevelDescription)
        {
            _skillDataRef = skillDataRef;
            _amount = amount;
            _cooldown = cooldown;
            _skillLevelName = skillLevelName;
            _skillLevelDescription = skillLevelDescription;
        }
    }
}

