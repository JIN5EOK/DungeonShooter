using UnityEngine;

namespace DungeonShooter
{
    /// <summary>
    /// 엔티티의 구르기(회피)를 담당하는 컴포넌트 인터페이스.
    /// </summary>
    public interface IDashComponent
    {
        public bool IsDashing { get; }
        public bool IsReady { get; }
        public void StartDash(Vector2 direction);
        public float GetCooldownPercent();
        public float GetRemainingCooldown();
    }
}
