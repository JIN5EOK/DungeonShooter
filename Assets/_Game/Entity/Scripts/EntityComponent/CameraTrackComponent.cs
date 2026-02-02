using Cysharp.Threading.Tasks;
using Unity.Cinemachine;
using UnityEngine;
using VContainer;

namespace DungeonShooter
{
    /// <summary>
    /// 엔티티를 추적하는 카메라를 부착하는 MonoBehaviour 컴포넌트
    /// </summary>
    public class CameraTrackComponent : MonoBehaviour
    {
        // 나중에 카메라 타입 추가되면 변경 가능하도록 변경, 지금은 타입이 하나라 이렇게
        private CameraTrackType _cameraTrackType = CameraTrackType.PlayerChaseCamera;
        private ISceneResourceProvider _sceneResourceProvider;

        [Inject]
        private void Construct(ISceneResourceProvider sceneResourceProvider)
        {
            _sceneResourceProvider = sceneResourceProvider;
        }

        /// <summary>
        /// 설정된 카메라 타입에 해당하는 추적 카메라를 로드해 자식으로 추가합니다.
        /// </summary>
        public async UniTask AttachCameraAsync()
        {
            var address = _cameraTrackType.ToString();
            if (string.IsNullOrEmpty(address))
            {
                Debug.LogWarning($"{nameof(CameraTrackComponent)}: 지원하지 않는 카메라 타입입니다. {_cameraTrackType}");
                return;
            }

            var instance = await _sceneResourceProvider.GetInstanceAsync(address);
            if (instance == null)
            {
                Debug.LogWarning($"{nameof(CameraTrackComponent)}: 리소스를 찾을 수 없습니다. 주소: {address}");
                return;
            }

            instance.transform.SetParent(transform);
            instance.GetComponent<CinemachineCamera>().Target.TrackingTarget = transform;
        }
    }
}
