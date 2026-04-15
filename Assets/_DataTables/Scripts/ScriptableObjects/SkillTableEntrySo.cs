using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Serialization;

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
        
        [SerializeField] private int _id;
        [SerializeField] private int _skillNameId;
        [SerializeField] private int _skillDescriptionId;
        [SerializeField] private AssetReferenceT<Sprite> _skillIconRef;
        [SerializeField] private List<SkillLevelData> _skillLevels;

        public int Id
        {
            get => _id;
            set => _id = value;
        }

        public List<SerializedSkillTableRow> CreateSerializedDto()
        {
            var list = new List<SerializedSkillTableRow>();
            list.Add(new SerializedSkillTableRow(_id, _skillNameId, _skillDescriptionId, _skillIconRef.RuntimeKey.ToString(), null, null, null, null));
            for (var i = 0; i < _skillLevels.Count; i++)
            {
                var data = _skillLevels[i];
                list.Add(new SerializedSkillTableRow(_id, null, null, null, i + 1,data.skillDataRef.RuntimeKey.ToString()
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
                _skillNameId = dto.SkillNameId ?? _skillNameId;
                _skillDescriptionId = dto.SkillDescriptionId ?? _skillDescriptionId;
                _skillIconRef = string.IsNullOrEmpty(dto.SkillIconKey) ? _skillIconRef : new AssetReferenceT<Sprite>(dto.SkillIconKey);
                
                if(dto.Level != null)
                    _skillLevels.Add(new SkillLevelData(new AssetReferenceT<SkillData>(dto.SkillDataKey), dto.Amount ?? 0, dto.Cooldown ?? 0.0f));
            }
        }
    }
}

