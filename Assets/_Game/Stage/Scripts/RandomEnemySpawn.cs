using UnityEngine;
using VContainer;

namespace DungeonShooter
{
    /// <summary>
    /// 스테이지에 맞는 랜덤한 적을 하나 배치함
    /// </summary>
    public class RandomEnemySpawn : MonoBehaviour
    {
        [Inject]
        public async Awaitable Construct(IStageResourceProvider resourceProvider)
        {
            Debug.Log("Constructing RandomEnemySpawn");
            await SpawnEnemy(resourceProvider);
        }

        private async Awaitable SpawnEnemy(IStageResourceProvider resourceProvider)
        {
            var enemy = await resourceProvider.GetRandomEnemy();
            
            if (enemy == null)
            {
                Debug.LogWarning($"[{nameof(RandomEnemySpawn)}] 적 생성 실패");
                Destroy(gameObject);
                return;
            }

            Debug.Log($"[{nameof(RandomEnemySpawn)}] 적 생성 성공: {enemy.name}");
            enemy.transform.SetParent(transform.parent);
            enemy.transform.position = transform.position;
            Destroy(gameObject);
        }
    }
}