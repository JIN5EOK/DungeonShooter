using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using CsvHelper;
using CsvHelper.Configuration;

namespace DungeonShooter
{
    public static class CSVSerializer
    {
        public static bool CsvToSo(Type dtoType, string csvPath, string writeFolder)
        {
            if (dtoType == null)
            {
                LogHandler.LogError(nameof(CSVSerializer), "DTO 타입이 null입니다.");
                return false;
            }

            if (!File.Exists(csvPath))
            {
                LogHandler.LogError(nameof(CSVSerializer), $"CSV 파일이 존재하지 않습니다: {csvPath}");
                return false;
            }

            if (string.IsNullOrWhiteSpace(writeFolder))
            {
                LogHandler.LogError(nameof(CSVSerializer), "writeFolder가 비었습니다.");
                return false;
            }

            var soType = GetSoTypeFromDto(dtoType);
            if (soType == null)
            {
                LogHandler.LogError(nameof(CSVSerializer), $"DTO에 {nameof(CsvDtoForAttribute)}가 없거나 SO 타입이 유효하지 않습니다. DTO={dtoType.FullName}");
                return false;
            }

            var records = ReadDtoRecords(dtoType, csvPath, out var readError);
            if (records == null)
            {
                LogHandler.LogError(nameof(CSVSerializer), readError ?? "CSV 읽기에 실패했습니다.");
                return false;
            }

            var idAccessor = new IdAccessor(soType);
            var applyMethod = dtoType.GetMethod("ApplyTo", BindingFlags.Public | BindingFlags.Instance);
            if (applyMethod == null)
            {
                LogHandler.LogError(nameof(CSVSerializer), $"DTO에 ApplyTo 메서드가 없습니다. DTO={dtoType.FullName}");
                return false;
            }

            var total = 0;
            var succeeded = 0;
            var failed = 0;
            var errors = new List<string>();

            foreach (var dto in records)
            {
                total++;

                try
                {
                    var dtoId = GetDtoId(dto);
                    if (dtoId == null)
                        throw new InvalidOperationException($"DTO에 Id 프로퍼티가 없거나 int가 아닙니다. DTO={dtoType.FullName}");

                    var so = FindSoById(soType, writeFolder, idAccessor, dtoId.Value) ?? CreateSoAsset(soType, writeFolder, dtoId.Value);
                    applyMethod.Invoke(dto, new[] { so });

                    succeeded++;
                }
                catch (Exception ex)
                {
                    failed++;
                    var msg = ex.InnerException?.Message ?? ex.Message;
                    errors.Add(msg);
                    LogHandler.LogError(nameof(CSVSerializer), $"CSV->SO 실패: {msg}");
                    LogHandler.LogException(nameof(CSVSerializer), ex);
                }
            }

            LogHandler.Log(nameof(CSVSerializer), $"CSV->SO 완료. Total={total}, Succeeded={succeeded}, Failed={failed}");
            return failed == 0;
        }

