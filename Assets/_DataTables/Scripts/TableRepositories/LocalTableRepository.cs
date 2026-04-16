using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;
using VContainer;
using VContainer.Unity;

namespace DungeonShooter
{
    /// <summary>
    /// 데이터 스크립터블 오브젝트들을 로드하여 캐싱
    /// </summary>
    public class LocalTableRepository : ITableRepository, IAsyncStartable
    {
        private readonly Dictionary<int, ITableEntry> _cache = new();
        
        /// <summary>
        /// 테이블 리포지토리를 초기화합니다.
        /// 스크립터블 오브젝트를 파싱하여 캐시에 저장합니다.
        /// </summary>

        public async UniTask StartAsync(CancellationToken cancellation = new CancellationToken())
        {
            await Addressables.InitializeAsync().Task;
            LoadAndCacheTableSO<SkillTableEntrySo>("SkillSO");
            LoadAndCacheTableSO<PlayerConfigSo>("PlayerConfigSO");
            LoadAndCacheTableSO<EnemyConfigSo>("EnemyConfigSO");
        }

        
        /// <summary>
        /// ID로 테이블 엔트리를 가져옵니다. 타입을 지정하지 않고 엔트리만 조회할 때 사용합니다.
        /// </summary>
        public ITableEntry GetTableEntry(int id)
        {
            return _cache.TryGetValue(id, out var entry) ? entry : null;
        }

        /// <summary>
        /// ID에 해당하는 테이블 엔트리의 런타임 타입을 반환합니다.
        /// </summary>
        public Type GetTableEntryTypeByID(int id)
        {
            return _cache.TryGetValue(id, out var entry) ? entry.GetType() : null;
        }

        /// <summary>
        /// 테이블 엔트리를 ID로 조회합니다.
        /// </summary>
        public T GetTableEntry<T>(int id) where T : class, ITableEntry
        {
            if (_cache.TryGetValue(id, out var entry))
            {
                if (entry is T typedEntry)
                {
                    return typedEntry;
                }

                LogHandler.LogWarning<LocalTableRepository>($"ID {id}의 엔트리가 {nameof(T)} 타입이 아닙니다. (실제 타입: {entry.GetType().Name})");
                return null;
            }
    
            LogHandler.LogWarning<LocalTableRepository>($"ID {id}를 가진 {nameof(T)} 엔트리를 찾을 수 없습니다.");
            return null;
        }

        /// <summary>
        /// 지정한 타입의 테이블 엔트리 전체 목록을 가져옵니다.
        /// </summary>
        public IReadOnlyList<T> GetAllTableEntries<T>() where T : class, ITableEntry
        {
            return _cache.Values.OfType<T>().ToList();
        }

        /// <summary>
        /// StringTextTable에서 ID에 해당하는 표시 문자열을 가져옵니다.
        /// </summary>
        public string GetStringText(int stringId)
        {
            // StringTextTableEntry를 제거했으므로, 기본적으로 ID 문자열을 반환합니다.
            return stringId.ToString();
        }
        
        private void LoadAndCacheTableSO<T>(string label) where T : ScriptableObject, ITableEntry
        {
            try
            {
                var handle = Addressables.LoadAssetsAsync<T>((object)label, null);
                var soList = handle.WaitForCompletion();

                if (soList == null)
                {
                    LogHandler.LogError<LocalTableRepository>($"SO 파일을 로드할 수 없습니다: {label}");
                    return;
                }

                CacheEntries(soList);

                LogHandler.Log<LocalTableRepository>($"{typeof(T).Name} 테이블 로드 완료: {soList.Count}개 엔트리");
            }
            catch (Exception ex)
            {
                LogHandler.LogError<LocalTableRepository>($"테이블 로드 실패 ({label}): {ex.Message}");
            }
        }

        /// <summary>
        /// 엔트리 리스트를 캐시에 저장합니다.
        /// </summary>
        private void CacheEntries<T>(IList<T> entries) where T : class, ITableEntry
        {
            foreach (var entry in entries)
            {
                var id = entry.Id;

                if (_cache.ContainsKey(id))
                {
                    LogHandler.LogWarning<LocalTableRepository>($"ID {id}가 중복됩니다. ({typeof(T).Name}) 첫 번째 엔트리를 사용합니다.");
                    continue;
                }

                _cache[id] = entry;
            }
        }
    }
}
