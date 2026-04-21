using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Localization;
using UnityEngine.Serialization;

namespace DungeonShooter
{
    /// <summary>
    /// 플레이어 캐릭터 설정 테이블 엔트리
    /// CSV 등 테이블을 통해 편집되는 플레이어 캐릭터 정보
    /// </summary>
    [Serializable]
    [CreateAssetMenu(menuName = "DungeonShooter/DataTables/PlayerConfig")]
    public sealed class PlayerConfigSo : ScriptableObject, ITableEntry, ISerializableObject<PlayerConfigDto>
    {
        public int Id => _id;

        /// <summary>플레이어 캐릭터 이름 (StringTextTableEntry.Id)</summary>
        public string NameId => _name.GetLocalizedString();

        /// <summary>플레이어 캐릭터 설명 (StringTextTableEntry.Id)</summary>
        public string DescriptionId => _description.GetLocalizedString();
        
        /// <summary> 플레이어 게임오브젝트 어드레서블 주소 </summary>
        public AssetReferenceGameObject GameObjectRef => _gameObjectRef;
        
        /// <summary> 1번 액티브 스킬 </summary>
        public SkillTableEntrySo Skill1 => skill1;

        /// <summary> 2번 액티브 스킬 </summary>
        public SkillTableEntrySo Skill2 => skill2;

        /// <summary> 기본 스탯 </summary>
        public StatsDto Stats => _stats;

        public List<SkillTableEntrySo> Skills => _skills;
        
        
        [TextArea][SerializeField] private string _memo;

        [SerializeField] private int _id;
        [SerializeField] private LocalizedString _name;
        [SerializeField] private LocalizedString _description;
        [SerializeField] private AssetReferenceGameObject _gameObjectRef;
        [SerializeField] private SkillTableEntrySo skill1;
        [SerializeField] private SkillTableEntrySo skill2;
        [SerializeField] private StatsDto _stats;
        [SerializeField] private List<SkillTableEntrySo> _skills;
        public List<PlayerConfigDto> CreateSerializedDto()
        {
            var stats = _stats ?? new StatsDto();
            var skillIds = new List<string>();
            if (_skills != null)
                foreach (var s in _skills)
                    if (s != null)
                        skillIds.Add(s.Id.ToString());
            var skills = SoSerializeHelper.SerializeStrings(skillIds);
            return new List<PlayerConfigDto>
            {
                new(
                    _id,
                    SoSerializeHelper.SerializeLocalizedString(_name),
                    SoSerializeHelper.SerializeLocalizedString(_description),
                    _gameObjectRef != null ? SoSerializeHelper.SerializeAssetReference(_gameObjectRef) : string.Empty,
                    skill1 != null ? skill1.Id.ToString() : string.Empty,
                    skill2 != null ? skill2.Id.ToString() : string.Empty,
                    stats.MaxHp,
                    stats.Attack,
                    stats.Defense,
                    stats.MoveSpeed,
                    skills,
                    _memo)
            };
        }

        public void ApplyFromSerializedDto(List<PlayerConfigDto> serializedDto)
        {
            if (serializedDto == null || serializedDto.Count == 0)
                return;

            var dto = serializedDto[0];
            _memo = dto.Memo;
            _id = dto.Id;
            _name = SoSerializeHelper.DeserializeLocalizedString(dto.Name);
            _description = SoSerializeHelper.DeserializeLocalizedString(dto.Description);

            _gameObjectRef = string.IsNullOrEmpty(dto.GameObjectKey)
                ? _gameObjectRef
                : SoSerializeHelper.DeserializeAssetReferenceGameObject(dto.GameObjectKey);

            if (_stats == null)
                _stats = new StatsDto();
            _stats.Apply(dto.MaxHp, dto.Attack, dto.Defense, dto.MoveSpeed);
        }
    }
}
