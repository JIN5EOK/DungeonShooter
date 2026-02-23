using System.Collections;
using DG.Tweening;
using Jin5eok;
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
        private const float DespawnSeconds = 15f;
        private const float DropJumpDuration = 1f;
        private const float DropJumpPower = 2f;
        private const float LandingRandomRadius = 0.25f;

        private Item item;
        private IInventory _inventory;
        private Coroutine _despawnCoroutine;

        public bool CanInteract => _canInteract;
        private bool _canInteract;
        private SpriteRenderer _spriteRenderer;
        public Item Item => item;

        [Inject]
        public void Construct(IInventory inventory)
        {
            _inventory = inventory;
        }

        /// <summary>
        /// 팩토리에서 풀/신규 생성 시 아이템과 인벤토리를 한 번에 설정합니다.
        /// </summary>
        public void Initialize(Item itemToSet, IInventory inventory)
        {
            _inventory = inventory;
            SetItem(itemToSet);
        }

        public void SetItem(Item itemToSet)
        {
            item = itemToSet;
            ApplyItemIcon();
            _canInteract = false;
            transform.DOKill();
            PlayDropJumpAndEnableInteract();
            StartDespawnTimer();
        }

        private void PlayDropJumpAndEnableInteract()
        {
            var startPos = transform.position;
            var randomOffset = LandingRandomRadius * Random.insideUnitCircle;
            var landingPos = startPos + new Vector3(randomOffset.x, randomOffset.y, 0f);

            var jumpTween = transform.DOJump(landingPos, DropJumpPower, 1, DropJumpDuration).SetEase(Ease.OutQuad);
            var rotateTween = transform.DORotate(new Vector3(0f, 0f, 360f), DropJumpDuration / 5f, RotateMode.FastBeyond360).SetEase(Ease.Linear).SetLoops(5);
            
            DOTween.Sequence()
                .Append(jumpTween)
                .Join(rotateTween)
                .SetTarget(transform)
                .OnComplete(() =>
                {
                    _canInteract = true;
                });
        }

        private void ApplyItemIcon()
        {
            if (_spriteRenderer == null)
            {
                _spriteRenderer = gameObject.AddOrGetComponent<SpriteRenderer>();
                if (_spriteRenderer == null)
                    return;
            }
            
            _spriteRenderer.sprite = item != null ? item.Icon : null;
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
            Despawn();
        }

        private void StartDespawnTimer()
        {
            StopDespawnTimer();
            _despawnCoroutine = StartCoroutine(DespawnAfterSeconds(DespawnSeconds));
        }

        private void StopDespawnTimer()
        {
            if (_despawnCoroutine != null)
            {
                StopCoroutine(_despawnCoroutine);
                _despawnCoroutine = null;
            }
        }

        private IEnumerator DespawnAfterSeconds(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            _despawnCoroutine = null;
            Despawn();
        }

        private void Despawn()
        {
            StopDespawnTimer();
            transform.DOKill();
            var poolable = GetComponent<PoolableComponent>();
            if (poolable != null)
                poolable.Release();
            else
                Destroy(gameObject);
        }
    }
}

