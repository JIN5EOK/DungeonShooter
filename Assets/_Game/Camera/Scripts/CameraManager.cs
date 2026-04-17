using Cysharp.Threading.Tasks;
using Unity.Cinemachine;
using UnityEngine;

namespace DungeonShooter
{
    public class CameraManager : ICameraManager
    {
        private readonly IResourceProvider _resourceProvider;
        private CinemachineCamera _chaseCamera;

        public CameraManager(IResourceProvider resourceProvider)
        {
            _resourceProvider = resourceProvider;
        }

        public async UniTask BindAsync(Transform target)
        {
            if (target == null)
                return;

            if (_chaseCamera == null)
            {
                var address = CameraTrackType.PlayerChaseCamera.ToString();
                var instance = await _resourceProvider.GetInstanceAsync(address);
                if (instance == null)
                    return;

                _chaseCamera = instance.GetComponent<CinemachineCamera>();
            }

            if (_chaseCamera != null)
            {
                _chaseCamera.Target.TrackingTarget = target;
            }
        }
    }
}

