using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using Jin5eok;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;

namespace DungeonShooter
{
    public class Player : EntityBase
    {
        private ISceneResourceProvider _sceneResourceProvider;
        private PlayerManager _playerManager;
        private HealthComponent _healthComponent;
        private MovementComponent _movementComponent;
        private DashComponent _dashComponent;
        private InteractComponent _interactComponent;
        private CameraTrackComponent _cameraTrackComponent;
        private bool _isDead;

        [Inject]
        private void Construct(ISceneResourceProvider sceneResourceProvider)
        {
            _sceneResourceProvider = sceneResourceProvider;
        }

        /// <summary>
        /// PlayerManager가 아바타 바인딩 시 호출합니다. 씬용 컴포넌트와 사망 구독을 설정합니다.
        /// </summary>
        public async UniTask SetupSceneComponents(PlayerManager manager)
        {
            if (manager == null) return;

            _playerManager = manager;
            _movementComponent = gameObject.AddOrGetComponent<MovementComponent>();
            _interactComponent = gameObject.AddOrGetComponent<InteractComponent>();
            _healthComponent = gameObject.AddOrGetComponent<HealthComponent>();
            _dashComponent = gameObject.AddOrGetComponent<DashComponent>();
            _cameraTrackComponent = _sceneResourceProvider.AddOrGetComponentWithInejct<CameraTrackComponent>(gameObject);
            await _cameraTrackComponent.AttachCameraAsync();
            _healthComponent.FullHeal();
            _healthComponent.OnDeath += HandleDeath;
        }

        private void HandleDeath()
        {
            if (_isDead) return;

            _isDead = true;
            LogHandler.Log<Player>("플레이어 사망!");
            enabled = false;
            StartCoroutine(GameOverSequence());
        }

        private IEnumerator GameOverSequence()
        {
            yield return new WaitForSeconds(1f);

            var spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                var fadeTime = 1f;
                var startColor = spriteRenderer.color;
                for (float t = 0; t < fadeTime; t += Time.deltaTime)
                {
                    var alpha = Mathf.Lerp(1f, 0f, t / fadeTime);
                    spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
                    yield return null;
                }
            }

            yield return new WaitForSeconds(0.5f);
            LogHandler.Log<Player>("게임 오버! 씬 재시작 중...");
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private void OnDestroy()
        {
            if (_healthComponent != null)
            {
                _healthComponent.OnDeath -= HandleDeath;
            }

            _playerManager?.UnbindPlayerEntity(this);
        }
    }
}
