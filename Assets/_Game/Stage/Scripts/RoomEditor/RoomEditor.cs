#if UNITY_EDITOR
using Jin5eok;
using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using UnityEngine.AddressableAssets;

namespace DungeonShooter
{
    /// <summary>
    /// 방 에디팅을 위한 에디터 전용 컴포넌트
    /// </summary>
    [ExecuteInEditMode]
    public class RoomEditor : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("방 에디터 프리셋 리소스 설정")]
        private SceneResourceProviderEditor _resourceProvider;

        [SerializeField]
        [Tooltip("Ground 타일 (방 크기 표시용)")]
        private AssetReferenceT<TileBase> _groundTile;

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
        [Tooltip("불러올 방 데이터 파일 (TextAsset)")]
        private TextAsset _loadFile;

        [SerializeField]
        [Tooltip("배치할 오브젝트 테이블 ID (인스펙터에서 선택 후 씬 뷰 클릭으로 배치)")]
        private int _selectedPlaceableTableId;

        public string SavePath => _savePath;
        public int SelectedPlaceableTableId { get => _selectedPlaceableTableId; set => _selectedPlaceableTableId = value; }
        public TextAsset LoadFile => _loadFile;
        public string FileName => _fileName;
        public int RoomSizeX => _roomSizeX;
        public int RoomSizeY => _roomSizeY;

        private ITableRepository _tableRepository;
        private RoomInstantiator _roomInstantiator;

        public ITableRepository GetOrCreateTableRepository()
        {
            if (_tableRepository == null)
            {
                _tableRepository = new LocalTableRepository();
            }
            return _tableRepository;
        }

        private RoomInstantiator GetRoomInstantiator()
        {
            if (_roomInstantiator == null && _resourceProvider != null)
            {
                _roomInstantiator = new RoomInstantiator(null, null, _resourceProvider, GetOrCreateTableRepository());
            }
            return _roomInstantiator;
        }

        public void SetSavePath(string path) => _savePath = path;
        public void SetLoadFile(TextAsset file) => _loadFile = file;
        public void SetFileName(string fileName) => _fileName = fileName;

        /// <summary>
        /// Tilemaps와 Objects 게임 오브젝트 자식 구조를 생성합니다.
        /// </summary>
        private void InitializeRoom()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(transform.GetChild(i).gameObject);
            }
            UpdateRoom();
        }

        public void SaveMap()
        {
            if (!ValidateEditorMode()) return;

            var roomData = RoomDataSerializer.SerializeRoom(gameObject, _roomSizeX, _roomSizeY);
            if (roomData == null)
            {
                LogHandler.LogError<RoomEditor>("방 직렬화에 실패했습니다.");
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

            if (_loadFile == null)
            {
                LogHandler.LogError<RoomEditor>("불러올 파일이 지정되지 않았습니다.");
                return;
            }

            InitializeRoom();

            var roomData = RoomDataSerializer.DeserializeRoom(_loadFile);
            if (roomData == null)
            {
                LogHandler.LogError<RoomEditor>("방 역직렬화에 실패했습니다.");
                return;
            }

            if (_resourceProvider == null)
            {
                LogHandler.LogError<RoomEditor>("ResourceProvider가 설정되지 않았습니다.");
                return;
            }

            // 로드한 파일의 경로와 이름으로 저장 경로 및 파일 이름 갱신
            var filePath = AssetDatabase.GetAssetPath(_loadFile);
            if (!string.IsNullOrEmpty(filePath))
            {
                var directory = System.IO.Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory))
                {
                    _savePath = directory + "/";
                }

                var fileNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(filePath);
                if (!string.IsNullOrEmpty(fileNameWithoutExtension))
                {
                    _fileName = fileNameWithoutExtension;
                }
            }

            var roomInstantiator = GetRoomInstantiator();
            if (roomInstantiator == null)
            {
                return;
            }

            roomInstantiator.GetOrCreateRoomStructure(transform, gameObject.name);

            _roomSizeX = roomData.RoomSizeX;
            _roomSizeY = roomData.RoomSizeY;

            var centerPos = Vector2.zero;

            roomInstantiator.ClearTiles(gameObject.transform);

            var groundTile = LoadGroundTile();
            if (groundTile == null)
            {
                LogHandler.LogError<RoomEditor>("Ground 타일을 로드할 수 없습니다.");
                return;
            }

            roomInstantiator.PlaceBaseTiles(gameObject.transform, centerPos, roomData, groundTile);
            roomInstantiator.PlaceAdditionalTilesSync(gameObject.transform, centerPos, roomData);
            roomInstantiator.PlaceObjectsSync(gameObject.transform, roomData);
            roomInstantiator.GetOrCreateChild(transform, RoomConstants.ObjectsGameObjectName).gameObject.AddOrGetComponent<Tilemap>();
            EditorUtility.SetDirty(this);
            LogHandler.Log<RoomEditor>($"방 불러오기 완료: {_loadFile.name}");
        }


        /// <summary>
        /// 방 크기에 맞춰 타일을 배치합니다.
        /// </summary>
        public void UpdateRoom()
        {
            if (!ValidateEditorMode()) return;

            if (_resourceProvider == null)
            {
                LogHandler.LogError<RoomEditor>("ResourceProvider가 설정되지 않았습니다.");
                return;
            }

            var tempRoomData = new RoomData
            {
                RoomSizeX = _roomSizeX,
                RoomSizeY = _roomSizeY
            };

            var helper = GetRoomInstantiator();
            if (helper == null)
            {
                return;
            }

            helper.ClearTiles(transform);
            helper.GetOrCreateRoomStructure(transform, gameObject.name);

            var centerPos = Vector2.zero;

            var groundTile = LoadGroundTile();
            if (groundTile == null)
            {
                LogHandler.LogError<RoomEditor>("Ground 타일을 로드할 수 없습니다.");
                return;
            }

            helper.PlaceBaseTiles(this.transform, centerPos, tempRoomData, groundTile);

            EditorUtility.SetDirty(this);
            var tilemapsParent = this.transform.Find(RoomConstants.TilemapGameObjectName);
            if (tilemapsParent != null)
            {
                EditorUtility.SetDirty(tilemapsParent);
            }
        }

        /// <summary>
        /// 방을 초기 상태로 리셋합니다.
        /// 생성된 타일맵을 모두 제거하고, Objects 하위의 오브젝트들을 모두 제거합니다.
        /// </summary>
        public void ResetRoom()
        {
            if (!ValidateEditorMode()) return;


            // 구조 재생성
            InitializeRoom();

            EditorUtility.SetDirty(this);
            LogHandler.Log<RoomEditor>("방이 초기 상태로 리셋되었습니다.");
        }

        /// <summary>
        /// Ground 타일을 로드합니다.
        /// </summary>
        private TileBase LoadGroundTile()
        {
            if (_groundTile == null || !_groundTile.RuntimeKeyIsValid())
            {
                LogHandler.LogWarning<RoomEditor>("Ground 타일이 설정되지 않았습니다.");
                return null;
            }

            if (_resourceProvider == null)
            {
                LogHandler.LogError<RoomEditor>("ResourceProvider가 설정되지 않았습니다.");
                return null;
            }

            var address = _groundTile.RuntimeKey.ToString();
            return _resourceProvider.GetAssetSync<TileBase>(address);
        }

        /// <summary>
        /// 선택한 테이블 ID로 오브젝트를 지정 위치에 배치합니다. (에디터 전용)
        /// </summary>
        /// <param name="tableId">RoomEventTriggerTableEntry, EnemyConfigTableEntry 또는 PlayerConfigTableEntry ID</param>
        /// <param name="worldPosition">월드 위치 (Z는 0으로 사용)</param>
        /// <returns>생성된 인스턴스, 실패 시 null</returns>
        public GameObject PlaceObjectAt(int tableId, Vector3 worldPosition)
        {
            if (tableId == 0)
            {
                return null;
            }
            if (_resourceProvider == null)
            {
                LogHandler.LogError<RoomEditor>("ResourceProvider가 설정되지 않았습니다.");
                return null;
            }

            var tableRepo = GetOrCreateTableRepository();
            var entry = tableRepo.GetTableEntry(tableId);
            if (entry == null)
            {
                LogHandler.LogWarning<RoomEditor>($"테이블 엔트리를 찾을 수 없습니다. ID: {tableId}");
                return null;
            }

            GameObject instance;
            if (entry is RoomEventTriggerTableEntry eventTriggerEntry)
            {
                var nameText = tableRepo.GetStringText(eventTriggerEntry.NameId);
                instance = new GameObject($"[EventTrigger] {nameText} (ID:{tableId})");
            }
            else
            {
                var address = entry is EnemyConfigTableEntry eConfig ? eConfig.GameObjectKey
                    : entry is PlayerConfigTableEntry pConfig ? pConfig.GameObjectKey
                    : null;
                
                if (string.IsNullOrEmpty(address))
                {
                    return null;
                }
                instance = _resourceProvider.GetInstanceSync(address);
                if (instance == null)
                {
                    return null;
                }
            }

            var marker = instance.AddOrGetComponent<RoomObjectMarker>();
            marker.TableId = tableId;

            var helper = new RoomInstantiator();
            var objectsParent = helper.GetOrCreateChild(transform, RoomConstants.ObjectsGameObjectName);
            instance.transform.SetParent(objectsParent);
            instance.transform.position = new Vector3(worldPosition.x, worldPosition.y, 0);
            instance.transform.rotation = Quaternion.identity;

            EditorUtility.SetDirty(this);
            return instance;
        }

        /// <summary>
        /// 에디터 모드인지 확인합니다.
        /// </summary>
        private bool ValidateEditorMode()
        {
            if (Application.isPlaying)
            {
                LogHandler.LogWarning<RoomEditor>("플레이 모드에서는 사용할 수 없습니다.");
                return false;
            }
            return true;
        }

    }
}
#endif

