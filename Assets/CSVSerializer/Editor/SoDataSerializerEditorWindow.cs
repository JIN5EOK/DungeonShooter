using System;
using System.IO;
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
                    _lastResult = SoCsvPipeline.CsvToSo<FooSo, SerializedFoo>(_importCsvPath, folderPath);
                }

                if (GUILayout.Button("SO -> CSV"))
                {
                    var outputPath = _exportCsvPath;
                    if (string.IsNullOrWhiteSpace(outputPath))
                        outputPath = Path.Combine(Application.dataPath, $"{nameof(SerializedFoo)}.csv").Replace("\\", "/");

                    _lastResult = SoCsvPipeline.SoToCsv<FooSo, SerializedFoo>(folderPath, outputPath);
                }
            }

            if (_lastResult.HasValue)
            {
                EditorGUILayout.Space(8);
                EditorGUILayout.HelpBox(_lastResult.Value ? "성공" : "실패", _lastResult.Value ? MessageType.Info : MessageType.Error);
            }
        }
    }
}

