using System;
using System.Collections;
using UnityEngine;
using DungeonShooter;
using VContainer;
/// <summary>
/// 보물상자. 플레이어가 가까이 다가가서 상호작용 키를 누르면 열리고 보상을 지급합니다.
/// </summary>
[RequireComponent(typeof(Collider2D))]
[DisallowMultipleComponent]
public class TreasureChest : MonoBehaviour, IInteractable
{
    [Header("상호작용 설정")]
    [Tooltip("상호작용 가능한 거리")]
    [SerializeField] private float interactionRange = 2f;

    [Header("보상 설정")]
    [Tooltip("상자에서 나올 코인 수")]
    [SerializeField, Min(0)] private int coinReward = 10;
    
    [Tooltip("코인 드롭 프리팹 (선택 사항, 없으면 직접 지급)")]
    [SerializeField] private GameObject coinPrefab;

    [Header("시각적 설정")]
    [Tooltip("닫힌 상자 스프라이트")]
    [SerializeField] private Sprite closedSprite;
    
    [Tooltip("열린 상자 스프라이트")]
    [SerializeField] private Sprite openSprite;
    
    [Tooltip("상자 열림 애니메이션 시간")]
    [SerializeField] private float openAnimationDuration = 0.5f;
    
    [Tooltip("상자 열림 파티클 이펙트 (선택 사항)")]
    [SerializeField] private ParticleSystem openEffect;
    
    [Tooltip("상호작용 가능 시 표시할 UI (선택 사항)")]
    [SerializeField] private GameObject interactionPrompt;

    [Header("디버그")]
    [SerializeField] private bool showDebugInfo = true;

    private Collider2D _interactionCollider;
    private SpriteRenderer _spriteRenderer;
    private bool _isOpen = false;
    private bool _playerInRange = false;
    private CoinInventory _coinInventory;

    /// <summary>
    /// 상자가 열렸을 때 발생하는 이벤트
    /// </summary>
    public event Action<TreasureChest> OnOpened;

    private void Awake()
    {
        _interactionCollider = GetComponent<Collider2D>();
        _interactionCollider.isTrigger = true;
        
        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (_spriteRenderer == null)
        {
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        // 닫힌 상태로 초기화
        if (_spriteRenderer != null && closedSprite != null)
        {
            _spriteRenderer.sprite = closedSprite;
        }

        // 상호작용 프롬프트 초기화
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
    }

    [Inject]
    public void Construct(CoinInventory coinInventory)
    {
        _coinInventory = coinInventory;
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(GameTags.Player))
        {
            _playerInRange = true;

            // PlayerProto에 상호작용 가능한 오브젝트로 등록
            Player player = other.GetComponent<Player>();
            if (player != null)
            {
                player.RegisterInteractable(this);
            }

            // 상호작용 프롬프트 표시
            if (interactionPrompt != null && !_isOpen)
            {
                interactionPrompt.SetActive(true);
            }

            if (showDebugInfo)
            {
                Debug.Log($"[TreasureChest] {gameObject.name}: 플레이어가 범위 내에 들어왔습니다.");
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(GameTags.Player))
        {
            _playerInRange = false;

            // PlayerProto에서 상호작용 가능한 오브젝트에서 제거
            Player player = other.GetComponent<Player>();
            if (player != null)
            {
                player.UnregisterInteractable(this);
            }

            // 상호작용 프롬프트 숨기기
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(false);
            }

            if (showDebugInfo)
            {
                Debug.Log($"[TreasureChest] {gameObject.name}: 플레이어가 범위를 벗어났습니다.");
            }
        }
    }

    /// <summary>
    /// 상자를 엽니다.
    /// </summary>
    public void OpenChest()
    {
        if (_isOpen) return;

        _isOpen = true;
        _interactionCollider.enabled = false; // 더 이상 상호작용 불가

        if (showDebugInfo)
        {
            Debug.Log($"[TreasureChest] {gameObject.name}: 상자가 열렸습니다!");
        }

        // 상호작용 프롬프트 숨기기
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }

        // 시각적 효과
        StartCoroutine(PlayOpenAnimation());

        // 보상 지급
        GiveReward();

        // 이벤트 발생
        OnOpened?.Invoke(this);
    }

    /// <summary>
    /// 상자 열림 애니메이션 재생
    /// </summary>
    private IEnumerator PlayOpenAnimation()
    {
        // 스프라이트 변경
        if (_spriteRenderer != null && openSprite != null)
        {
            _spriteRenderer.sprite = openSprite;
        }

        // 파티클 이펙트 재생
        if (openEffect != null)
        {
            openEffect.Play();
        }

        // 애니메이션 시간 대기 (추가 효과를 위해)
        yield return new WaitForSeconds(openAnimationDuration);
    }

    /// <summary>
    /// 보상 지급
    /// </summary>
    private void GiveReward()
    {
        if (coinReward <= 0) return;

        // 코인 프리팹이 있으면 드롭, 없으면 직접 지급
        if (coinPrefab != null)
        {
            // 코인을 상자 주변에 드롭
            for (int i = 0; i < coinReward; i++)
            {
                Vector2 dropPosition = (Vector2)transform.position + UnityEngine.Random.insideUnitCircle * 1.5f;
                Instantiate(coinPrefab, dropPosition, Quaternion.identity);
            }

            if (showDebugInfo)
            {
                Debug.Log($"[TreasureChest] {gameObject.name}: 코인 {coinReward}개를 드롭했습니다.");
            }
        }
        else
        {
            // 직접 지급
            if (_coinInventory != null)
            {
                _coinInventory.AddCoins(coinReward);
                if (showDebugInfo)
                {
                    Debug.Log($"[TreasureChest] {gameObject.name}: 코인 {coinReward}개를 지급했습니다. 총 {_coinInventory.CurrentCoins}개");
                }
            }
            else
            {
                Debug.LogWarning($"[TreasureChest] {gameObject.name}: CoinInventory를 찾을 수 없어 보상을 지급하지 못했습니다.");
            }
        }
    }

    /// <summary>
    /// 상자가 열려있는지 여부
    /// </summary>
    public bool IsOpen => _isOpen;

    /// <summary>
    /// 플레이어가 범위 내에 있는지 여부
    /// </summary>
    public bool IsPlayerInRange => _playerInRange;

    // IInteractable 구현
    public bool CanInteract => !_isOpen && _playerInRange;

    public void Interact()
    {
        if (CanInteract)
        {
            OpenChest();
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!showDebugInfo) return;

        // 상호작용 범위 시각화
        Gizmos.color = _playerInRange ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}

