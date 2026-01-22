using Unity.VisualScripting;
using UnityEngine;

namespace DungeonShooter
{
    /// <summary>
    /// 게임 내 UI 요소들을 통합 관리하는 매니저
    /// </summary>
    public class GameUIManager : MonoBehaviour
    {
        [Header("체력 UI")]
        [SerializeField] private HealthBarUI healthBarUI;

        [Header("스킬 UI")]
        [SerializeField] private SkillCooldownUI[] skillUIs;

        [Header("플레이어 참조")]
        [SerializeField] private Player player;

        private HealthComponent _playerHealth;
        private CooldownComponent _playerCooldowns;

        private async void Start()
        {
            // 한 프레임 후에 초기화 (PlayerProto.Start() 이후 보장)
            await Awaitable.NextFrameAsync();
            await InitializeUIAsync();
        }

        private void Update()
        {
            UpdateUI();
        }

        /// <summary>
        /// UI 비동기 초기화
        /// </summary>
        private async Awaitable InitializeUIAsync()
        {
            // 플레이어가 지정되지 않았으면 찾기
            if (player == null)
            {
                player = FindFirstObjectByType<Player>();
            }

            if (player == null)
            {
                LogHandler.LogError<GameUIManager>("플레이어를 찾을 수 없습니다!");
                return;
            }

            // 플레이어 컴포넌트 참조
            _playerHealth = player.GetComponent<HealthComponent>();
            _playerCooldowns = player.GetComponent<CooldownComponent>();

            // CooldownComponent가 아직 초기화 안 됐으면 재시도
            if (_playerCooldowns == null)
            {
                LogHandler.LogWarning<GameUIManager>("CooldownComponent가 아직 초기화되지 않음. 재시도 중...");
                await RetryInitializationAsync();
                return;
            }

            LogHandler.Log<GameUIManager>("초기화 완료");

            // 체력 UI 초기화
            if (healthBarUI != null && _playerHealth != null)
            {
                healthBarUI.Initialize(_playerHealth);
            }

            // 스킬 UI 초기화
            InitializeSkillUIs();
        }

        /// <summary>
        /// CooldownComponent 초기화 재시도 (Awaitable 사용)
        /// </summary>
        private async Awaitable RetryInitializationAsync()
        {
            var retryCount = 0;
            const int maxRetries = 10;

            while (retryCount < maxRetries)
            {
                await Awaitable.WaitForSecondsAsync(0.1f); // 0.1초 대기

                if (player != null)
                {
                    _playerCooldowns = player.GetComponent<CooldownComponent>();
                    if (_playerCooldowns != null)
                    {
                        LogHandler.Log<GameUIManager>($"CooldownComponent 초기화 완료! (재시도: {retryCount + 1}회)");
                        InitializeSkillUIs();
                        return; // 성공하면 종료
                    }
                }

                retryCount++;
            }

            LogHandler.LogError<GameUIManager>($"CooldownComponent 초기화 실패! {maxRetries}회 재시도 후 포기.");
        }

        /// <summary>
        /// 스킬 UI 초기화
        /// </summary>
        private void InitializeSkillUIs()
        {
            if (skillUIs != null && skillUIs.Length > 0)
            {
                var skillKeys = new string[] { "dash", "skill1", "skill2", "skill3" };
                var skillNames = new string[] { "회피", "슬래시", "회전베기", "점프공격" };

                for (int i = 0; i < skillUIs.Length && i < skillKeys.Length; i++)
                {
                    if (skillUIs[i] != null)
                    {
                        skillUIs[i].Initialize(skillKeys[i], skillNames[i]);
                    }
                }
            }
        }

        /// <summary>
        /// UI 업데이트
        /// </summary>
        private void UpdateUI()
        {
            // 체력 UI 업데이트
            if (healthBarUI != null && _playerHealth != null)
            {
                healthBarUI.UpdateUI();
            }

            // 스킬 UI 업데이트
            if (skillUIs != null && _playerCooldowns != null)
            {
                for (int i = 0; i < skillUIs.Length; i++)
                {
                    if (skillUIs[i] != null)
                    {
                        skillUIs[i].UpdateCooldown(_playerCooldowns);
                    }
                }
            }
        }
    }
}
