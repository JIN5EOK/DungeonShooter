using System.Collections.Generic;
using System.Threading.Tasks;
using Jin5eok;
using UnityEngine;
using UnityEngine.Tilemaps;
using VContainer;

namespace DungeonShooter
{
    /// <summary>
    /// Room 오브젝트 계층구조 및 타일,오브젝트 생성 유틸리티
    /// </summary>
    public class RoomInstantiator
    {
        private readonly IPlayerFactory _playerFactory;
        private readonly IEnemyFactory _enemyFactory;
        private readonly ISceneResourceProvider _sceneResourceProvider;
        private readonly ITableRepository _tableRepository;
        private Transform _stageRoot;

        /// <summary>
        /// 구조 전용 메서드(GetOrCreateChild, GetOrCreateTilemap 등)만 사용할 때 사용합니다. (에디터/직렬화)
        /// </summary>
        public RoomInstantiator()
        {
            _playerFactory = null;
            _enemyFactory = null;
            _sceneResourceProvider = null;
            _tableRepository = null;
        }

        [Inject]
        public RoomInstantiator(
            IPlayerFactory playerFactory,
            IEnemyFactory enemyFactory,
            ISceneResourceProvider sceneResourceProvider,
            ITableRepository tableRepository)
        {
            _playerFactory = playerFactory;
            _enemyFactory = enemyFactory;
            _sceneResourceProvider = sceneResourceProvider;
            _tableRepository = tableRepository;
        }

        /// <summary>
        /// Room의 전체 계층 구조를 생성하거나 가져옵니다.
        /// </summary>
        public void GetOrCreateRoomStructure(Transform stageRoot = null, string roomName = "Stage")
        {
            _stageRoot = stageRoot != null ? stageRoot : new GameObject(roomName).transform;
            var tilemapsParent = GetOrCreateChild(_stageRoot, RoomConstants.TilemapGameObjectName);
            tilemapsParent.gameObject.AddOrGetComponent<Grid>();
            GetOrCreateTilemapInternal(RoomConstants.TilemapGroundName);
            GetOrCreateTilemapInternal(RoomConstants.TilemapDecoName);
            GetOrCreateChild(_stageRoot, RoomConstants.ObjectsGameObjectName);
        }

        /// <summary>
        /// 지정된 이름의 자식 GameObject를 찾거나 생성합니다.
        /// </summary>
        public Transform GetOrCreateChild(Transform parent, string childName)
        {
            var child = parent.Find(childName);
            if (child == null)
            {
                var childObj = new GameObject(childName);
                childObj.transform.SetParent(parent);
                child = childObj.transform;
            }
            return child;
        }

        private RenderingLayer GetRenderingLayerForTilemap(string tilemapName)
        {
            return tilemapName.Contains(RenderingLayers.Ground.LayerName) ? RenderingLayers.Ground
                : tilemapName.Contains(RenderingLayers.Wall.LayerName) ? RenderingLayers.Wall
                : tilemapName.Contains(RenderingLayers.Deco.LayerName) ? RenderingLayers.Deco
                : RenderingLayers.Ground;
        }

        /// <summary>
        /// 지정된 이름의 Tilemap을 찾거나 생성합니다.
        /// </summary>
        public Tilemap GetOrCreateTilemap(Transform stageRoot, string tilemapName)
        {
            _stageRoot = stageRoot;
            return GetOrCreateTilemapInternal(tilemapName);
        }

        private Tilemap GetOrCreateTilemapInternal(string tilemapName)
        {
            var tilemapRoot = GetOrCreateChild(_stageRoot, RoomConstants.TilemapGameObjectName);
            var tilemapObj = GetOrCreateChild(tilemapRoot, tilemapName);
            var tilemap = tilemapObj.gameObject.AddOrGetComponent<Tilemap>();
            var renderer = tilemapObj.gameObject.AddOrGetComponent<TilemapRenderer>();
            var collider = tilemapObj.gameObject.AddOrGetComponent<TilemapCollider2D>();
            var rigidBody2D = tilemapObj.gameObject.AddOrGetComponent<Rigidbody2D>();
            rigidBody2D.bodyType = RigidbodyType2D.Kinematic;
            tilemapObj.gameObject.AddOrGetComponent<CompositeCollider2D>();

            var renderingLayer = GetRenderingLayerForTilemap(tilemapName);
            renderer.sortingLayerName = renderingLayer.LayerName;
            collider.compositeOperation = Collider2D.CompositeOperation.Merge;

            return tilemap;
        }

