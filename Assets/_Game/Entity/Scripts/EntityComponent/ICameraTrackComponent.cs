using Cysharp.Threading.Tasks;

namespace DungeonShooter
{
    /// <summary>
    /// 엔티티를 추적하는 카메라를 부착하는 컴포넌트 인터페이스.
    /// </summary>
    public interface ICameraTrackComponent
    {
        public UniTask AttachCameraAsync();
    }
}
