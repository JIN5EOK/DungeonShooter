using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace DungeonShooter
{
    public static class SoCSVPipeline
    {
        public static bool CsvToSo<TSo, TSerialized>(string csvPath, string writeFolder)
            where TSo : ScriptableObject, ISerializableObject<TSerialized>
            where TSerialized : class, ITableEntry, new()
        {
            if (string.IsNullOrWhiteSpace(writeFolder))
            {
                LogHandler.LogError(nameof(SoCSVPipeline), "SO Folder가 비었습니다.");
                return false;
            }

            var dtos = CSVParseHelper.ReadCsv<TSerialized>(csvPath);
            if (dtos.Count == 0)
            {
                LogHandler.LogWarning(nameof(SoCSVPipeline), "CSV에서 읽은 레코드가 없습니다.");
                return false;
            }

            var byId = LoadAllSoById<TSo>(writeFolder);
            var succeeded = 0;
            var failed = 0;

            var grouped = dtos.GroupBy(d => d.Id).ToList();
            foreach (var group in grouped)
            {
                try
                {
                    var id = group.Key;
                    var rows = group.ToList();

                    if (!byId.TryGetValue(id, out var so) || so == null)
                    {
                        so = CreateSoAsset<TSo>(writeFolder, id);
                        byId[id] = so;
                    }

                    so.ApplyFromSerializedDto(rows);
                    EditorUtility.SetDirty(so);
                    succeeded++;
                }
                catch (Exception ex)
                {
                    failed++;
                    LogHandler.LogError(nameof(SoCSVPipeline), $"CSV->SO 실패 (Id={group.Key}): {ex.Message}");
                    LogHandler.LogException(nameof(SoCSVPipeline), ex);
                }
            }

            AssetDatabase.SaveAssets();
            LogHandler.Log(nameof(SoCSVPipeline), $"CSV->SO 완료. Succeeded={succeeded}, Failed={failed}");
            return failed == 0;
        }

        public static bool SoToCsv<TSo, TSerialized>(string readFolder, string csvPath)
            where TSo : ScriptableObject, ISerializableObject<TSerialized>
            where TSerialized : class, ITableEntry, new()
        {
            if (string.IsNullOrWhiteSpace(readFolder))
            {
                LogHandler.LogError(nameof(SoCSVPipeline), "SO Folder가 비었습니다.");
                return false;
            }

            var assets = LoadAllSo<TSo>(readFolder);
            if (assets.Count == 0)
            {
                LogHandler.LogWarning(nameof(SoCSVPipeline), "SO 폴더에서 에셋을 찾지 못했습니다.");
                return false;
            }

            var dtos = new List<TSerialized>();
            foreach (var so in assets)
                dtos.AddRange(so.CreateSerializedDto());

            var ok = CSVParseHelper.WriteCsv(dtos, csvPath);
            if (ok)
                LogHandler.Log(nameof(SoCSVPipeline), $"SO->CSV 완료. Count={dtos.Count}, Path={csvPath}");
            else
                LogHandler.LogError(nameof(SoCSVPipeline), $"SO->CSV 실패. Path={csvPath}");

            AssetDatabase.Refresh();
            return ok;
        }

        private static Dictionary<int, TSo> LoadAllSoById<TSo>(string folder) where TSo : ScriptableObject, ITableEntry
        {
            var result = new Dictionary<int, TSo>();
            foreach (var so in LoadAllSo<TSo>(folder))
                result[so.Id] = so;
            return result;
        }

        private static List<TSo> LoadAllSo<TSo>(string folder) where TSo : ScriptableObject
        {
            if (string.IsNullOrWhiteSpace(folder) || !AssetDatabase.IsValidFolder(folder))
                return new List<TSo>();

            var guids = AssetDatabase.FindAssets($"t:{typeof(TSo).Name}", new[] { folder });
            var list = new List<TSo>(guids.Length);

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<TSo>(path);
                if (asset != null)
                    list.Add(asset);
            }

            return list;
        }

        private static TSo CreateSoAsset<TSo>(string folder, int id) where TSo : ScriptableObject
        {
            var so = ScriptableObject.CreateInstance<TSo>();
            var assetPath = CombineAssetPath(folder, $"{typeof(TSo).Name}_{id}.asset");
            AssetDatabase.CreateAsset(so, assetPath);
            return so;
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
    }
}

