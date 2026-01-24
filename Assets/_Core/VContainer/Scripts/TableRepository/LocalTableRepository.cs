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
        /// <summary>
        /// 타입별 캐시: Dictionary<타입, Dictionary<ID, 엔트리>>
        /// </summary>
        private readonly Dictionary<Type, Dictionary<int, object>> _cache = new();

        /// <summary>
        /// CSV 파일 경로 설정
        /// </summary>
        private struct CSVTableConfig
        {
            public string AssetPath;
            public Type EntryType;
        }

        /// <summary>
        /// 테이블 리포지토리를 초기화합니다.
        /// 모든 CSV 파일을 로드하고 파싱하여 캐시에 저장합니다.
        /// </summary>
        public void Initialize()
        {
            var tableConfigs = new[]
            {
                new CSVTableConfig { AssetPath = "Assets/_Data/Tables/SkillTable.csv", EntryType = typeof(SkillTableEntry) },
                new CSVTableConfig { AssetPath = "Assets/_Data/Tables/ItemTable.csv", EntryType = typeof(ItemTableEntry) },
            };

            foreach (var config in tableConfigs)
            {
                LoadAndCacheTable(config.AssetPath, config.EntryType);
            }
        }

        /// <summary>
        /// 테이블 엔트리를 ID로 조회합니다.
        /// </summary>
        public T GetTableEntry<T>(int id) where T : class
        {
            var type = typeof(T);

            if (!_cache.TryGetValue(type, out var typeCache))
            {
                LogHandler.LogWarning<LocalTableRepository>($"캐시에 타입 {nameof(T)}이 없습니다.");
                return null;
            }

            if (typeCache.TryGetValue(id, out var entry))
            {
                return entry as T;
            }

            LogHandler.LogWarning<LocalTableRepository>($"ID {id}를 가진 {nameof(T)} 엔트리를 찾을 수 없습니다.");
            return null;
        }

        /// <summary>
        /// CSV 파일을 로드하고 캐시에 저장합니다.
        /// </summary>
        private void LoadAndCacheTable(string assetPath, Type entryType)
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

                var entries = ParseCSVFile(textAsset, entryType);
                CacheEntries(entryType, entries);

                Addressables.Release(handle);
                LogHandler.Log<LocalTableRepository>($"{entryType.Name} 테이블 로드 완료: {entries.Count}개 엔트리");
            }
            catch (Exception ex)
            {
                LogHandler.LogError<LocalTableRepository>($"테이블 로드 실패 ({assetPath}): {ex.Message}");
            }
        }

        /// <summary>
        /// CSV 파일을 파싱합니다.
        /// </summary>
        private List<object> ParseCSVFile(UnityEngine.TextAsset textAsset, Type entryType)
        {
            // 리플렉션을 사용하여 CSVTableParser.Parse<T>를 호출
            var parseMethod = typeof(CSVTableParser)
                .GetMethod(nameof(CSVTableParser.Parse))
                ?.MakeGenericMethod(entryType);

            if (parseMethod == null)
            {
                LogHandler.LogError<LocalTableRepository>($"CSVTableParser.Parse 메서드를 찾을 수 없습니다.");
                return new List<object>();
            }

            var result = parseMethod.Invoke(null, new object[] { textAsset });
            if (result is System.Collections.IList listResult)
            {
                var entries = new List<object>();
                foreach (var item in listResult)
                {
                    entries.Add(item);
                }
                return entries;
            }

            return new List<object>();
        }

        /// <summary>
        /// 엔트리 리스트를 캐시에 저장합니다.
        /// </summary>
        private void CacheEntries(Type entryType, List<object> entries)
        {
            var typeCache = new Dictionary<int, object>();

            foreach (var entry in entries)
            {
                // 엔트리의 Id 속성 값을 가져옴
                var idProperty = entryType.GetProperty(nameof(SkillTableEntry.Id));
                if (idProperty == null || !idProperty.CanRead)
                {
                    LogHandler.LogWarning<LocalTableRepository>($"{entryType.Name}에 Id 속성이 없습니다.");
                    continue;
                }

                var id = (int)idProperty.GetValue(entry);

                if (typeCache.ContainsKey(id))
                {
                    LogHandler.LogWarning<LocalTableRepository>($"ID {id}가 중복됩니다. ({entryType.Name}) 첫 번째 엔트리를 사용합니다.");
                    continue;
                }

                typeCache[id] = entry;
            }

            _cache[entryType] = typeCache;
        }
    }
}
