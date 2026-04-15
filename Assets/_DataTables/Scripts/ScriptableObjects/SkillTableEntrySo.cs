using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Localization;

namespace DungeonShooter
{
    [CreateAssetMenu(menuName = "DungeonShooter/DataTables/SkillTableEntry")]
    public sealed class SkillTableEntrySo : ScriptableObject, ISerializableObject<SerializedSkillTableRow>
    {
        [Serializable]
        public class SkillLevelData
        {
            
            public AssetReferenceT<SkillData> skillDataRef;
            public int amount;
            public float cooldown;

            public SkillLevelData(AssetReferenceT<SkillData> skillDataRef, int amount, float cooldown)
            {
                this.skillDataRef = skillDataRef;
                this.amount = amount;
                this.cooldown = cooldown;
            }
        }
        
        public int Id
        {
            get => _id;
            protected set => _id = value;
        }

        public string SkillName => _skillName.GetLocalizedString();
        public string SkillDescription => _skillDescription.GetLocalizedString();
        public AssetReferenceT<Sprite> SkillIconRef => _skillIconRef;
        public IReadOnlyList<SkillLevelData> SkillLevels => _skillLevels;
        
        [SerializeField] private int _id;
        [SerializeField] private LocalizedString _skillName;
        [SerializeField] private LocalizedString _skillDescription;
        [SerializeField] private AssetReferenceT<Sprite> _skillIconRef;
        [SerializeField] private List<SkillLevelData> _skillLevels;

        public List<SerializedSkillTableRow> CreateSerializedDto()
        {
            var list = new List<SerializedSkillTableRow>();
            list.Add(new SerializedSkillTableRow(
                _id
                , SoSerializeHelper.SerializeLocalizedString(_skillName)
                , SoSerializeHelper.SerializeLocalizedString(_skillDescription)
                , SoSerializeHelper.SerializeAssetReference(_skillIconRef)
                , null, null, null, null));
            
            for (var i = 0; i < _skillLevels.Count; i++)
            {
                var data = _skillLevels[i];
                list.Add(new SerializedSkillTableRow(
                    _id, null, null, null
                    , i + 1
                    , SoSerializeHelper.SerializeAssetReference(data.skillDataRef)
                    , data.amount, data.cooldown));
            }

            return list;
        }

        public void ApplyFromSerializedDto(List<SerializedSkillTableRow> serializedDto)
        {
            _skillLevels.Clear();
            foreach (var dto in serializedDto)
            {
                _id = dto.Id;
                _skillName = !string.IsNullOrEmpty(dto.SkillName) ? SoSerializeHelper.DeserializeLocalizedString(dto.SkillName) : _skillName;
                _skillDescription = !string.IsNullOrEmpty(dto.SkillDescription) ? SoSerializeHelper.DeserializeLocalizedString(dto.SkillDescription) : _skillDescription;
                _skillIconRef = string.IsNullOrEmpty(dto.SkillIconKey) ? _skillIconRef : SoSerializeHelper.DeserializeAssetReference<Sprite>(dto.SkillIconKey);
                if (dto.Level != null)
                {
                    var skillRef = SoSerializeHelper.DeserializeAssetReference<SkillData>(dto.SkillDataKey);
                    _skillLevels.Add(new SkillLevelData(skillRef, dto.Amount ?? 0, dto.Cooldown ?? 0.0f));
                }
                    
            }
        }
    }
}