        public void ClearTiles(Transform stageRoot)
        {
            _stageRoot = stageRoot;
            var groundTilemap = GetOrCreateTilemapInternal(RoomConstants.TilemapGroundName);
            var decoTilemap = GetOrCreateTilemapInternal(RoomConstants.TilemapDecoName);
            groundTilemap.ClearAllTiles();
            decoTilemap.ClearAllTiles();
        }

        /// <summary>
        /// 베이스 타일 배치 로직
        /// </summary>
        public void PlaceBaseTiles(Transform stageRoot, Vector2 centerPos, RoomData roomData, TileBase groundTile)
        {
            _stageRoot = stageRoot;
            var roomSizeX = roomData.RoomSizeX;
            var roomSizeY = roomData.RoomSizeY;

            GetOrCreateChild(_stageRoot, RoomConstants.TilemapGameObjectName);
            var groundTilemap = GetOrCreateTilemapInternal(RoomConstants.TilemapGroundName);

            var centerPosInt = new Vector3Int((int)centerPos.x, (int)centerPos.y, 0);
            var startX = -roomSizeX / 2;
            var startY = -roomSizeY / 2;

            for (int x = 0; x < roomSizeX; x++)
            {
                for (int y = 0; y < roomSizeY; y++)
                {
                    var localPos = new Vector3Int(startX + x, startY + y, 0);
                    var worldTilePos = localPos + centerPosInt;
                    groundTilemap.SetTile(worldTilePos, groundTile);
                }
            }
        }

        /// <summary>
        /// 추가 타일을 비동기적으로 배치합니다.
        /// </summary>
        public async Task PlaceAdditionalTilesAsync(Transform stageRoot, Vector2 centerPos, RoomData roomData)
        {
            _stageRoot = stageRoot;
            if (!ValidatePlaceAdditionalTiles(roomData))
                return;
            var tileBases = await LoadTileBasesAsync(roomData);
            PlaceAdditionalTilesInternal(centerPos, roomData.Tiles, tileBases);
        }

        /// <summary>
        /// 추가 타일을 동기적으로 배치합니다.
        /// </summary>
        public void PlaceAdditionalTilesSync(Transform stageRoot, Vector2 centerPos, RoomData roomData)
        {
            _stageRoot = stageRoot;
            if (!ValidatePlaceAdditionalTiles(roomData))
                return;
            var tileBases = LoadTileBasesSync(roomData);
            PlaceAdditionalTilesInternal(centerPos, roomData.Tiles, tileBases);
        }

        private bool ValidatePlaceAdditionalTiles(RoomData roomData)
        {
            if (_stageRoot == null || roomData == null || _sceneResourceProvider == null)
            {
                LogHandler.LogError(nameof(RoomInstantiator), "파라미터가 올바르지 않습니다.");
                return false;
            }
            return true;
        }

        private async Task<List<TileBase>> LoadTileBasesAsync(RoomData roomData)
        {
            var list = new List<TileBase>();
            foreach (var tileData in roomData.Tiles)
            {
                var address = roomData.GetAddress(tileData.Index);
                list.Add(await _sceneResourceProvider.GetAssetAsync<TileBase>(address));
            }
            return list;
        }

        private List<TileBase> LoadTileBasesSync(RoomData roomData)
        {
            var list = new List<TileBase>();
            foreach (var tileData in roomData.Tiles)
            {
                var address = roomData.GetAddress(tileData.Index);
                list.Add(_sceneResourceProvider.GetAssetSync<TileBase>(address));
            }
            return list;
        }

        private void PlaceAdditionalTilesInternal(Vector3 centerPos, List<TileLayerData> tileDatas, List<TileBase> tileBases)
        {
            var centerPosInt = new Vector3Int((int)centerPos.x, (int)centerPos.y, 0);

            for (int i = 0; i < tileDatas.Count; i++)
            {
                var sortingLayerName = RenderingLayers.GetLayerName(tileDatas[i].Layer);
                var tilemapName = $"{RoomConstants.TimemapComponentNameBase}{sortingLayerName}";
                var tilemap = GetOrCreateTilemapInternal(tilemapName);
                var localPos = new Vector3Int(tileDatas[i].Position.x, tileDatas[i].Position.y, 0);
                var worldTilePos = localPos + centerPosInt;
                tilemap.SetTile(worldTilePos, tileBases[i]);
            }
        }

