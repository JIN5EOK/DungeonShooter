using System;
using System.Threading.Tasks;
using UnityEngine;
using VContainer;

namespace DungeonShooter
{
    /// <summary>
    /// 스테이지에 맞는 랜덤한 적을 하나 배치함
    /// </summary>
    
    public class RandomEnemySpawn : MonoBehaviour, IInitializationAwaiter
    {
        public bool IsInitialized => false; // 어차피 초기화 직후 바로 파괴되므로 false
        public Task<bool> InitializationTask { get; private set; }
        private IStageResourceProvider _resourceProvider;
        [Inject]
        public async Awaitable Construct(IStageResourceProvider resourceProvider)
        {
            _resourceProvider = resourceProvider;
            InitializationTask = SpawnEnemy();
            await InitializationTask;
            Destroy(gameObject); // 스테이지에서 많이 사용하게 된다면 오브젝트 풀링 사용해야 할수도?, 지금은 스테이지 생성시에만 사용하므로 보류
        }

        private async Task<bool> SpawnEnemy()
        {
            var enemy = await _resourceProvider.GetRandomEnemy();
            
            if (enemy == null)
            {
                Debug.LogWarning($"[{nameof(RandomEnemySpawn)}] 적 생성 실패");
                return false;
            }

            Debug.Log($"[{nameof(RandomEnemySpawn)}] 적 생성 성공: {enemy.name}");
            enemy.transform.SetParent(transform.parent);
            enemy.transform.position = transform.position;
            return true;
        }
    }
}