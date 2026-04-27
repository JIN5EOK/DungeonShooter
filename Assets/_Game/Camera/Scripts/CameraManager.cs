using System;
using Cysharp.Threading.Tasks;
using Unity.Cinemachine;
using UnityEngine;

namespace DungeonShooter
{
    public class CameraManager : MonoBehaviour, ICameraManager
    {
        [SerializeField] private CinemachineCamera _chaseCamera;

        private void Awake()
        {
            if (_chaseCamera == null)
            {
                _chaseCamera = GetComponent<CinemachineCamera>();
            }
        }

        public void SetTarget(Transform target)
        {
            _chaseCamera.Target.TrackingTarget = target;
        }
    }
}

