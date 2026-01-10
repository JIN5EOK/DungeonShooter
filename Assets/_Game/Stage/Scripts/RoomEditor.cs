#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace DungeonShooter
{
    /// <summary>
    /// 방 에디팅을 위한 에디터 전용 컴포넌트
    /// </summary>
    [ExecuteInEditMode]
    public class RoomEditor : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("저장 경로")]
        private string _savePath = "Assets/";

        [SerializeField]
        [Tooltip("불러오기 경로")]
        private string _loadPath;

        public string SavePath => _savePath;
        public string LoadPath => _loadPath;

        public void SetSavePath(string path) => _savePath = path;
        public void SetLoadPath(string path) => _loadPath = path;

        private void OnEnable()
        {
            if (Application.isPlaying)
            {
                return;
            }

            EnsureStructure();
        }

        /// <summary>
        /// Tilemaps와 Objects 자식 구조를 확인하고 없으면 생성합니다.
        /// </summary>
        private void EnsureStructure()
        {
            // Tilemaps 자식 확인 및 생성
            var tilemapsTransform = transform.Find(RoomConstants.TILEMAPS_GAMEOBJECT_NAME);
            if (tilemapsTransform == null)
            {
                var tilemaps = new GameObject(RoomConstants.TILEMAPS_GAMEOBJECT_NAME);
                tilemaps.transform.SetParent(transform);
                tilemaps.AddComponent<Grid>();
            }
            else
            {
                // Grid 컴포넌트 확인
                if (tilemapsTransform.GetComponent<Grid>() == null)
                {
                    tilemapsTransform.gameObject.AddComponent<Grid>();
                }
            }

            // Objects 자식 확인 및 생성
            if (transform.Find(RoomConstants.OBJECTS_GAMEOBJECT_NAME) == null)
            {
                var objects = new GameObject(RoomConstants.OBJECTS_GAMEOBJECT_NAME);
                objects.transform.SetParent(transform);
            }
        }

        public void SaveMap()
        {
            if (!ValidateEditorMode()) return;

            var roomData = RoomDataSerializer.SerializeRoom(gameObject);
            if (roomData == null)
            {
                Debug.LogError($"[{nameof(RoomEditor)}] 방 직렬화에 실패했습니다.");
                return;
            }

            var fileName = $"{gameObject.name}.json";
            var fullPath = System.IO.Path.Combine(_savePath, fileName);
            RoomDataSerializer.SaveToFile(roomData, fullPath);

            EditorUtility.SetDirty(this);
            AssetDatabase.Refresh();
        }

        public void LoadRoom()
        {
            if (!ValidateEditorMode()) return;

            if (string.IsNullOrEmpty(_loadPath))
            {
                Debug.LogError($"[{nameof(RoomEditor)}] 불러올 파일 경로가 지정되지 않았습니다.");
                return;
            }

            EnsureStructure();

            // 파일 경로에서 TextAsset 로드
            var textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(_loadPath);
            if (textAsset == null)
            {
                Debug.LogError($"[{nameof(RoomEditor)}] 파일을 찾을 수 없습니다: {_loadPath}");
                return;
            }

            var roomData = RoomDataSerializer.DeserializeRoom(textAsset);
            if (roomData == null)
            {
                Debug.LogError($"[{nameof(RoomEditor)}] 방 역직렬화에 실패했습니다.");
                return;
            }

            // StageInstantiator를 사용하여 데이터 불러오기
            StageInstantiator.InstantiateRoomEditor(roomData, roomObj: gameObject);

            EditorUtility.SetDirty(this);
            Debug.Log($"[{nameof(RoomEditor)}] 방 불러오기 완료: {_loadPath}");
        }


        /// <summary>
        /// 에디터 모드인지 확인합니다.
        /// </summary>
        private bool ValidateEditorMode()
        {
            if (Application.isPlaying)
            {
                Debug.LogWarning($"[{nameof(RoomEditor)}] 플레이 모드에서는 사용할 수 없습니다.");
                return false;
            }
            return true;
        }

    }
}
#endif