        public static bool SoToCsv(Type dtoType, string readFolder, string csvPath)
        {
            if (dtoType == null)
            {
                LogHandler.LogError(nameof(CSVSerializer), "DTO 타입이 null입니다.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(readFolder))
            {
                LogHandler.LogError(nameof(CSVSerializer), "readFolder가 비었습니다.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(csvPath))
            {
                LogHandler.LogError(nameof(CSVSerializer), "csvPath가 비었습니다.");
                return false;
            }

            var soType = GetSoTypeFromDto(dtoType);
            if (soType == null)
            {
                LogHandler.LogError(nameof(CSVSerializer), $"DTO에 {nameof(CsvDtoForAttribute)}가 없거나 SO 타입이 유효하지 않습니다. DTO={dtoType.FullName}");
                return false;
            }

            var populateMethod = dtoType.GetMethod("PopulateFromSo", BindingFlags.Public | BindingFlags.Instance);
            if (populateMethod == null)
            {
                LogHandler.LogError(nameof(CSVSerializer), $"DTO에 PopulateFromSo 메서드가 없습니다. DTO={dtoType.FullName}");
                return false;
            }

            try
            {
                var assets = LoadAllAssets(soType, readFolder);
                var dtoList = new List<object>(assets.Count);

                foreach (var so in assets)
                {
                    var dto = Activator.CreateInstance(dtoType);
                    populateMethod.Invoke(dto, new[] { so });
                    dtoList.Add(dto);
                }

                WriteDtoRecords(dtoType, dtoList, csvPath);
                LogHandler.Log(nameof(CSVSerializer), $"SO->CSV 완료. Count={dtoList.Count}, Path={csvPath}");
                return true;
            }
            catch (Exception ex)
            {
                LogHandler.LogError(nameof(CSVSerializer), $"SO->CSV 실패: {ex.Message}");
                LogHandler.LogException(nameof(CSVSerializer), ex);
                return false;
            }
        }

        private static Type GetSoTypeFromDto(Type dtoType)
        {
            var attr = dtoType.GetCustomAttribute<CsvDtoForAttribute>();
            if (attr?.SoType == null)
                return null;
            if (!typeof(UnityEngine.ScriptableObject).IsAssignableFrom(attr.SoType))
                return null;
            return attr.SoType;
        }

        private static int? GetDtoId(object dto)
        {
            if (dto == null)
                return null;

            var prop = dto.GetType().GetProperty("Id", BindingFlags.Public | BindingFlags.Instance);
            if (prop == null || prop.PropertyType != typeof(int))
                return null;

            return (int)prop.GetValue(dto);
        }

        private static IReadOnlyList<object> ReadDtoRecords(Type dtoType, string csvPath, out string error)
        {
            error = null;

            try
            {
                var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true,
                    NewLine = "\n",
                };

                using var reader = new StreamReader(csvPath);
                using var csv = new CsvReader(reader, csvConfig);

                var getRecordsMethod = typeof(CsvReader)
                    .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .First(m => m.Name == nameof(CsvReader.GetRecords) && m.IsGenericMethodDefinition);

                var generic = getRecordsMethod.MakeGenericMethod(dtoType);
                var enumerable = (System.Collections.IEnumerable)generic.Invoke(csv, null);

                var list = new List<object>();
                foreach (var item in enumerable)
                    list.Add(item);

                return list;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return null;
            }
        }

        private static void WriteDtoRecords(Type dtoType, IReadOnlyList<object> dtos, string csvPath)
        {
            var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                NewLine = "\n",
            };

            using var writer = new StreamWriter(csvPath);
            using var csv = new CsvWriter(writer, csvConfig);

            var typedListType = typeof(List<>).MakeGenericType(dtoType);
            var typedList = (System.Collections.IList)Activator.CreateInstance(typedListType);
            foreach (var dto in dtos)
                typedList.Add(dto);

            var writeRecordsMethod = typeof(CsvWriter)
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .First(m => m.Name == nameof(CsvWriter.WriteRecords) && m.IsGenericMethodDefinition);

            var generic = writeRecordsMethod.MakeGenericMethod(dtoType);
            generic.Invoke(csv, new object[] { typedList });
        }

        private static UnityEngine.ScriptableObject FindSoById(Type soType, string folder, IdAccessor idAccessor, int id)
        {
            var assets = LoadAllAssets(soType, folder);
            foreach (var asset in assets)
            {
                if (idAccessor.TryGetId(asset, out var assetId) && assetId == id)
                    return asset;
            }

            return null;
        }

        private static UnityEngine.ScriptableObject CreateSoAsset(Type soType, string folder, int id)
        {
            var instance = UnityEngine.ScriptableObject.CreateInstance(soType);
            var assetName = $"{soType.Name}_{id}.asset";

            var assetPath = CombineAssetPath(folder, assetName);

#if UNITY_EDITOR
            UnityEditor.AssetDatabase.CreateAsset(instance, assetPath);
            UnityEditor.EditorUtility.SetDirty(instance);
            UnityEditor.AssetDatabase.SaveAssets();
#endif

            return instance;
        }

        private static List<UnityEngine.ScriptableObject> LoadAllAssets(Type soType, string folder)
        {
#if UNITY_EDITOR
            var guids = UnityEditor.AssetDatabase.FindAssets($"t:{soType.Name}", new[] { folder });
            var list = new List<UnityEngine.ScriptableObject>(guids.Length);

            foreach (var guid in guids)
            {
                var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                var asset = UnityEditor.AssetDatabase.LoadAssetAtPath(path, soType) as UnityEngine.ScriptableObject;
                if (asset != null)
                    list.Add(asset);
            }

            return list;
#else
            return new List<UnityEngine.ScriptableObject>();
#endif
        }

        private static string CombineAssetPath(string folder, string fileName)
        {
            if (string.IsNullOrWhiteSpace(folder))
                return fileName;

            var normalized = folder.Replace("\\", "/");
            if (normalized.EndsWith("/", StringComparison.Ordinal))
                return normalized + fileName;

            return normalized + "/" + fileName;
        }

        private sealed class IdAccessor
        {
            private readonly PropertyInfo _idProperty;
            private readonly FieldInfo _idField;

            public IdAccessor(Type soType)
            {
                _idProperty = soType.GetProperty("Id", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                _idField = soType.GetField("Id", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance) ??
                           soType.GetField("id", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            }

            public bool TryGetId(UnityEngine.ScriptableObject so, out int id)
            {
                id = 0;

                if (_idProperty != null && _idProperty.PropertyType == typeof(int) && _idProperty.CanRead)
                {
                    id = (int)_idProperty.GetValue(so);
                    return true;
                }

                if (_idField != null && _idField.FieldType == typeof(int))
                {
                    id = (int)_idField.GetValue(so);
                    return true;
                }

                return false;
            }
        }
    }
}

