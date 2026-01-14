#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace DungeonShooter
{
    [CustomEditor(typeof(RoomEditor))]
    public class RoomEditorEditor : Editor
    {
        private SerializedProperty _exampleTileProperty;
        private SerializedProperty _roomSizeXProperty;
        private SerializedProperty _roomSizeYProperty;

        private void OnEnable()
        {
            _exampleTileProperty = serializedObject.FindProperty("_exampleTile");
            _roomSizeXProperty = serializedObject.FindProperty("_roomSizeX");
            _roomSizeYProperty = serializedObject.FindProperty("_roomSizeY");
        }

        public override void OnInspectorGUI()
        {
            var roomEditor = (RoomEditor)target;
            
            serializedObject.Update();

            // 방 크기 설정 섹션
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("방 크기 설정, ExampleTile은 실제 게임에 생성되는 타일이 아닙니다.", EditorStyles.boldLabel);
            
            EditorGUILayout.PropertyField(_exampleTileProperty);
            EditorGUILayout.IntSlider(_roomSizeXProperty, RoomConstants.ROOM_SIZE_MIN_X, RoomConstants.ROOM_SIZE_MAX_X, "방 크기 X");
            EditorGUILayout.IntSlider(_roomSizeYProperty, RoomConstants.ROOM_SIZE_MIN_Y, RoomConstants.ROOM_SIZE_MAX_Y, "방 크기 Y");
            
            serializedObject.ApplyModifiedProperties();
            
            if (GUILayout.Button("타일 업데이트", GUILayout.Height(25)))
            {
                roomEditor.UpdateRoomSizeTiles();
            }

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
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("파일 이름", GUILayout.Width(80));
            var fileName = EditorGUILayout.TextField(roomEditor.FileName);
            roomEditor.SetFileName(fileName);
            EditorGUILayout.EndHorizontal();
            
            if (string.IsNullOrEmpty(roomEditor.FileName))
            {
                EditorGUILayout.HelpBox("파일 이름이 비어있으면 게임오브젝트 이름이 사용됩니다.", MessageType.Info);
            }
            
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

            // 리셋 섹션
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("리셋", EditorStyles.boldLabel);
            
            if (GUILayout.Button("초기 상태로 리셋", GUILayout.Height(30)))
            {
                if (EditorUtility.DisplayDialog(
                    "방 리셋 확인",
                    "생성된 타일맵과 오브젝트를 모두 제거하고 초기 상태로 돌아갑니다. 계속하시겠습니까?",
                    "확인",
                    "취소"))
                {
                    roomEditor.ResetRoom();
                }
            }

            EditorGUILayout.Space();
        }
    }
}
#endif

