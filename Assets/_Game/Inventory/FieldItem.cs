using UnityEngine;
using VContainer;

namespace DungeonShooter
{
    /// <summary>
    /// 필드에 떨어져 있는 아이템
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class FieldItem : MonoBehaviour, IInteractable
    {
        private Item item;
        private Inventory _inventory;
        
        public bool CanInteract => _canInteract;
        private bool _canInteract = true;
        
        public Item Item => item;
        
        [Inject]
        public void Construct(Inventory inventory)
        {
            _inventory = inventory;
        }

        /// <summary>
        /// 팩토리에서 풀/신규 생성 시 아이템과 인벤토리를 한 번에 설정합니다.
        /// </summary>
        public void Initialize(Item itemToSet, Inventory inventory)
        {
            _inventory = inventory;
            SetItem(itemToSet);
        }

        /// <summary>
        /// 아이템만 설정합니다. (인벤토리는 이미 주입된 경우) 스프라이트렌더러에는 아이템 아이콘을 적용합니다.
        /// </summary>
        public void SetItem(Item itemToSet)
        {
            item = itemToSet;
            ApplyItemIcon();
        }

        private void ApplyItemIcon()
        {
            var spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();

            spriteRenderer.sprite = item != null ? item.Icon : null;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag(GameTags.Player))
                return;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!other.CompareTag(GameTags.Player))
                return;
        }

        public void Interact()
        {
            if (!CanInteract)
                return;

            if (_inventory == null)
            {
                LogHandler.LogWarning<FieldItem>("Inventory가 주입되지 않았습니다.");
                return;
            }

            if (item == null)
            {
                LogHandler.LogWarning<FieldItem>("아이템이 설정되지 않았습니다.");
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
        /// 아이템을 수집한 후 처리합니다. 풀링 오브젝트면 풀 반환, 아니면 파괴합니다.
        /// </summary>
        private void OnCollected()
        {
            var poolable = GetComponent<PoolableComponent>();
            if (poolable != null)
                poolable.Release();
            else
                Destroy(gameObject);
        }
    }
}

