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
        private string _csvPath;
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
            EditorGUILayout.LabelField("경로", EditorStyles.boldLabel);

            using (new EditorGUILayout.HorizontalScope())
            {
                _csvPath = EditorGUILayout.TextField("CSV Path", _csvPath);
                if (GUILayout.Button("...", GUILayout.Width(28)))
                {
                    var picked = EditorUtility.OpenFilePanel("CSV 선택", Application.dataPath, "csv");
                    if (!string.IsNullOrEmpty(picked))
                        _csvPath = picked;
                }
            }

            _soFolder = (DefaultAsset)EditorGUILayout.ObjectField("SO Folder", _soFolder, typeof(DefaultAsset), false);

            EditorGUILayout.Space(12);

            var folderPath = _soFolder != null ? AssetDatabase.GetAssetPath(_soFolder) : string.Empty;

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("CSV -> SO"))
                {
                    _lastResult = CsvToSo_SerializedFoo(_csvPath, folderPath);
                }

                if (GUILayout.Button("SO -> CSV"))
                {
                    var outputPath = _csvPath;
                    if (string.IsNullOrWhiteSpace(outputPath))
                    {
                        outputPath = Path.Combine(Application.dataPath, $"{nameof(SerializedFoo)}.csv");
                        outputPath = outputPath.Replace("\\", "/");
                    }

                    _lastResult = SoToCsv_SerializedFoo(folderPath, outputPath);
                }
            }

            if (_lastResult.HasValue)
            {
                EditorGUILayout.Space(8);
                EditorGUILayout.HelpBox(_lastResult.Value ? "성공" : "실패", _lastResult.Value ? MessageType.Info : MessageType.Error);
            }
        }

        private static bool CsvToSo_SerializedFoo(string csvPath, string writeFolder)
        {
            if (string.IsNullOrWhiteSpace(writeFolder))
            {
                LogHandler.LogError(nameof(SoDataSerializerEditorWindow), "SO Folder가 비었습니다.");
                return false;
            }

            var dtos = CSVSerializer.ReadCsv<SerializedFoo>(csvPath);
            if (dtos.Count == 0)
            {
                LogHandler.LogWarning(nameof(SoDataSerializerEditorWindow), "CSV에서 읽은 레코드가 없습니다.");
                return false;
            }

            var byId = LoadAllFooSoById(writeFolder);
            var succeeded = 0;
            var failed = 0;

            foreach (var dto in dtos)
            {
                try
                {
                    if (!byId.TryGetValue(dto.Id, out var so) || so == null)
                    {
                        so = CreateFooSoAsset(writeFolder, dto.Id);
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

        private static bool SoToCsv_SerializedFoo(string readFolder, string csvPath)
        {
            if (string.IsNullOrWhiteSpace(readFolder))
            {
                LogHandler.LogError(nameof(SoDataSerializerEditorWindow), "SO Folder가 비었습니다.");
                return false;
            }

            var assets = LoadAllFooSo(readFolder);
            var dtos = new List<SerializedFoo>(assets.Count);

            foreach (var so in assets)
            {
                var dto = new SerializedFoo();
                dto.PopulateFromSo(so);
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

        private static Dictionary<int, FooSo> LoadAllFooSoById(string folder)
        {
            var result = new Dictionary<int, FooSo>();
            foreach (var so in LoadAllFooSo(folder))
            {
                result[so.Id] = so;
            }

            return result;
        }

        private static List<FooSo> LoadAllFooSo(string folder)
        {
            var guids = AssetDatabase.FindAssets($"t:{nameof(FooSo)}", new[] { folder });
            var list = new List<FooSo>(guids.Length);
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<FooSo>(path);
                if (asset != null)
                    list.Add(asset);
            }

            return list;
        }

        private static FooSo CreateFooSoAsset(string folder, int id)
        {
            var so = ScriptableObject.CreateInstance<FooSo>();

            so.Id = id;

            var assetPath = CombineAssetPath(folder, $"{nameof(FooSo)}_{id}.asset");
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

