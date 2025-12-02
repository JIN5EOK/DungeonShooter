using UnityEngine;

/// <summary>
/// 카메라 추적 시스템
/// 플레이어나 다른 대상을 자연스럽게 따라다니거나, 원하는 위치로 이동할 수 있습니다.
/// </summary>
public class CameraController : MonoBehaviour
{
    [Header("추적 설정")]
    [Tooltip("추적할 대상 Transform (null이면 자동으로 Player 태그 찾기)")]
    [SerializeField] private Transform _target;
    
    [Tooltip("카메라 오프셋 (대상 위치 기준)")]
    [SerializeField] private Vector3 _offset = new Vector3(0, 0, -10);
    
    [Tooltip("추적 속도 (높을수록 빠르게 따라감)")]
    [SerializeField] private float _followSpeed = 5f;
    
    [Tooltip("X축 추적 활성화")]
    [SerializeField] private bool _followX = true;
    
    [Tooltip("Y축 추적 활성화")]
    [SerializeField] private bool _followY = true;

    [Header("고정 위치 모드")]
    [Tooltip("고정 위치 모드 활성화 시 추적 대신 지정된 위치로 이동")]
    [SerializeField] private bool _useFixedPosition = false;
    
    [Tooltip("고정 위치 (고정 위치 모드 활성화 시 사용)")]
    [SerializeField] private Vector3 _fixedPosition = Vector3.zero;

    private Vector3 _velocity = Vector3.zero;
    private Camera _camera;

    private void Awake()
    {
        _camera = GetComponent<Camera>();
        if (_camera == null)
        {
            _camera = Camera.main;
        }
    }

    private void Start()
    {
        // 타겟이 지정되지 않았으면 Player 태그를 가진 오브젝트 찾기
        if (_target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag(GameTags.Player);
            if (player != null)
            {
                _target = player.transform;
                Debug.Log($"[CameraController] Player 자동 찾기 완료: {player.name}");
            }
            else
            {
                Debug.LogWarning("[CameraController] Player 태그를 가진 오브젝트를 찾을 수 없습니다.");
            }
        }
    }

    private void LateUpdate()
    {
        if (_useFixedPosition)
        {
            // 고정 위치 모드: 지정된 위치로 이동
            MoveToPosition(_fixedPosition);
        }
        else if (_target != null)
        {
            // 추적 모드: 타겟을 따라다니기
            FollowTarget();
        }
    }

    /// <summary>
    /// 타겟을 추적합니다.
    /// </summary>
    private void FollowTarget()
    {
        Vector3 targetPosition = _target.position + _offset;
        Vector3 currentPosition = transform.position;

        // X, Y축 추적 여부에 따라 위치 계산
        if (!_followX) targetPosition.x = currentPosition.x;
        if (!_followY) targetPosition.y = currentPosition.y;

        // Z축은 항상 오프셋 유지
        targetPosition.z = _offset.z;

        // 부드러운 이동 (SmoothDamp 사용)
        transform.position = Vector3.SmoothDamp(
            currentPosition,
            targetPosition,
            ref _velocity,
            1f / _followSpeed
        );
    }

    /// <summary>
    /// 지정된 위치로 카메라를 이동시킵니다.
    /// </summary>
    /// <param name="position">이동할 위치</param>
    public void MoveToPosition(Vector3 position)
    {
        Vector3 targetPosition = position + _offset;
        targetPosition.z = _offset.z;

        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref _velocity,
            1f / _followSpeed
        );
    }

    /// <summary>
    /// 지정된 위치로 카메라를 즉시 이동시킵니다 (부드러운 이동 없음).
    /// </summary>
    /// <param name="position">이동할 위치</param>
    public void SetPositionImmediate(Vector3 position)
    {
        Vector3 targetPosition = position + _offset;
        targetPosition.z = _offset.z;
        transform.position = targetPosition;
        _velocity = Vector3.zero;
    }

    /// <summary>
    /// 추적 대상을 설정합니다.
    /// </summary>
    /// <param name="target">추적할 Transform (null이면 추적 중지)</param>
    public void SetTarget(Transform target)
    {
        _target = target;
        _useFixedPosition = false;
        
        if (target == null)
        {
            Debug.Log("[CameraController] 추적 대상이 null로 설정되어 추적이 중지됩니다.");
        }
        else
        {
            Debug.Log($"[CameraController] 추적 대상 변경: {target.name}");
        }
    }

    /// <summary>
    /// 고정 위치 모드를 활성화/비활성화합니다.
    /// </summary>
    /// <param name="enabled">활성화 여부</param>
    public void SetFixedPositionMode(bool enabled)
    {
        _useFixedPosition = enabled;
    }

    /// <summary>
    /// 고정 위치를 설정합니다.
    /// </summary>
    /// <param name="position">고정 위치</param>
    public void SetFixedPosition(Vector3 position)
    {
        _fixedPosition = position;
        _useFixedPosition = true;
    }

    /// <summary>
    /// 추적 속도를 설정합니다.
    /// </summary>
    /// <param name="speed">추적 속도</param>
    public void SetFollowSpeed(float speed)
    {
        _followSpeed = Mathf.Max(0.1f, speed);
    }

    /// <summary>
    /// 카메라 오프셋을 설정합니다.
    /// </summary>
    /// <param name="offset">오프셋</param>
    public void SetOffset(Vector3 offset)
    {
        _offset = offset;
    }

    /// <summary>
    /// 현재 추적 중인 대상 Transform을 반환합니다.
    /// </summary>
    public Transform GetTarget()
    {
        return _target;
    }

    /// <summary>
    /// 현재 카메라 위치를 반환합니다 (오프셋 제외).
    /// </summary>
    public Vector3 GetCameraPosition()
    {
        Vector3 pos = transform.position;
        pos.z = 0; // Z축 오프셋 제거
        return pos - _offset;
    }
}

