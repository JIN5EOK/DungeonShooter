using UnityEngine;
using DungeonShooter;
using VContainer;

/// <summary>
/// 필드에 떨어져 있는 아이템의 기본 클래스
/// </summary>
[RequireComponent(typeof(Collider2D))]
public abstract class FieldItemBase : MonoBehaviour, IInteractable
{
    [Header("아이템 설정")]
    [Tooltip("아이템 데이터")]
    [SerializeField] protected ItemData itemData;

    [Header("상호작용 설정")]
    [Tooltip("상호작용 가능한 거리")]
    [SerializeField] protected float interactionRange = 2f;

    [Tooltip("상호작용 가능 시 표시할 UI (선택 사항)")]
    [SerializeField] protected GameObject interactionPrompt;

    protected Collider2D _interactionCollider;
    protected bool _canInteract = true;
    protected Inventory _inventory;

    public bool CanInteract => _canInteract;

    protected virtual void Awake()
    {
        _interactionCollider = GetComponent<Collider2D>();
        _interactionCollider.isTrigger = true;

        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
    }

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(GameTags.Player))
            return;

        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(true);
        }
    }

    protected virtual void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(GameTags.Player))
            return;

        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
    }

    [Inject]
    public void Construct(Inventory inventory)
    {
        _inventory = inventory;
    }

    public abstract void Interact();

    /// <summary>
    /// 아이템을 인벤토리에 추가합니다.
    /// </summary>
    protected abstract void AddToInventory();

    /// <summary>
    /// 아이템을 수집한 후 처리합니다.
    /// </summary>
    protected virtual void OnCollected()
    {
        _canInteract = false;
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
        Destroy(gameObject);
    }
}

