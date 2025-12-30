using UnityEngine;

/// <summary>
/// 공격 범위를 시각적으로 표시하는 컴포넌트.
/// 원형 범위를 LineRenderer로 그립니다.
/// </summary>
public class AttackRangeVisualizer : MonoBehaviour
{
    [Header("범위 설정")]
    [SerializeField] private float radius = 1.5f;
    [SerializeField] private int segments = 32; // 원의 세밀도
    
    [Header("시각적 설정")]
    [SerializeField] private Color rangeColor = new Color(1f, 0f, 0f, 0.5f); // 반투명 빨간색
    [SerializeField] private float lineWidth = 0.1f;
    [SerializeField] private bool alwaysVisible = false; // 항상 보이기
    
    [Header("표시 조건")]
    [SerializeField] private bool showOnAttack = true; // 공격 시 표시
    [SerializeField] private float showDuration = 0.3f; // 표시 지속 시간
    
    private LineRenderer _lineRenderer;
    private bool _isShowing = false;
    private float _showTimer = 0f;

    private void Awake()
    {
        // LineRenderer가 없으면 자동으로 추가
        _lineRenderer = GetComponent<LineRenderer>();
        if (_lineRenderer == null)
        {
            _lineRenderer = gameObject.AddComponent<LineRenderer>();
        }
        
        SetupLineRenderer();
        DrawCircle();
        
        if (!alwaysVisible)
        {
            _lineRenderer.enabled = false;
        }
    }

    private void OnEnable()
    {
        // 활성화될 때 원 다시 그리기 (위치가 변경되었을 수 있음)
        if (_lineRenderer != null)
        {
            DrawCircle();
        }
    }

    private void Update()
    {
        if (!alwaysVisible && _isShowing)
        {
            _showTimer -= Time.deltaTime;
            if (_showTimer <= 0f)
            {
                Hide();
            }
        }
    }

    /// <summary>
    /// LineRenderer 설정
    /// </summary>
    private void SetupLineRenderer()
    {
        // 부모가 있으면 로컬 좌표계, 없으면 월드 좌표계 사용
        _lineRenderer.useWorldSpace = transform.parent == null;
        _lineRenderer.loop = true;
        _lineRenderer.startWidth = lineWidth;
        _lineRenderer.endWidth = lineWidth;
        _lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        _lineRenderer.startColor = rangeColor;
        _lineRenderer.endColor = rangeColor;
        
        // Sorting Order 설정 (Renderer를 통해)
        var renderer = _lineRenderer as Renderer;
        if (renderer != null)
        {
            renderer.sortingOrder = 10; // 다른 오브젝트 위에 표시
        }
    }

    /// <summary>
    /// 원 그리기
    /// </summary>
    private void DrawCircle()
    {
        _lineRenderer.positionCount = segments + 1;
        
        // useWorldSpace에 따라 좌표계 결정
        var useWorld = _lineRenderer.useWorldSpace;
        var center = useWorld ? transform.position : Vector3.zero;
        
        for (int i = 0; i <= segments; i++)
        {
            var angle = (i / (float)segments) * 360f * Mathf.Deg2Rad;
            var x = Mathf.Cos(angle) * radius;
            var y = Mathf.Sin(angle) * radius;
            
            var pos = center + new Vector3(x, y, 0);
            _lineRenderer.SetPosition(i, pos);
        }
    }

    /// <summary>
    /// 범위 표시
    /// </summary>
    public void Show()
    {
        if (alwaysVisible) return;
        
        _isShowing = true;
        _showTimer = showDuration;
        _lineRenderer.enabled = true;
    }

    /// <summary>
    /// 범위 숨기기
    /// </summary>
    public void Hide()
    {
        if (alwaysVisible) return;
        
        _isShowing = false;
        _lineRenderer.enabled = false;
    }

    /// <summary>
    /// 범위 반경 설정
    /// </summary>
    public void SetRadius(float newRadius)
    {
        radius = newRadius;
        DrawCircle();
    }

    /// <summary>
    /// 위치 설정 후 원 다시 그리기 (useWorldSpace = true일 때 필요)
    /// </summary>
    public void UpdatePosition()
    {
        if (_lineRenderer != null && _lineRenderer.useWorldSpace)
        {
            DrawCircle();
        }
    }

    /// <summary>
    /// 범위 색상 설정
    /// </summary>
    public void SetColor(Color color)
    {
        rangeColor = color;
        _lineRenderer.startColor = color;
        _lineRenderer.endColor = color;
    }

    /// <summary>
    /// 현재 반경 반환
    /// </summary>
    public float GetRadius() => radius;
}

