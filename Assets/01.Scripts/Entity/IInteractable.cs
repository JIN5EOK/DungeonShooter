namespace DungeonShooter
{
    /// <summary>
    /// 상호작용 가능한 오브젝트를 나타내는 인터페이스
    /// </summary>
    public interface IInteractable
    {
        /// <summary>
        /// 상호작용을 수행합니다.
        /// </summary>
        public void Interact();
        /// <summary>
        /// 상호작용 가능한지 여부
        /// </summary>
        public bool CanInteract { get; }
    }
}

