#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace DungeonShooter
{
    [CustomEditor(typeof(RoomEditor))]
    public class RoomEditorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var roomEditor = (RoomEditor)target;

            // 저장 섹션
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("저장", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("저장 경로", GUILayout.Width(80));
            roomEditor.SetSavePath(EditorGUILayout.TextField(roomEditor.SavePath));
            
            if (GUILayout.Button("선택", GUILayout.Width(50)))
            {
                var path = EditorUtility.OpenFolderPanel("저장 경로 선택", "Assets", "");
                if (!string.IsNullOrEmpty(path))
                {
                    // Assets 폴더 기준 상대 경로로 변환
                    if (path.StartsWith(Application.dataPath))
                    {
                        path = "Assets" + path.Substring(Application.dataPath.Length);
                    }
                    roomEditor.SetSavePath(path);
                }
            }
            EditorGUILayout.EndHorizontal();
            
            if (GUILayout.Button("방 저장", GUILayout.Height(30)))
            {
                roomEditor.SaveMap();
            }

            EditorGUILayout.Space(10);

            // 로드 섹션
            EditorGUILayout.LabelField("로드", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("불러오기 경로", GUILayout.Width(80));
            roomEditor.SetLoadPath(EditorGUILayout.TextField(roomEditor.LoadPath));
            
            if (GUILayout.Button("선택", GUILayout.Width(50)))
            {
                var path = EditorUtility.OpenFilePanel("불러오기 경로 선택", "Assets", "json");
                if (!string.IsNullOrEmpty(path))
                {
                    // Assets 폴더 기준 상대 경로로 변환
                    if (path.StartsWith(Application.dataPath))
                    {
                        path = "Assets" + path.Substring(Application.dataPath.Length);
                    }
                    roomEditor.SetLoadPath(path);
                }
            }
            EditorGUILayout.EndHorizontal();
            
            if (GUILayout.Button("방 불러오기", GUILayout.Height(30)))
            {
                roomEditor.LoadRoom();
            }

            EditorGUILayout.Space();
        }
    }
}
#endif

