using UnityEngine;

namespace DungeonShooter
{
    /// <summary>
    /// 이동, 공격 등 행동 결정에 필요한 입력 정보.
    /// 플레이어는 키 입력, 적은 AI 컴포넌트로 조작한다.
    /// </summary>
    public interface IEntityInputContext
    {
        public Vector2 LastMoveDirection { get; }
        public Vector2 MoveInput { get; set; }
        /// <summary>사용할 액티브 스킬의 테이블 엔트리 ID. 0이면 스킬 입력 없음.</summary>
        public int SkillInput { get; set; }
    }
    
    public class EntityInputContext : IEntityInputContext
    {
        public Vector2 LastMoveDirection { get; private set; }

        public Vector2 MoveInput
        {
            get => _moveInput;
            set
            {
                if (value != Vector2.zero) 
                    LastMoveDirection = value.normalized;
                
                _moveInput = value;
            }
        }

        private Vector2 _moveInput;
        /// <summary>사용할 액티브 스킬의 테이블 엔트리 ID. 0이면 스킬 입력 없음.</summary>
        public int SkillInput { get; set; }
    }
}