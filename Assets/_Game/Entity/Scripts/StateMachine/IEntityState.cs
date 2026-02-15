namespace DungeonShooter
{
    /// <summary>
    /// 엔티티 상태머신의 개별 상태 인터페이스
    /// </summary>
    public interface IEntityState
    {
        EntityStates States { get; }

        void Initialize(IEntityStateMachine stateMachine);
        
        /// <summary>
        /// 해당 상태로 진입할 때 호출됩니다.
        /// </summary>
        void OnEnter();

        /// <summary>
        /// 해당 상태에서 벗어날 때 호출됩니다.
        /// </summary>
        void OnExit();

        /// <summary>
        /// 매 프레임 호출됩니다.
        /// </summary>
        void OnUpdate();
    }
}
