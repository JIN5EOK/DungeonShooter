using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace DungeonShooter
{
    public sealed class SoDataSerializerEditorWindow : EditorWindow
    {
        private string _importCsvPath;
        private string _exportCsvPath;
        private DefaultAsset _soFolder;
        private bool? _lastResult;

        [MenuItem("DungeonShooter/CSV Serializer")]
        private static void Open()
        {
            GetWindow<SoDataSerializerEditorWindow>("CSV Serializer");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("샘플 전용 (FooSo <-> SerializedFoo)", EditorStyles.boldLabel);

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("CSV -> SO", EditorStyles.boldLabel);

            using (new EditorGUILayout.HorizontalScope())
            {
                _importCsvPath = EditorGUILayout.TextField("CSV File", _importCsvPath);
                if (GUILayout.Button("...", GUILayout.Width(28)))
                {
                    var picked = EditorUtility.OpenFilePanel("CSV 선택", Application.dataPath, "csv");
                    if (!string.IsNullOrEmpty(picked))
                        _importCsvPath = picked;
                }
            }

            _soFolder = (DefaultAsset)EditorGUILayout.ObjectField("SO Folder", _soFolder, typeof(DefaultAsset), false);

            EditorGUILayout.Space(12);
            EditorGUILayout.LabelField("SO -> CSV", EditorStyles.boldLabel);

            using (new EditorGUILayout.HorizontalScope())
            {
                _exportCsvPath = EditorGUILayout.TextField("Save Path", _exportCsvPath);
                if (GUILayout.Button("...", GUILayout.Width(28)))
                {
                    var defaultName = $"{nameof(SerializedFoo)}.csv";
                    var picked = EditorUtility.SaveFilePanel("CSV 저장", Application.dataPath, defaultName, "csv");
                    if (!string.IsNullOrEmpty(picked))
                        _exportCsvPath = picked;
                }
            }

            EditorGUILayout.Space(12);

            var folderPath = _soFolder != null ? AssetDatabase.GetAssetPath(_soFolder) : string.Empty;

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("CSV -> SO"))
                {
                    _lastResult = CSVToSo<FooSo, SerializedFoo>(_importCsvPath, folderPath);
                }

                if (GUILayout.Button("SO -> CSV"))
                {
                    var outputPath = _exportCsvPath;
                    if (string.IsNullOrWhiteSpace(outputPath))
                        outputPath = Path.Combine(Application.dataPath, $"{nameof(SerializedFoo)}.csv").Replace("\\", "/");

                    _lastResult = SoToCSV<FooSo, SerializedFoo>(folderPath, outputPath);
                }
            }

            if (_lastResult.HasValue)
            {
                EditorGUILayout.Space(8);
                EditorGUILayout.HelpBox(_lastResult.Value ? "성공" : "실패", _lastResult.Value ? MessageType.Info : MessageType.Error);
            }
        }

        private static bool CSVToSo<TSo, TSerialized>(string csvPath, string writeFolder) where TSo : ScriptableObject, IIntId where TSerialized : ISerializeSODto<TSo>
        {
            if (string.IsNullOrWhiteSpace(writeFolder))
            {
                LogHandler.LogError(nameof(SoDataSerializerEditorWindow), "SO Folder가 비었습니다.");
                return false;
            }

            var dtos = CSVSerializer.ReadCsv<TSerialized>(csvPath);
            if (dtos.Count == 0)
            {
                LogHandler.LogWarning(nameof(SoDataSerializerEditorWindow), "CSV에서 읽은 레코드가 없습니다.");
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
                    LogHandler.LogError(nameof(SoDataSerializerEditorWindow), $"CSV->SO 실패 (Id={dto.Id}): {ex.Message}");
                    LogHandler.LogException(nameof(SoDataSerializerEditorWindow), ex);
                }
            }

            AssetDatabase.SaveAssets();
            LogHandler.Log(nameof(SoDataSerializerEditorWindow), $"CSV->SO 완료. Succeeded={succeeded}, Failed={failed}");
            return failed == 0;
        }

        private static bool SoToCSV<TSo, TSerialized>(string readFolder, string csvPath) where TSo : ScriptableObject, IIntId where TSerialized : ISerializeSODto<TSo>
        {
            if (string.IsNullOrWhiteSpace(readFolder))
            {
                LogHandler.LogError(nameof(SoDataSerializerEditorWindow), "SO Folder가 비었습니다.");
                return false;
            }

            var assets = LoadAllSo<TSo>(readFolder);
            if (assets.Count == 0)
            {
                LogHandler.LogWarning(nameof(SoDataSerializerEditorWindow), "SO 폴더에서 에셋을 찾지 못했습니다.");
                return false;
            }
            var dtos = new List<TSerialized>(assets.Count);

            foreach (var so in assets)
            {
                var dto = Activator.CreateInstance<TSerialized>();
                dto.PopulateFrom(so);
                dtos.Add(dto);
            }

            var ok = CSVSerializer.WriteCsv(dtos, csvPath);
            if (ok)
                LogHandler.Log(nameof(SoDataSerializerEditorWindow), $"SO->CSV 완료. Count={dtos.Count}, Path={csvPath}");
            else
                LogHandler.LogError(nameof(SoDataSerializerEditorWindow), $"SO->CSV 실패. Path={csvPath}");

            AssetDatabase.Refresh();
            return ok;
        }

        private static Dictionary<int, TSo> LoadAllSoById<TSo>(string folder) where TSo : ScriptableObject, IIntId
        {
            var result = new Dictionary<int, TSo>();
            foreach (var so in LoadAllSo<TSo>(folder))
            {
                result[so.Id] = so;
            }

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
            var so = CreateInstance<TSo>();

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

