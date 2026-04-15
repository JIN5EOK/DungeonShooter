using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace DungeonShooter
{
    public sealed class SoDataSerializerEditorWindow : EditorWindow
    {
        private DefaultAsset _soFolder;
        private bool? _lastResult;

        [MenuItem("DungeonShooter/CSV Serializer")]
        private static void Open()
        {
            GetWindow<SoDataSerializerEditorWindow>("CSV Serializer");
        }

        private void OnGUI()
        {
            EditorGUILayout.HelpBox("스크립터블 오브젝트 <-> CSV 파일 변환기", MessageType.None);
            EditorGUILayout.LabelField($"설정", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("변환시 사용할 스크립터블 오브젝트가 위치한 경로 설정", MessageType.Info);
            EditorGUILayout.Space(8);
            _soFolder = (DefaultAsset)EditorGUILayout.ObjectField("스크립터블 오브젝트 경로", _soFolder, typeof(DefaultAsset), false);
            EditorGUILayout.Space(8);
            
            
            EditorGUILayout.Space(12);

            AddSerializeMenu<FooSo, SerializedFoo>();
            AddSerializeMenu<SkillTableEntrySo, SerializedSkillTableDto>();
            
            if (_lastResult.HasValue)
            {
                EditorGUILayout.Space(8);
                EditorGUILayout.HelpBox(_lastResult.Value ? "성공" : "실패", _lastResult.Value ? MessageType.Info : MessageType.Error);
            }   
        }
        
        private void AddSerializeMenu<TSo, TSerialized>()
            where TSo : ScriptableObject, ISerializableObject<TSerialized>
            where TSerialized : class, ITableEntry, new()
        {
            EditorGUILayout.Space(12);
            EditorGUILayout.LabelField($"{typeof(TSo).Name} / CSV 변환", EditorStyles.boldLabel);

            EditorGUILayout.Space(12);

            // SOToCSV
            
            var folderPath = _soFolder != null ? AssetDatabase.GetAssetPath(_soFolder) : string.Empty;

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button($"CSV -> {typeof(TSo).Name} 변환"))
                {
                    var picked = EditorUtility.OpenFilePanel("CSV 선택", Application.dataPath, "csv");
                    if (string.IsNullOrEmpty(picked))
                        return;

                    _lastResult = SoCSVPipeline.CsvToSo<TSo, TSerialized>(picked, folderPath);
                }

                if (GUILayout.Button($"{typeof(TSo).Name} -> CSV 변환"))
                {
                    var defaultName = $"{typeof(TSo).Name}.csv";
                    var picked = EditorUtility.SaveFilePanel("CSV 저장", Application.dataPath, defaultName, "csv");
                    if (string.IsNullOrEmpty(picked))
                        return;

                    var outputPath = picked;

                    _lastResult = SoCSVPipeline.SoToCsv<TSo, TSerialized>(folderPath, outputPath);
                }
            }
        }
    }
    
}

