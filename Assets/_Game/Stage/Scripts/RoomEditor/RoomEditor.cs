#if UNITY_EDITOR
using Jin5eok;
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
        [Tooltip("방 에디터 프리셋 리소스 설정")]
        private EditorStageResourceProvider _resourceProvider;

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

        public string SavePath => _savePath;
        public TextAsset LoadFile => _loadFile;
        public string FileName => _fileName;
        public int RoomSizeX => _roomSizeX;
        public int RoomSizeY => _roomSizeY;

        public void SetSavePath(string path) => _savePath = path;
        public void SetLoadFile(TextAsset file) => _loadFile = file;
        public void SetFileName(string fileName) => _fileName = fileName;

        /// <summary>
        /// Tilemaps와 Objects 게임 오브젝트 자식 구조를 생성합니다.
        /// </summary>
        private void EnsureStructure()
        {
            RoomCreateHelper.ClearRoomObject(transform);
            RoomCreateHelper.ClearTiles(transform);
            UpdateRoomSizeTiles();
            // 타일 팔레트로 게임오브젝트 배치하기 위해 에디터 한정으로 Tilemap 컴포넌트 추가  
            RoomCreateHelper.GetOrCreateChild(transform, RoomConstants.OBJECTS_GAMEOBJECT_NAME).gameObject.AddOrGetComponent<Tilemap>();
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

            EnsureStructure();

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

            // Room 구조 생성
            RoomCreateHelper.GetOrCreateRoomStructure(transform, gameObject.name);
            
            // 방 크기 업데이트
            _roomSizeX = roomData.RoomSizeX;
            _roomSizeY = roomData.RoomSizeY;

            var centerPos = Vector2.zero; // Room 레벨에서는 중심이 (0,0)

            // 맵 배치
            RoomCreateHelper.ClearTiles(gameObject.transform);

            RoomCreateHelper.PlaceBaseTilesSync(gameObject.transform, centerPos, roomData, _resourceProvider);
            RoomCreateHelper.PlaceAdditionalTilesSync(gameObject.transform, centerPos, roomData, _resourceProvider);
            RoomCreateHelper.PlaceObjectsSync(gameObject.transform, roomData, _resourceProvider);

            EditorUtility.SetDirty(this);
            LogHandler.Log<RoomEditor>($"방 불러오기 완료: {_loadFile.name}");
        }


        /// <summary>
        /// 방 크기에 맞춰 타일을 배치합니다.
        /// </summary>
        public void UpdateRoomSizeTiles()
        {
            if (!ValidateEditorMode()) return;

            if (_resourceProvider == null)
            {
                LogHandler.LogError<RoomEditor>("ResourceProvider가 설정되지 않았습니다.");
                return;
            }

            // 임시 RoomData 생성 (방 크기만 설정)
            var tempRoomData = new RoomData
            {
                RoomSizeX = _roomSizeX,
                RoomSizeY = _roomSizeY
            };
            RoomCreateHelper.ClearTiles(transform);
            // Room 구조 생성
            RoomCreateHelper.GetOrCreateRoomStructure(transform, gameObject.name);

            // 베이스 타일 배치 (동기적으로 실행)
            var centerPos = Vector2.zero; // Room 레벨에서는 중심이 (0,0)
            RoomCreateHelper.PlaceBaseTilesSync(this.transform, centerPos, tempRoomData, _resourceProvider);

            EditorUtility.SetDirty(this);
            var tilemapsParent = this.transform.Find(RoomConstants.TILEMAPS_GAMEOBJECT_NAME);
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
            EnsureStructure();

            EditorUtility.SetDirty(this);
            LogHandler.Log<RoomEditor>("방이 초기 상태로 리셋되었습니다.");
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

