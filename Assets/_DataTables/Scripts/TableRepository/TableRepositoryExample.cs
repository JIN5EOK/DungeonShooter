using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace DungeonShooter
{
    /// <summary>
    /// ITableRepository 사용 예시
    /// 실제 게임에서는 Inventory, SkillComponent 등에서 이런 식으로 사용
    /// </summary>
    public class TableRepositoryExample : ITickable
    {
        private readonly ITableRepository _tableRepository;

        [Inject]
        public TableRepositoryExample(ITableRepository tableRepository)
        {
            Debug.Log("Inject");
            _tableRepository = tableRepository;
        }

        /// <summary>
        /// 스킬 테이블 엔트리 조회 예시
        /// </summary>
        public void GetSkillExample()
        {
            // ID 1인 스킬 조회
            var skillEntry = _tableRepository.GetTableEntry<SkillTableEntry>(1);
            Debug.Log(skillEntry is null);
            if (skillEntry != null)
            {
                LogHandler.Log<TableRepositoryExample>($"스킬: {skillEntry.Id}, 쿨다운: {skillEntry.Cooldown}");
                
                // 방법 1: 자주 사용되는 수치는 직접 필드로 접근
                LogHandler.Log<TableRepositoryExample>($"데미지: {skillEntry.Damage}, 회복량: {skillEntry.Heal}");
                
                // 방법 2: 확장 가능한 수치는 키-값 방식으로 조회
                var range = skillEntry.GetFloatAmount("range");
                var speed = skillEntry.GetFloatAmount("speed");
                
                LogHandler.Log<TableRepositoryExample>($"범위: {range}, 속도: {speed}");
            }
        }

        /// <summary>
        /// 아이템 테이블 엔트리 조회 예시
        /// </summary>
        public void GetItemExample()
        {
            // ID 10인 아이템 조회
            var itemEntry = _tableRepository.GetTableEntry<ItemTableEntry>(10);
            
            if (itemEntry != null)
            {
                LogHandler.Log<TableRepositoryExample>($"아이템: {itemEntry.ItemName}, 타입: {itemEntry.ItemType}");
            }
        }


        public void Tick()
        {
            if(Input.GetKeyDown(KeyCode.Space))
                GetSkillExample();
        }
    }
}
