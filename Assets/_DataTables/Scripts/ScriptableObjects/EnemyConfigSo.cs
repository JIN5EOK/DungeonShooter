using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Localization;

namespace DungeonShooter
{
    /// <summary>
    /// 적 캐릭터 설정 테이블 엔트리 (SO 기반)
    /// CSV 등 테이블을 통해 편집되는 적 캐릭터 정보
    /// </summary>
    [Serializable]
    [CreateAssetMenu(menuName = "DungeonShooter/DataTables/EnemyConfig")]
    public sealed class EnemyConfigSo : ScriptableObject, ISerializableObject<EnemyConfigDto>
    {
        public int Id => _id;

        public string NameId => _name.GetLocalizedString();

        /// <summary>적 프리팹 어드레서블 레퍼런스</summary>
        public AssetReferenceGameObject GameObjectRef => _gameObjectRef;

        /// <summary>AI 동작 타입</summary>
        public AssetReferenceT<AiBTBase> AIType => _aiType;

        /// <summary>기본 스탯</summary>
        public StatsDto Stats => _stats;

        /// <summary>처치 시 획득 경험치</summary>
        public int Exp => _exp;

        /// <summary>활성 스킬 레퍼런스 목록</summary>
        public List<SkillTableEntrySo> ActiveSkills => _activeSkills;

        [TextArea][SerializeField] private string _memo;

        [SerializeField] private int _id;
        [SerializeField] private LocalizedString _name;
        [SerializeField] private AssetReferenceGameObject _gameObjectRef;
        [SerializeField] private AssetReferenceT<AiBTBase> _aiType;
        [SerializeField] private StatsDto _stats;
        [SerializeField] private int _exp;
        [SerializeField] private List<SkillTableEntrySo> _activeSkills;

        public List<EnemyConfigDto> CreateSerializedDto()
        {
            var stats = _stats ?? new StatsDto();
            var skillIds = new List<string>();
            if (_activeSkills != null)
                foreach (var s in _activeSkills)
                    if (s != null)
                        skillIds.Add(s.Id.ToString());
            var activeSkills = SoSerializeHelper.SerializeStrings(skillIds);

            return new List<EnemyConfigDto>
            {
                new(
                    _id,
                    SoSerializeHelper.SerializeLocalizedString(_name),
                    _gameObjectRef != null ? SoSerializeHelper.SerializeAssetReference(_gameObjectRef) : string.Empty,
                    _aiType != null ? SoSerializeHelper.SerializeAssetReference(_aiType) : string.Empty,
                    stats.MaxHp,
                    stats.Attack,
                    stats.Defense,
                    stats.MoveSpeed,
                    _exp,
                    activeSkills,
                    _memo)
            };
        }

        public void ApplyFromSerializedDto(List<EnemyConfigDto> serializedDto)
        {
            if (serializedDto == null || serializedDto.Count == 0)
                return;

            var dto = serializedDto[0];
            _memo = dto.Memo;
            _id = dto.Id;
            _name = SoSerializeHelper.DeserializeLocalizedString(dto.Name);
            _aiType = SoSerializeHelper.DeserializeAssetReference<AiBTBase>(dto.AITypeKey);
            _exp = dto.Exp;

            _gameObjectRef = string.IsNullOrEmpty(dto.GameObjectKey)
                ? _gameObjectRef
                : SoSerializeHelper.DeserializeAssetReferenceGameObject(dto.GameObjectKey);

            if (_stats == null)
                _stats = new StatsDto();
            _stats.Apply(dto.MaxHp, dto.Attack, dto.Defense, dto.MoveSpeed);
        }
    }
}

