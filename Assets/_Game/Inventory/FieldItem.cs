using UnityEngine;
using DungeonShooter;
using VContainer;

namespace DungeonShooter
{
    /// <summary>
    /// 필드에 떨어져 있는 아이템
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class FieldItem : MonoBehaviour, IInteractable
    {
        [Header("아이템 설정")]
        [Tooltip("아이템 인스턴스 (런타임에 설정)")]
        [SerializeField] private ItemBase item;

        [Header("상호작용 설정")]
        [Tooltip("상호작용 가능한 거리")]
        [SerializeField] private float interactionRange = 2f;

        [Tooltip("상호작용 가능 시 표시할 UI (선택 사항)")]
        [SerializeField] private GameObject interactionPrompt;

        private Collider2D _interactionCollider;
        private bool _canInteract = true;
        private Inventory _inventory;

        public bool CanInteract => _canInteract;
        public ItemBase Item => item;

        private void Awake()
        {
            _interactionCollider = GetComponent<Collider2D>();
            _interactionCollider.isTrigger = true;

            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(false);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag(GameTags.Player))
                return;

            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(true);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
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

        /// <summary>
        /// 아이템을 설정합니다. (런타임에 호출)
        /// </summary>
        public void SetItem(ItemBase itemToSet)
        {
            item = itemToSet;
        }

        public void Interact()
        {
            if (!CanInteract)
                return;

            if (_inventory == null)
            {
                Debug.LogWarning($"[{nameof(FieldItem)}] Inventory가 주입되지 않았습니다.");
                return;
            }

            if (item == null)
            {
                Debug.LogWarning($"[{nameof(FieldItem)}] 아이템이 설정되지 않았습니다.");
                return;
            }

            AddToInventory();
        }

        /// <summary>
        /// 아이템을 인벤토리에 추가합니다.
        /// </summary>
        private void AddToInventory()
        {
            _inventory.AddItem(item);
            OnCollected();
        }

        /// <summary>
        /// 아이템을 수집한 후 처리합니다.
        /// </summary>
        private void OnCollected()
        {
            _canInteract = false;
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(false);
            }
            Destroy(gameObject);
        }
    }
}

