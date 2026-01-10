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
        [Tooltip("불러오기 경로")]
        private string _loadPath;

        public string SavePath => _savePath;
        public string LoadPath => _loadPath;
        public int RoomSizeX => _roomSizeX;
        public int RoomSizeY => _roomSizeY;

        public void SetSavePath(string path) => _savePath = path;
        public void SetLoadPath(string path) => _loadPath = path;

        public void UpdateRoomSizeTilesPublic()
        {
            UpdateRoomSizeTiles();
        }

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

            var roomData = RoomDataSerializer.SerializeRoom(gameObject, _roomSizeX, _roomSizeY);
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
        /// 방 크기에 맞춰 BaseTilemap_Ground에 타일을 배치합니다.
        /// </summary>
        private void UpdateRoomSizeTiles()
        {
            if (!ValidateEditorMode()) return;

            if (_exampleTile == null)
            {
                Debug.LogWarning($"[{nameof(RoomEditor)}] 예제 타일이 설정되지 않았습니다.");
                return;
            }

            // BaseTilemaps 찾기 또는 생성
            var baseTilemapsTransform = transform.Find("BaseTilemaps");
            if (baseTilemapsTransform == null)
            {
                var baseTilemaps = new GameObject("BaseTilemaps");
                baseTilemaps.transform.SetParent(transform);
                baseTilemapsTransform = baseTilemaps.transform;
            }

            // BaseTilemap_Ground 찾기 또는 생성
            var baseTilemapGroundTransform = baseTilemapsTransform.Find("BaseTilemap_Ground");
            Tilemap tilemap;
            
            if (baseTilemapGroundTransform == null)
            {
                var baseTilemapGround = new GameObject("BaseTilemap_Ground");
                baseTilemapGround.transform.SetParent(baseTilemapsTransform);
                tilemap = baseTilemapGround.AddComponent<Tilemap>();
                baseTilemapGround.AddComponent<TilemapRenderer>();
            }
            else
            {
                tilemap = baseTilemapGroundTransform.GetComponent<Tilemap>();
                if (tilemap == null)
                {
                    tilemap = baseTilemapGroundTransform.gameObject.AddComponent<Tilemap>();
                    baseTilemapGroundTransform.gameObject.AddComponent<TilemapRenderer>();
                }
            }

            // 기존 타일 모두 제거
            tilemap.ClearAllTiles();

            // 중앙 기준으로 타일 배치 (Transform 중앙이 (0,0)이 되도록)
            var startX = -_roomSizeX / 2;
            var startY = -_roomSizeY / 2;
            
            for (int x = 0; x < _roomSizeX; x++)
            {
                for (int y = 0; y < _roomSizeY; y++)
                {
                    var position = new Vector3Int(startX + x, startY + y, 0);
                    tilemap.SetTile(position, _exampleTile);
                }
            }

            EditorUtility.SetDirty(this);
            EditorUtility.SetDirty(tilemap);
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