        /// <summary>
        /// 오브젝트를 비동기적으로 배치합니다.
        /// </summary>
        public async Task<List<GameObject>> PlaceObjectsAsync(Transform stageRoot, RoomData roomData, Vector3 worldOffset = default)
        {
            _stageRoot = stageRoot;
            var instances = await ResolveAndCreateInstancesAsync(roomData, worldOffset);
            PlaceObjectsInternal(worldOffset, roomData.Objects, instances);
            return instances;
        }

        /// <summary>
        /// 오브젝트를 동기적으로 배치합니다.
        /// </summary>
        public List<GameObject> PlaceObjectsSync(Transform stageRoot, RoomData roomData, Vector3 worldOffset = default)
        {
            _stageRoot = stageRoot;
            var instances = ResolveAndCreateInstancesSync(roomData, worldOffset);
            PlaceObjectsInternal(worldOffset, roomData.Objects, instances);
            return instances;
        }

        private async Task<List<GameObject>> ResolveAndCreateInstancesAsync(RoomData roomData, Vector3 worldOffset)
        {
            var instances = new List<GameObject>();
            foreach (var objectData in roomData.Objects)
            {
                var worldPosition = new Vector3(objectData.Position.x, objectData.Position.y, 0) + worldOffset;
                var instance = objectData.TableId == 0 ? null : await ResolveByTableIdAsync(objectData.TableId, worldPosition, objectData.Rotation);
                if (instance != null && objectData.TableId != 0 && Application.isPlaying == false)
                {
                    var marker = instance.AddOrGetComponent<RoomObjectMarker>();
                    marker.TableId = objectData.TableId;
                }
                instances.Add(instance);
            }
            return instances;
        }

        private List<GameObject> ResolveAndCreateInstancesSync(RoomData roomData, Vector3 worldOffset)
        {
            var instances = new List<GameObject>();
            foreach (var objectData in roomData.Objects)
            {
                var worldPosition = new Vector3(objectData.Position.x, objectData.Position.y, 0) + worldOffset;
                var instance = objectData.TableId == 0 ? null : ResolveByTableIdSync(objectData.TableId, worldPosition, objectData.Rotation);
                if (instance != null && objectData.TableId != 0 && Application.isPlaying == false)
                {
                    var marker = instance.AddOrGetComponent<RoomObjectMarker>();
                    marker.TableId = objectData.TableId;
                }
                instances.Add(instance);
            }
            return instances;
        }

        private async Task<GameObject> ResolveByTableIdAsync(int tableId, Vector3 position, Quaternion rotation)
        {
            if (_tableRepository == null) return null;

            var entry = _tableRepository.GetTableEntry(tableId);
            if (entry == null) return null;

            if (entry is RoomEventTriggerTableEntry eventTriggerEntry)
                return await ResolveRoomEventTriggerEntryAsync(eventTriggerEntry, position, rotation);
            if (entry is EnemyConfigTableEntry enemyConfig)
                return await ResolveEnemyEntryAsync(tableId, enemyConfig, position, rotation);
            return null;
        }

        private GameObject ResolveByTableIdSync(int tableId, Vector3 position, Quaternion rotation)
        {
            if (_tableRepository == null) return null;

            var entry = _tableRepository.GetTableEntry(tableId);
            if (entry == null) return null;
            if (entry is RoomEventTriggerTableEntry eventTriggerEntry)
                return ResolveRoomEventTriggerEntrySync(eventTriggerEntry, position, rotation);
            if (entry is EnemyConfigTableEntry enemyConfig)
                return ResolveEnemyEntrySync(tableId, enemyConfig, position, rotation);
            return null;
        }

        private async Task<GameObject> ResolveEnemyEntryAsync(int tableId, EnemyConfigTableEntry enemyConfig, Vector3 position, Quaternion rotation)
        {
            if (Application.isPlaying && _enemyFactory != null)
            {
                var enemy = await _enemyFactory.GetEnemyByConfigIdAsync(tableId, position, rotation,GetOrCreateChild(_stageRoot, RoomConstants.ObjectsGameObjectName));
                return enemy != null ? enemy.gameObject : null;
            }
            return await _sceneResourceProvider.GetInstanceAsync(enemyConfig.GameObjectKey, position, rotation, GetOrCreateChild(_stageRoot, RoomConstants.ObjectsGameObjectName));
        }

