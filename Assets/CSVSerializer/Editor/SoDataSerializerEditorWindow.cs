using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace DungeonShooter
{
    public sealed class SoDataSerializerEditorWindow : EditorWindow
    {
        private int _selectedIndex;
        private string _csvPath;
        private DefaultAsset _soFolder;
        private List<Type> _dtoTypes;
        private string[] _dtoTypeNames;
        private bool? _lastResult;

        [MenuItem("DungeonShooter/CSV Serializer")]
        private static void Open()
        {
            GetWindow<SoDataSerializerEditorWindow>("CSV Serializer");
        }

        private void OnEnable()
        {
            RefreshDtoTypes();
        }

        private void OnGUI()
        {
            if (_dtoTypes == null || _dtoTypes.Count == 0)
            {
                EditorGUILayout.HelpBox("CsvDtoForAttribute가 붙은 DTO를 찾지 못했습니다.", MessageType.Warning);
                if (GUILayout.Button("리스트 새로고침"))
                    RefreshDtoTypes();
                return;
            }

            EditorGUILayout.LabelField("DTO 선택", EditorStyles.boldLabel);
            _selectedIndex = EditorGUILayout.Popup(_selectedIndex, _dtoTypeNames);

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

            var dtoType = _dtoTypes[Mathf.Clamp(_selectedIndex, 0, _dtoTypes.Count - 1)];
            var folderPath = _soFolder != null ? AssetDatabase.GetAssetPath(_soFolder) : string.Empty;

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("CSV -> SO"))
                {
                    _lastResult = CSVSerializer.CsvToSo(dtoType, _csvPath, folderPath);
                }

                if (GUILayout.Button("SO -> CSV"))
                {
                    var outputPath = _csvPath;
                    if (string.IsNullOrWhiteSpace(outputPath))
                    {
                        outputPath = Path.Combine(Application.dataPath, $"{dtoType.Name}.csv");
                        outputPath = outputPath.Replace("\\", "/");
                    }

                    _lastResult = CSVSerializer.SoToCsv(dtoType, folderPath, outputPath);
                }
            }

            if (_lastResult.HasValue)
            {
                EditorGUILayout.Space(8);
                EditorGUILayout.HelpBox(_lastResult.Value ? "성공" : "실패", _lastResult.Value ? MessageType.Info : MessageType.Error);
            }

            EditorGUILayout.Space(12);
            if (GUILayout.Button("리스트 새로고침"))
                RefreshDtoTypes();
        }

        private void RefreshDtoTypes()
        {
            _dtoTypes = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(a =>
                {
                    try { return a.GetTypes(); }
                    catch { return Array.Empty<Type>(); }
                })
                .Where(t => t != null && t.IsClass && !t.IsAbstract)
                .Where(t => t.GetCustomAttribute<CsvDtoForAttribute>() != null)
                .OrderBy(t => t.FullName, StringComparer.Ordinal)
                .ToList();

            _dtoTypeNames = _dtoTypes.Select(t => t.FullName).ToArray();
            _selectedIndex = Mathf.Clamp(_selectedIndex, 0, Math.Max(0, _dtoTypes.Count - 1));
        }
    }
}

