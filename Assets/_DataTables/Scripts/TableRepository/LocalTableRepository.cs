using System;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;

namespace DungeonShooter
{
    /// <summary>
    /// 로컬 CSV 파일을 통해 테이블 데이터를 제공하는 리포지토리
    /// 초기화 시 모든 CSV를 파싱하여 메모리에 캐싱
    /// </summary>
    public class LocalTableRepository : ITableRepository
    {
        private readonly Dictionary<int, ITableEntry> _cache = new();
        
        /// <summary>
        /// 테이블 리포지토리를 초기화합니다.
        /// 모든 CSV 파일을 로드하고 파싱하여 캐시에 저장합니다.
        /// </summary>
        
        public LocalTableRepository()
        {
            LoadAndCacheTable<SkillTableEntry>("SkillTable");
            LoadAndCacheTable<ItemTableEntry>("ItemTable");
            LoadAndCacheTable<StageConfigTableEntry>("StageConfigTable");
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
        /// CSV 파일을 로드하고 캐시에 저장합니다.
        /// </summary>
        private void LoadAndCacheTable<T>(string assetPath) where T : class, ITableEntry, new()
        {
            try
            {
                var handle = Addressables.LoadAssetAsync<UnityEngine.TextAsset>(assetPath);
                var textAsset = handle.WaitForCompletion();

                if (textAsset == null)
                {
                    LogHandler.LogError<LocalTableRepository>($"CSV 파일을 로드할 수 없습니다: {assetPath}");
                    return;
                }

                var entries = CSVTableParser.Parse<T>(textAsset);
                CacheEntries(entries);

                Addressables.Release(handle);
                LogHandler.Log<LocalTableRepository>($"{typeof(T).Name} 테이블 로드 완료: {entries.Count}개 엔트리");
            }
            catch (Exception ex)
            {
                LogHandler.LogError<LocalTableRepository>($"테이블 로드 실패 ({assetPath}): {ex.Message}");
            }
        }

        /// <summary>
        /// 엔트리 리스트를 캐시에 저장합니다.
        /// </summary>
        private void CacheEntries<T>(List<T> entries) where T : class, ITableEntry
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