        private GameObject ResolveEnemyEntrySync(int tableId, EnemyConfigTableEntry enemyConfig, Vector3 position, Quaternion rotation)
        {
            if (Application.isPlaying && _enemyFactory != null)
            {
                var enemy = _enemyFactory.GetEnemyByConfigIdSync(tableId, position, rotation,GetOrCreateChild(_stageRoot, RoomConstants.ObjectsGameObjectName));
                return enemy != null ? enemy.gameObject : null;
            }
            return _sceneResourceProvider.GetInstanceSync(enemyConfig.GameObjectKey, position, rotation, GetOrCreateChild(_stageRoot, RoomConstants.ObjectsGameObjectName));
        }

        private async Task<GameObject> ResolveRoomEventTriggerEntryAsync(RoomEventTriggerTableEntry eventTriggerEntry, Vector3 position, Quaternion rotation)
        {
            if (!Application.isPlaying)
            {
                var nameText = _tableRepository?.GetStringText(eventTriggerEntry.NameId) ?? eventTriggerEntry.NameId.ToString();
                var go = new GameObject($"[EventTrigger] {nameText} (ID:{eventTriggerEntry.Id})");
                var marker = go.AddComponent<RoomObjectMarker>();
                marker.TableId = eventTriggerEntry.Id;
                go.transform.SetParent(GetOrCreateChild(_stageRoot, RoomConstants.ObjectsGameObjectName));
                return go;
            }
            if (!System.Enum.IsDefined(typeof(RoomEventTriggerType), eventTriggerEntry.Id))
                return null;
            var triggerType = (RoomEventTriggerType)eventTriggerEntry.Id;
            switch (triggerType)
            {
                case RoomEventTriggerType.PlayerSpawnPoint:
                    return (await _playerFactory.GetPlayerAsync(position, rotation,GetOrCreateChild(_stageRoot, RoomConstants.ObjectsGameObjectName))).gameObject;
                case RoomEventTriggerType.RandomEnemySpawn:
                    return (await _enemyFactory.GetRandomEnemyAsync(position, rotation,GetOrCreateChild(_stageRoot, RoomConstants.ObjectsGameObjectName))).gameObject;
            }
            return null;
        }

        private GameObject ResolveRoomEventTriggerEntrySync(RoomEventTriggerTableEntry eventTriggerEntry, Vector3 position, Quaternion rotation)
        {
            if (!Application.isPlaying)
            {
                var nameText = _tableRepository?.GetStringText(eventTriggerEntry.NameId) ?? eventTriggerEntry.NameId.ToString();
                var go = new GameObject($"[EventTrigger] {nameText} (ID:{eventTriggerEntry.Id})");
                go.transform.SetParent(GetOrCreateChild(_stageRoot, RoomConstants.ObjectsGameObjectName));
                var marker = go.AddComponent<RoomObjectMarker>();
                marker.TableId = eventTriggerEntry.Id;
                return go;
            }
            if (!System.Enum.IsDefined(typeof(RoomEventTriggerType), eventTriggerEntry.Id))
                return null;
            var triggerType = (RoomEventTriggerType)eventTriggerEntry.Id;
            switch (triggerType)
            {
                case RoomEventTriggerType.PlayerSpawnPoint:
                    return _playerFactory.GetPlayerSync(position, rotation,GetOrCreateChild(_stageRoot, RoomConstants.ObjectsGameObjectName)).gameObject;
                case RoomEventTriggerType.RandomEnemySpawn:
                    return _enemyFactory.GetRandomEnemySync(position, rotation,GetOrCreateChild(_stageRoot, RoomConstants.ObjectsGameObjectName)).gameObject;
            }
            return null;
        }

        private void PlaceObjectsInternal(Vector3 worldOffset, List<ObjectData> objectDatas, List<GameObject> createdObjects)
        {
            var objectsParent = GetOrCreateChild(_stageRoot, RoomConstants.ObjectsGameObjectName);

            for (int i = 0; i < objectDatas.Count; i++)
            {
                var objectData = objectDatas[i];
                var instance = createdObjects[i];
                if (instance == null)
                {
                    continue;
                }
                instance.transform.position = new Vector3(objectData.Position.x, objectData.Position.y, 0) + worldOffset;
                instance.transform.rotation = objectData.Rotation;
            }
        }
    }
}

