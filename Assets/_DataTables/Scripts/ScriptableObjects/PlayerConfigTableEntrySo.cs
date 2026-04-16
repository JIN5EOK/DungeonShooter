using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace DungeonShooter
{
    /// <summary>
    /// 플레이어 캐릭터 설정 테이블 엔트리
    /// CSV 등 테이블을 통해 편집되는 플레이어 캐릭터 정보
    /// </summary>
    [Serializable]
    [CreateAssetMenu(menuName = "DungeonShooter/DataTables/PlayerConfig")]
    public class PlayerTableEntrySo : ScriptableObject, ISerializableObject<SerializedPlayerConfigTableDTo>
    {
        public int Id { get; set; }

        /// <summary>플레이어 캐릭터 이름 (StringTextTableEntry.Id)</summary>
        public int NameId { get; set; }

        /// <summary>플레이어 캐릭터 설명 (StringTextTableEntry.Id)</summary>
        public int DescriptionId { get; set; }
        
        /// <summary> 플레이어 게임오브젝트 어드레서블 주소 </summary>
        public AssetReferenceGameObject GameObjectRef { get; set; }
        
        /// <summary> 1번 액티브 스킬 </summary>
        public AssetReferenceT<SkillTableEntrySo> Skill1Ref { get; set; }
        
        /// <summary> 2번 액티브 스킬 </summary>
        public AssetReferenceT<SkillTableEntrySo> Skill2Ref { get; set; }
        
        /// <summary> 기본 스탯 </summary>
        public StatsDto Stats { get; set; }

        /// <summary> 기본적으로 지닐 SkillTableEntry.Id 리스트</summary>
        public List<SkillTableEntrySo> AcquirableSkills { get; set; } = new();

        public List<SerializedPlayerConfigTableDTo> CreateSerializedDto()
        {
            throw new NotImplementedException();
        }

        public void ApplyFromSerializedDto(List<SerializedPlayerConfigTableDTo> serializedDto)
        {
            throw new NotImplementedException();
        }
    }
}
