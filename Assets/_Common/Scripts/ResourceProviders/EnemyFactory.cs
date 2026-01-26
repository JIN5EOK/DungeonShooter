using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using VContainer;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace DungeonShooter
{
    /// <summary>
    /// 적 캐릭터를 생성하는 팩토리
    /// </summary>
    public class EnemyFactory : IEnemyFactory
    {
        private readonly StageConfig _stageConfig;
        private readonly ISceneResourceProvider _sceneResourceProvider;
        private List<string> EnemyAddresses { get; set; }

        [Inject]
        public EnemyFactory(StageContext context, ISceneResourceProvider sceneResourceProvider)
        {
            _stageConfig = context.StageConfig;
            _sceneResourceProvider = sceneResourceProvider;
            Initialize();
        }

        /// <summary>
        /// StageConfig의 Label 데이터를 기반으로 에셋의 어드레스 목록을 로드하여 저장합니다.
        /// </summary>
        private void Initialize()
        {
            if (!string.IsNullOrEmpty(_stageConfig.StageEnemyLabel.labelString))
            {
                var handle = Addressables.LoadResourceLocationsAsync(_stageConfig.StageEnemyLabel.labelString);
                handle.WaitForCompletion();
                EnemyAddresses = handle.Result.Select(location => location.PrimaryKey).ToList();

                Addressables.Release(handle);
            }
        }

        /// <summary>
        /// 스테이지에 맞는 랜덤 적을 가져옵니다.
        /// </summary>
        public async UniTask<Enemy> GetRandomEnemyAsync()
        {
            var enemyAddress = GetRandomEnemyAddress();
            if (enemyAddress == null)
            {
                return null;
            }

            var enemyInstance = await _sceneResourceProvider.GetInstanceAsync(enemyAddress);
            return GetEnemyFromInstance(enemyInstance, enemyAddress);
        }

        /// <summary>
        /// 스테이지에 맞는 랜덤 적을 동기적으로 가져옵니다.
        /// </summary>
        public Enemy GetRandomEnemySync()
        {
            var enemyAddress = GetRandomEnemyAddress();
            if (enemyAddress == null)
            {
                return null;
            }

            var enemyInstance = _sceneResourceProvider.GetInstanceSync(enemyAddress);
            return GetEnemyFromInstance(enemyInstance, enemyAddress);
        }

        /// <summary>
        /// 랜덤 적 어드레스 선택
        /// </summary>
        private string GetRandomEnemyAddress()
        {
            if (EnemyAddresses == null || EnemyAddresses.Count == 0)
            {
                Debug.LogWarning($"[{nameof(EnemyFactory)}] 적 어드레스 목록이 비어있습니다.");
                return null;
            }

            return EnemyAddresses[Random.Range(0, EnemyAddresses.Count)];
        }

        /// <summary>
        /// 인스턴스에서 Enemy 컴포넌트 추출 및 검증
        /// </summary>
        private Enemy GetEnemyFromInstance(GameObject enemyInstance, string enemyAddress)
        {
            if (enemyInstance == null)
            {
                Debug.LogWarning($"[{nameof(EnemyFactory)}] 적 인스턴스 생성 실패: {enemyAddress}");
                return null;
            }

            var enemy = enemyInstance.GetComponent<Enemy>();
            if (enemy == null)
            {
                Debug.LogWarning($"[{nameof(EnemyFactory)}] 프리팹에 Enemy 컴포넌트가 없습니다: {enemyAddress}");
                Object.Destroy(enemyInstance);
                return null;
            }

            return enemy;
        }
    }
}
