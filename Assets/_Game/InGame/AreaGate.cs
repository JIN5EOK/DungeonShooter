using System.Collections;
using UnityEngine;

/// <summary>
/// 플레이어의 출입을 통제하는 차단막.
/// Open() 메서드가 호출되면 플레이어가 통과할 수 있게 됩니다.
/// </summary>
[RequireComponent(typeof(Collider2D))]
[DisallowMultipleComponent]
public class AreaGate : MonoBehaviour
{
    [Header("시각적 설정")]
    [Tooltip("클리어 시 비활성화될 시각 요소들")]
    [SerializeField] private GameObject[] visualElements;
    
    [Tooltip("클리어 시 재생할 이펙트 (선택 사항)")]
    [SerializeField] private ParticleSystem clearEffect;
    
    [Tooltip("클리어 시 페이드 아웃 시간")]
    [SerializeField] private float fadeOutDuration = 0.5f;
    
    [Header("디버그")]
    [SerializeField] private bool showDebugInfo = true;

    private Collider2D _gateCollider;
    private bool _isOpen = false;
    private SpriteRenderer[] _spriteRenderers;

    private void Awake()
    {
        _gateCollider = GetComponent<Collider2D>();
        _gateCollider.isTrigger = false; // 플레이어를 막기 위해 일반 Collider 사용
        
        // 시각 요소에서 SpriteRenderer 찾기
        if (visualElements != null && visualElements.Length > 0)
        {
            System.Collections.Generic.List<SpriteRenderer> renderers = new System.Collections.Generic.List<SpriteRenderer>();
            foreach (GameObject element in visualElements)
            {
                if (element != null)
                {
                    SpriteRenderer sr = element.GetComponent<SpriteRenderer>();
                    if (sr != null)
                    {
                        renderers.Add(sr);
                    }
                }
            }
            _spriteRenderers = renderers.ToArray();
        }
        else
        {
            // visualElements가 없으면 자신의 SpriteRenderer 사용
            _spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        }
    }

    /// <summary>
    /// 게이트를 엽니다 (플레이어 통과 가능)
    /// </summary>
    public void Open()
    {
        if (_isOpen) return;

        _isOpen = true;

        if (showDebugInfo)
        {
            Debug.Log($"[AreaGate] {gameObject.name} 열림!");
        }

        // Collider 비활성화 (플레이어 통과 가능)
        _gateCollider.enabled = false;

        // 시각적 효과
        StartCoroutine(PlayOpenEffect());
    }

    /// <summary>
    /// 게이트를 닫습니다 (플레이어 통과 불가)
    /// </summary>
    public void Close()
    {
        if (!_isOpen) return;

        _isOpen = false;

        if (showDebugInfo)
        {
            Debug.Log($"[AreaGate] {gameObject.name} 닫힘!");
        }

        // Collider 활성화 (플레이어 차단)
        _gateCollider.enabled = true;

        // 시각 요소 활성화
        if (visualElements != null)
        {
            foreach (GameObject element in visualElements)
            {
                if (element != null)
                {
                    element.SetActive(true);
                }
            }
        }
    }

    /// <summary>
    /// 게이트 열림 시각 효과 재생
    /// </summary>
    private IEnumerator PlayOpenEffect()
    {
        // 파티클 이펙트 재생
        if (clearEffect != null)
        {
            clearEffect.Play();
        }

        // 페이드 아웃 효과
        if (_spriteRenderers != null && _spriteRenderers.Length > 0 && fadeOutDuration > 0)
        {
            float elapsedTime = 0f;
            Color[] originalColors = new Color[_spriteRenderers.Length];
            
            for (int i = 0; i < _spriteRenderers.Length; i++)
            {
                if (_spriteRenderers[i] != null)
                {
                    originalColors[i] = _spriteRenderers[i].color;
                }
            }

            while (elapsedTime < fadeOutDuration)
            {
                elapsedTime += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeOutDuration);

                for (int i = 0; i < _spriteRenderers.Length; i++)
                {
                    if (_spriteRenderers[i] != null)
                    {
                        Color color = originalColors[i];
                        color.a = alpha;
                        _spriteRenderers[i].color = color;
                    }
                }

                yield return null;
            }
        }

        // 시각 요소 비활성화
        if (visualElements != null)
        {
            foreach (GameObject element in visualElements)
            {
                if (element != null)
                {
                    element.SetActive(false);
                }
            }
        }
        else
        {
            // visualElements가 없으면 자신 비활성화
            gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 게이트 열림 상태 반환
    /// </summary>
    public bool IsOpen => _isOpen;
}
