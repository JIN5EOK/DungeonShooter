using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Localization;

namespace DungeonShooter
{
    /// <summary>
    /// 플레이어 캐릭터 설정 테이블 엔트리
    /// CSV 등 테이블을 통해 편집되는 플레이어 캐릭터 정보
    /// </summary>
    [Serializable]
    [CreateAssetMenu(menuName = "DungeonShooter/DataTables/PlayerConfig")]
    public sealed class PlayerConfigSo : ScriptableObject, ISerializableObject<PlayerConfigDto>
    {
        public int Id => _id;

        /// <summary>플레이어 캐릭터 이름 (StringTextTableEntry.Id)</summary>
        public string NameId => _name.GetLocalizedString();

        /// <summary>플레이어 캐릭터 설명 (StringTextTableEntry.Id)</summary>
        public string DescriptionId => _description.GetLocalizedString();
        
        /// <summary> 플레이어 게임오브젝트 어드레서블 주소 </summary>
        public AssetReferenceGameObject GameObjectRef => _gameObjectRef;
        
        /// <summary> 1번 액티브 스킬 </summary>
        public AssetReferenceT<SkillTableEntrySo> Skill1Ref => _skill1Ref;
        
        /// <summary> 2번 액티브 스킬 </summary>
        public AssetReferenceT<SkillTableEntrySo> Skill2Ref => _skill2Ref;
        
        /// <summary> 기본 스탯 </summary>
        public StatsDto Stats => _stats;

        public List<AssetReferenceT<SkillTableEntrySo>> Skills => _skills;
        
        
        [TextArea][SerializeField] private string _memo;

        [SerializeField] private int _id;
        [SerializeField] private LocalizedString _name;
        [SerializeField] private LocalizedString _description;
        [SerializeField] private AssetReferenceGameObject _gameObjectRef;
        [SerializeField] private AssetReferenceT<SkillTableEntrySo> _skill1Ref;
        [SerializeField] private AssetReferenceT<SkillTableEntrySo> _skill2Ref;
        [SerializeField] private StatsDto _stats;
        [SerializeField] private List<AssetReferenceT<SkillTableEntrySo>> _skills;
        public List<PlayerConfigDto> CreateSerializedDto()
        {
            var stats = _stats ?? new StatsDto();
            var skills = _skills != null ? SoSerializeHelper.SerializeAssetReferences(_skills) : string.Empty;
            return new List<PlayerConfigDto>
            {
                new(
                    _id,
                    SoSerializeHelper.SerializeLocalizedString(_name),
                    SoSerializeHelper.SerializeLocalizedString(_description),
                    _gameObjectRef != null ? SoSerializeHelper.SerializeAssetReference(_gameObjectRef) : string.Empty,
                    _skill1Ref != null ? SoSerializeHelper.SerializeAssetReference(_skill1Ref) : string.Empty,
                    _skill2Ref != null ? SoSerializeHelper.SerializeAssetReference(_skill2Ref) : string.Empty,
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

            _skill1Ref = string.IsNullOrEmpty(dto.Skill1Key)
                ? _skill1Ref
                : SoSerializeHelper.DeserializeAssetReference<SkillTableEntrySo>(dto.Skill1Key);

            _skill2Ref = string.IsNullOrEmpty(dto.Skill2Key)
                ? _skill2Ref
                : SoSerializeHelper.DeserializeAssetReference<SkillTableEntrySo>(dto.Skill2Key);

            if (_stats == null)
                _stats = new StatsDto();
            _stats.Apply(dto.MaxHp, dto.Attack, dto.Defense, dto.MoveSpeed);

            if (_skills == null)
                _skills = new List<AssetReferenceT<SkillTableEntrySo>>();

            if (!string.IsNullOrWhiteSpace(dto.Skills))
                _skills = SoSerializeHelper.DeserializeAssetReferences<SkillTableEntrySo>(dto.Skills);
        }
    }
}
