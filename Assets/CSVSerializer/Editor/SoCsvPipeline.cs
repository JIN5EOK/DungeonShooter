using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace DungeonShooter
{
    public static class SoCsvPipeline
    {
        public static bool CsvToSo<TSo, TSerialized>(string csvPath, string writeFolder)
            where TSo : ScriptableObject, IIntId
            where TSerialized : ISerializeSODto<TSo>
        {
            if (string.IsNullOrWhiteSpace(writeFolder))
            {
                LogHandler.LogError(nameof(SoCsvPipeline), "SO Folder가 비었습니다.");
                return false;
            }

            var dtos = CSVSerializer.ReadCsv<TSerialized>(csvPath);
            if (dtos.Count == 0)
            {
                LogHandler.LogWarning(nameof(SoCsvPipeline), "CSV에서 읽은 레코드가 없습니다.");
                return false;
            }

            var byId = LoadAllSoById<TSo>(writeFolder);
            var succeeded = 0;
            var failed = 0;

            foreach (var dto in dtos)
            {
                try
                {
                    if (!byId.TryGetValue(dto.Id, out var so) || so == null)
                    {
                        so = CreateSoAsset<TSo>(writeFolder, dto.Id);
                        byId[dto.Id] = so;
                    }

                    dto.ApplyTo(so);
                    EditorUtility.SetDirty(so);
                    succeeded++;
                }
                catch (Exception ex)
                {
                    failed++;
                    LogHandler.LogError(nameof(SoCsvPipeline), $"CSV->SO 실패 (Id={dto.Id}): {ex.Message}");
                    LogHandler.LogException(nameof(SoCsvPipeline), ex);
                }
            }

            AssetDatabase.SaveAssets();
            LogHandler.Log(nameof(SoCsvPipeline), $"CSV->SO 완료. Succeeded={succeeded}, Failed={failed}");
            return failed == 0;
        }

        public static bool SoToCsv<TSo, TSerialized>(string readFolder, string csvPath)
            where TSo : ScriptableObject, IIntId
            where TSerialized : ISerializeSODto<TSo>
        {
            if (string.IsNullOrWhiteSpace(readFolder))
            {
                LogHandler.LogError(nameof(SoCsvPipeline), "SO Folder가 비었습니다.");
                return false;
            }

            var assets = LoadAllSo<TSo>(readFolder);
            if (assets.Count == 0)
            {
                LogHandler.LogWarning(nameof(SoCsvPipeline), "SO 폴더에서 에셋을 찾지 못했습니다.");
                return false;
            }

            var dtos = assets.Select(so =>
            {
                var dto = Activator.CreateInstance<TSerialized>();
                dto.PopulateFrom(so);
                return dto;
            }).ToList();

            var ok = CSVSerializer.WriteCsv(dtos, csvPath);
            if (ok)
                LogHandler.Log(nameof(SoCsvPipeline), $"SO->CSV 완료. Count={dtos.Count}, Path={csvPath}");
            else
                LogHandler.LogError(nameof(SoCsvPipeline), $"SO->CSV 실패. Path={csvPath}");

            AssetDatabase.Refresh();
            return ok;
        }

        private static Dictionary<int, TSo> LoadAllSoById<TSo>(string folder) where TSo : ScriptableObject, IIntId
        {
            var result = new Dictionary<int, TSo>();
            foreach (var so in LoadAllSo<TSo>(folder))
                result[so.Id] = so;
            return result;
        }

        private static List<TSo> LoadAllSo<TSo>(string folder) where TSo : ScriptableObject, IIntId
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

        private static TSo CreateSoAsset<TSo>(string folder, int id) where TSo : ScriptableObject, IIntId
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

