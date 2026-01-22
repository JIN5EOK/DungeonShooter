using System;
using UnityEngine;
using DungeonShooter;
using VContainer;

namespace DungeonShooter
{
    /// <summary>
    /// 플레이어가 접촉하면 코인이 흡수되어 사라지는 단순 수집 오브젝트.
    /// 실제 보상 처리(골드 증가 등)는 플레이어 측에서 OnCollected 이벤트를 구독해 구현한다.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    [DisallowMultipleComponent]
    public class CoinPickup : MonoBehaviour
    {
    [Tooltip("흡수 감지 후 오브젝트를 파괴하기까지의 지연 시간(연출용).")]
    [SerializeField] private float destroyDelay = 0.05f;

    [Header("코인 설정")]
    [SerializeField, Min(1)] private int coinValue = 1;

    [Tooltip("흡수 시 비활성화할 시각 요소")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Tooltip("흡수 시 재생할 파티클 (선택 사항)")]
    [SerializeField] private ParticleSystem collectEffect;

    private CoinInventory _coinInventory;
    /// <summary>
    /// 플레이어가 코인을 획득했을 때 알리는 이벤트. 
    /// PlayerProto 또는 플레이어 루트 GameObject를 전달한다.
    /// </summary>
    public event Action<GameObject> OnCollected;

    private Collider2D _coinCollider;
    private bool _collected;
    
    private void Awake()
    {
        _coinCollider = GetComponent<Collider2D>();
        _coinCollider.isTrigger = true;

        // SpriteRenderer를 수동으로 연결하지 않았다면 자동 참조
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
    }

    [Inject]
    public void Construct(CoinInventory coinInventory)
    {
        _coinInventory = coinInventory;
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_collected)
            return;

        if (!other.CompareTag(GameTags.Player))
            return;

        HandleCollect(other.gameObject);
    }

    private void HandleCollect(GameObject player)
    {
        _collected = true;
        _coinCollider.enabled = false;

        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }

        if (collectEffect != null)
        {
            collectEffect.transform.SetParent(null, worldPositionStays: true);
            collectEffect.Play();
            Destroy(collectEffect.gameObject, collectEffect.main.duration);
        }

        RewardPlayer(player);
        OnCollected?.Invoke(player);

        Destroy(gameObject, destroyDelay);
    }

    private void RewardPlayer(GameObject player)
    {
        if (_coinInventory == null)
        {
            LogHandler.LogWarning<CoinPickup>("CoinInventory를 찾을 수 없어 보상을 지급하지 못했습니다.");
            return;
        }

        _coinInventory.AddCoins(coinValue);
        LogHandler.Log<CoinPickup>($"{player.name}이(가) 코인 {coinValue}개 획득! 총 {_coinInventory.CurrentCoins}");
    }
    }
}

