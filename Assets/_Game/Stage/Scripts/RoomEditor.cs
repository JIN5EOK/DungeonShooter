#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;

namespace DungeonShooter
{
    /// <summary>
    /// 방 에디팅을 위한 에디터 전용 컴포넌트
    /// </summary>
    [ExecuteInEditMode]
    public class RoomEditor : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("방 크기 표시를 위해 설치할 타일맵")]
        private TileBase _exampleTile;

        [SerializeField] 
        private int _roomSizeX;
        [SerializeField] 
        private int _roomSizeY;
        
        [SerializeField]
        [Tooltip("저장 경로")]
        private string _savePath = "Assets/";

        [SerializeField]
        [Tooltip("저장할 파일 이름 (확장자 제외, 비어있으면 게임오브젝트 이름 사용)")]
        private string _fileName;

        [SerializeField]
        [Tooltip("불러오기 경로")]
        private string _loadPath;

        public string SavePath => _savePath;
        public string LoadPath => _loadPath;
        public string FileName => _fileName;
        public int RoomSizeX => _roomSizeX;
        public int RoomSizeY => _roomSizeY;

        public void SetSavePath(string path) => _savePath = path;
        public void SetLoadPath(string path) => _loadPath = path;
        public void SetFileName(string fileName) => _fileName = fileName;

        /// <summary>
        /// Tilemaps와 Objects 게임 오브젝트 자식 구조를 생성합니다.
        /// </summary>
        private void EnsureStructure()
        {
            RoomTilemapHelper.ClearRoomObject(gameObject);
            UpdateRoomSizeTiles();
        }

        public void SaveMap()
        {
            if (!ValidateEditorMode()) return;

            var roomData = RoomDataSerializer.SerializeRoom(gameObject, _roomSizeX, _roomSizeY);
            if (roomData == null)
            {
                Debug.LogError($"[{nameof(RoomEditor)}] 방 직렬화에 실패했습니다.");
                return;
            }

            var fileName = string.IsNullOrEmpty(_fileName) ? gameObject.name : _fileName;
            if (!fileName.EndsWith(".json"))
            {
                fileName += ".json";
            }
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
        /// 방 크기에 맞춰 타일을 배치합니다.
        /// </summary>
        public void UpdateRoomSizeTiles()
        {
            if (!ValidateEditorMode()) return;

            if (_exampleTile == null)
            {
                Debug.LogWarning($"[{nameof(RoomEditor)}] 예제 타일이 설정되지 않았습니다.");
                return;
            }

            // Tilemap_Ground 찾기 또는 생성
            var tilemap = RoomTilemapHelper.GetOrCreateRoomStructure(this.gameObject, null, gameObject.name);
            var groundTilemap = RoomTilemapHelper.GetOrCreateTilemap(tilemap.tilemapsParent, RoomConstants.TILEMAP_GROUND_NAME);
            
            // 기존 타일 모두 제거
            groundTilemap.ClearAllTiles();
            
            // 중앙 기준으로 타일 배치 (Transform 중앙이 (0,0)이 되도록)
            var startX = -_roomSizeX / 2;
            var startY = -_roomSizeY / 2;
            
            for (int x = 0; x < _roomSizeX; x++)
            {
                for (int y = 0; y < _roomSizeY; y++)
                {
                    var position = new Vector3Int(startX + x, startY + y, 0);
                    groundTilemap.SetTile(position, _exampleTile);
                }
            }

            EditorUtility.SetDirty(this);
            EditorUtility.SetDirty(tilemap.tilemapsParent);
        }

        /// <summary>
        /// 방을 초기 상태로 리셋합니다.
        /// 생성된 타일맵을 모두 제거하고, Objects 하위의 오브젝트들을 모두 제거합니다.
        /// </summary>
        public void ResetRoom()
        {
            if (!ValidateEditorMode()) return;


            // 구조 재생성
            EnsureStructure();

            EditorUtility.SetDirty(this);
            Debug.Log($"[{nameof(RoomEditor)}] 방이 초기 상태로 리셋되었습니다.");
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

