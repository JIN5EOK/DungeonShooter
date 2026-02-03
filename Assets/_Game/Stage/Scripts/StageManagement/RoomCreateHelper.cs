using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Jin5eok;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace DungeonShooter
{
    /// <summary>
    /// Room 오브젝트 계층구조 및 타일,오브젝트 생성 유틸리티
    /// </summary>
    public static class RoomCreateHelper
    {
        /// <summary>
        /// Room의 전체 계층 구조를 생성하거나 가져옵니다.
        /// </summary>
        /// <param name="stageRoot">Room Transform (null이면 새로 생성)</param>
        /// <param name="roomName">Room 이름</param>
        /// <returns>(tilemapsParent, objectsParent, baseTilemapsParent)</returns>
        public static void GetOrCreateRoomStructure(Transform stageRoot = null, string roomName = "Stage")
        {
            // Room GameObject 생성 또는 가져오기
            if (stageRoot == null)
            {
                stageRoot = new GameObject(roomName).transform;
            }

            // Tilemaps 구조 생성 또는 가져오기
            var tilemapsParent = GetOrCreateChild(stageRoot, RoomConstants.TILEMAPS_GAMEOBJECT_NAME);
            tilemapsParent.gameObject.AddOrGetComponent<Grid>();
            // Tilemap 컴포넌트 생성
            GetOrCreateTilemap(stageRoot, RoomConstants.TILEMAP_GROUND_NAME);
            GetOrCreateTilemap(stageRoot, RoomConstants.TILEMAP_DECO_NAME);
            // Objects 구조 생성 또는 가져오기
            GetOrCreateChild(stageRoot, RoomConstants.OBJECTS_GAMEOBJECT_NAME);
        }

        /// <summary>
        /// 지정된 이름의 자식 GameObject를 찾거나 생성합니다.
        /// </summary>
        public static Transform GetOrCreateChild(Transform parent, string childName)
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

        /// <summary>
        /// 타일맵 이름에 따라 적절한 렌더링 레이어를 반환합니다.
        /// </summary>
        private static RenderingLayer GetRenderingLayerForTilemap(string tilemapName)
        {
            return tilemapName.Contains(RenderingLayers.Ground.LayerName) ? RenderingLayers.Ground
                : tilemapName.Contains(RenderingLayers.Wall.LayerName) ? RenderingLayers.Wall
                : tilemapName.Contains(RenderingLayers.Deco.LayerName) ? RenderingLayers.Deco
                : RenderingLayers.Ground;
        }

        /// <summary>
        /// 지정된 이름의 Tilemap을 찾거나 생성합니다.
        /// </summary>
        public static Tilemap GetOrCreateTilemap(Transform stageRoot, string tilemapName)
        {
            var tilemapRoot = GetOrCreateChild(stageRoot, RoomConstants.TILEMAPS_GAMEOBJECT_NAME);
            var tilemapObj = GetOrCreateChild(tilemapRoot, tilemapName);
            var tilemap = tilemapObj.gameObject.AddOrGetComponent<Tilemap>();
            var renderer = tilemapObj.gameObject.AddOrGetComponent<TilemapRenderer>();
            var collider = tilemapObj.gameObject.AddOrGetComponent<TilemapCollider2D>();
            var rigidBody2D = tilemapObj.gameObject.AddOrGetComponent<Rigidbody2D>();
            rigidBody2D.bodyType = RigidbodyType2D.Kinematic;
            tilemapObj.gameObject.AddOrGetComponent<CompositeCollider2D>();
            
            // 타일맵 이름에 따라 적절한 렌더링 레이어 설정
            var renderingLayer = GetRenderingLayerForTilemap(tilemapName);
            renderer.sortingLayerName = renderingLayer.LayerName;

            collider.compositeOperation = Collider2D.CompositeOperation.Merge;
            
            return tilemap;
        }
        
        /// <summary>
        /// Room의 타일맵과 오브젝트를 모두 제거합니다.
        /// </summary>
        /// <param name="stageRoot">Room Transform</param>
        public static void ClearRoomObject(Transform stageRoot)
        {
            // Objects 하위의 모든 오브젝트 제거
            var objectsParent = stageRoot.transform.Find(RoomConstants.OBJECTS_GAMEOBJECT_NAME);
            if (objectsParent != null)
            {
                for (int i = objectsParent.childCount - 1; i >= 0; i--)
                {
                    var child = objectsParent.GetChild(i);
                    if (Application.isEditor && !Application.isPlaying)
                    {
                        UnityEngine.Object.DestroyImmediate(child.gameObject);
                    }
                    else
                    {
                        UnityEngine.Object.Destroy(child.gameObject);
                    }
                }
            }
        }
        
        public static void ClearTiles(Transform stageRoot)
        {
            var groundTilemap = GetOrCreateTilemap(stageRoot, RoomConstants.TILEMAP_GROUND_NAME);
            var decoTilemap = GetOrCreateTilemap(stageRoot, RoomConstants.TILEMAP_DECO_NAME);
            groundTilemap.ClearAllTiles();
            decoTilemap.ClearAllTiles();
        }

        /// <summary>
        /// 베이스 타일 배치 로직
        /// </summary>
        public static void PlaceBaseTiles(Transform stageRoot, Vector2 centerPos, RoomData roomData, TileBase groundTile)
        {
            var roomSizeX = roomData.RoomSizeX;
            var roomSizeY = roomData.RoomSizeY;

            var tilemapsParent = GetOrCreateChild(stageRoot, RoomConstants.TILEMAPS_GAMEOBJECT_NAME);
            var groundTilemap = GetOrCreateTilemap(stageRoot, RoomConstants.TILEMAP_GROUND_NAME);

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
        /// <param name="stageRoot">스테이지 루트 Transform</param>
        /// <param name="centerPos">방의 중심 위치 (월드 좌표)</param>
        /// <param name="roomData">방 데이터</param>
        /// <param name="sceneResourceProvider">씬 리소스 제공자</param>
        public static async Task PlaceAdditionalTilesAsync(Transform stageRoot, Vector2 centerPos, RoomData roomData, ISceneResourceProvider sceneResourceProvider)
        {
            if (stageRoot == null || roomData == null || sceneResourceProvider == null)
            {
                LogHandler.LogError(nameof(RoomCreateHelper), "파라미터가 올바르지 않습니다.");
                return;
            }

            // 타일 배치
            var tileBases = new List<TileBase>();
            foreach (var tileData in roomData.Tiles)
            {
                var address = roomData.GetAddress(tileData.Index);
                tileBases.Add(await sceneResourceProvider.GetAssetAsync<TileBase>(address));
            }
            
            PlaceAdditionalTilesInternal(stageRoot, centerPos, roomData.Tiles, tileBases);
        }

        /// <summary>
        /// 추가 타일을 동기적으로 배치합니다.
        /// </summary>
        /// <param name="stageRoot">스테이지 루트 Transform</param>
        /// <param name="centerPos">방의 중심 위치 (월드 좌표)</param>
        /// <param name="roomData">방 데이터</param>
        /// <param name="sceneResourceProvider">씬 리소스 제공자</param>
        public static void PlaceAdditionalTilesSync(Transform stageRoot, Vector2 centerPos, RoomData roomData, ISceneResourceProvider sceneResourceProvider)
        {
            if (stageRoot == null || roomData == null || sceneResourceProvider == null)
            {
                LogHandler.LogError(nameof(RoomCreateHelper), "파라미터가 올바르지 않습니다.");
                return;
            }

            // 타일 배치
            var tileBases = new List<TileBase>();
            foreach (var tileData in roomData.Tiles)
            {
                var address = roomData.GetAddress(tileData.Index);
                tileBases.Add(sceneResourceProvider.GetAssetSync<TileBase>(address));
            }
            
            PlaceAdditionalTilesInternal(stageRoot, centerPos, roomData.Tiles, tileBases);
        }
        
        /// <summary>
        /// 추가 타일 배치 로직
        /// </summary>
        private static void PlaceAdditionalTilesInternal(Transform stageRoot, Vector3 centerPos, List<TileLayerData> tileDatas, List<TileBase> tileBases)
        {
            var centerPosInt = new Vector3Int((int)centerPos.x, (int)centerPos.y, 0);

            for (int i = 0; i < tileDatas.Count; i++)
            {
                var sortingLayerName = RenderingLayers.GetLayerName(tileDatas[i].Layer);
                var tilemapName = $"{RoomConstants.TILEMAP_COMPONENT_NAME_BASE}{sortingLayerName}";
                var tilemap = GetOrCreateTilemap(stageRoot, tilemapName);
                var localPos = new Vector3Int(tileDatas[i].Position.x, tileDatas[i].Position.y, 0);
                var worldTilePos = localPos + centerPosInt;
                tilemap.SetTile(worldTilePos, tileBases[i]);
            }
        }

        /// <summary>
        /// 오브젝트를 비동기적으로 배치합니다.
        /// ObjectData.TableId가 0이 아니면 테이블 ID 기반(팩토리/테이블)으로, 0이면 legacy 어드레스 기반으로 생성합니다.
        /// </summary>
        /// <param name="stageRoot">스테이지 루트 Transform</param>
        /// <param name="roomData">방 데이터</param>
        /// <param name="playerFactory">플레이어 팩토리 (에디터에서는 null)</param>
        /// <param name="enemyFactory">적 팩토리 (에디터에서는 null)</param>
        /// <param name="sceneResourceProvider">씬 리소스 제공자</param>
        /// <param name="tableRepository">테이블 리포지토리 (테이블 ID 기반 배치 시 필수, 에디터에서 LocalTableRepository 전달)</param>
        /// <param name="worldOffset">월드 오프셋 (기본값: Vector3.zero)</param>
        /// <returns>생성된 오브젝트 리스트를 반환하는 Task</returns>
        public static async Task<List<GameObject>> PlaceObjectsAsync(Transform stageRoot, RoomData roomData, IPlayerFactory playerFactory, IEnemyFactory enemyFactory, ISceneResourceProvider sceneResourceProvider, ITableRepository tableRepository, Vector3 worldOffset = default)
        {
            var instances = new List<GameObject>();
            foreach (var objectData in roomData.Objects)
            {
                var instance = await ResolveAndCreateInstanceAsync(objectData, roomData, playerFactory, enemyFactory, sceneResourceProvider, tableRepository);
                if (instance != null && objectData.TableId != 0)
                {
                    var marker = instance.AddOrGetComponent<RoomObjectMarker>();
                    marker.TableId = objectData.TableId;
                }
                instances.Add(instance);
            }
            PlaceObjectsInternal(stageRoot, worldOffset, roomData.Objects, instances);
            return instances;
        }

        /// <summary>
        /// 오브젝트를 동기적으로 배치합니다.
        /// ObjectData.TableId가 0이 아니면 테이블 ID 기반(팩토리/테이블)으로, 0이면 legacy 어드레스 기반으로 생성합니다.
        /// </summary>
        /// <param name="stageRoot">스테이지 루트 Transform</param>
        /// <param name="roomData">방 데이터</param>
        /// <param name="playerFactory">플레이어 팩토리 (에디터에서는 null)</param>
        /// <param name="enemyFactory">적 팩토리 (에디터에서는 null)</param>
        /// <param name="sceneResourceProvider">씬 리소스 제공자</param>
        /// <param name="tableRepository">테이블 리포지토리 (테이블 ID 기반 배치 시 필수, 에디터에서 LocalTableRepository 전달)</param>
        /// <param name="worldOffset">월드 오프셋 (기본값: Vector3.zero)</param>
        /// <returns>생성된 오브젝트 리스트</returns>
        public static List<GameObject> PlaceObjectsSync(Transform stageRoot, RoomData roomData, IPlayerFactory playerFactory, IEnemyFactory enemyFactory, ISceneResourceProvider sceneResourceProvider, ITableRepository tableRepository, Vector3 worldOffset = default)
        {
            var instances = new List<GameObject>();
            foreach (var objectData in roomData.Objects)
            {
                var instance = ResolveAndCreateInstanceSync(objectData, roomData, playerFactory, enemyFactory, sceneResourceProvider, tableRepository);
                if (instance != null && objectData.TableId != 0)
                {
                    var marker = instance.AddOrGetComponent<RoomObjectMarker>();
                    marker.TableId = objectData.TableId;
                }
                instances.Add(instance);
            }
            PlaceObjectsInternal(stageRoot, worldOffset, roomData.Objects, instances);
            return instances;
        }

        private static async Task<GameObject> ResolveAndCreateInstanceAsync(ObjectData objectData, RoomData roomData, IPlayerFactory playerFactory, IEnemyFactory enemyFactory, ISceneResourceProvider sceneResourceProvider, ITableRepository tableRepository)
        {
            if (objectData.TableId == 0)
            {
                return null;
            }
            return await ResolveByTableIdAsync(objectData.TableId, playerFactory, enemyFactory, sceneResourceProvider, tableRepository);
        }

        private static GameObject ResolveAndCreateInstanceSync(ObjectData objectData, RoomData roomData, IPlayerFactory playerFactory, IEnemyFactory enemyFactory, ISceneResourceProvider sceneResourceProvider, ITableRepository tableRepository)
        {
            if (objectData.TableId == 0)
            {
                return null;
            }
            return ResolveByTableIdSync(objectData.TableId, playerFactory, enemyFactory, sceneResourceProvider, tableRepository);
        }

        private static async Task<GameObject> ResolveByTableIdAsync(int tableId, IPlayerFactory playerFactory, IEnemyFactory enemyFactory, ISceneResourceProvider sceneResourceProvider, ITableRepository tableRepository)
        {
            if (tableRepository == null)
            {
                return null;
            }

            var miscObjEntry = tableRepository.GetTableEntry<MiscObjectTableEntry>(tableId);
            if (miscObjEntry != null)
            {
                return await ResolveMiscObjectEntryAsync(miscObjEntry, playerFactory, enemyFactory, sceneResourceProvider);
            }

            var entry = tableRepository.GetTableEntry(tableId);
            if (entry == null)
            {
                return null;
            }
            if (entry is EnemyConfigTableEntry enemyConfig)
            {
                if (Application.isPlaying && enemyFactory != null)
                {
                    var enemy = await enemyFactory.GetEnemyByConfigIdAsync(tableId);
                    return enemy != null ? enemy.gameObject : null;
                }
                if (!string.IsNullOrEmpty(enemyConfig.GameObjectKey))
                {
                    return await sceneResourceProvider.GetInstanceAsync(enemyConfig.GameObjectKey);
                }
            }
            else if (entry is PlayerConfigTableEntry playerConfig)
            {
                if (Application.isPlaying && playerFactory != null)
                {
                    var player = await playerFactory.GetPlayerByConfigIdAsync(tableId);
                    return player != null ? player.gameObject : null;
                }
                if (!string.IsNullOrEmpty(playerConfig.GameObjectKey))
                {
                    return await sceneResourceProvider.GetInstanceAsync(playerConfig.GameObjectKey);
                }
            }
            return null;
        }

        private static async Task<GameObject> ResolveMiscObjectEntryAsync(MiscObjectTableEntry miscObjEntry, IPlayerFactory playerFactory, IEnemyFactory enemyFactory, ISceneResourceProvider sceneResourceProvider)
        {
            // 에디터/비플레이 모드에서는 마커 프리팹을 배치해 시각화합니다.
            if (!Application.isPlaying)
            {
                if (!string.IsNullOrEmpty(miscObjEntry.GameObjectKey))
                {
                    return await sceneResourceProvider.GetInstanceAsync(miscObjEntry.GameObjectKey);
                }
                return new GameObject($"[MiscObject] {miscObjEntry.Name} (ID:{miscObjEntry.Id})");
            }

            switch (miscObjEntry.ObjectType)
            {
                case MiscObjectType.PlayerSpawnPoint:
                    if (Application.isPlaying && playerFactory != null)
                    {
                        var player = await playerFactory.GetPlayerAsync();
                        return player != null ? player.gameObject : null;
                    }
                    return null;
                case MiscObjectType.RandomEnemySpawn:
                    if (Application.isPlaying && enemyFactory != null)
                    {
                        var enemy = await enemyFactory.GetRandomEnemyAsync();
                        return enemy != null ? enemy.gameObject : null;
                    }
                    return null;
                default:
                    return null;
            }
        }

        private static GameObject ResolveByTableIdSync(int tableId, IPlayerFactory playerFactory, IEnemyFactory enemyFactory, ISceneResourceProvider sceneResourceProvider, ITableRepository tableRepository)
        {
            if (tableRepository == null)
            {
                return null;
            }

            var miscObjEntry = tableRepository.GetTableEntry<MiscObjectTableEntry>(tableId);
            if (miscObjEntry != null)
            {
                return ResolveMiscObjectEntrySync(miscObjEntry, playerFactory, enemyFactory, sceneResourceProvider);
            }

            var entry = tableRepository.GetTableEntry(tableId);
            if (entry == null)
            {
                return null;
            }
            if (entry is EnemyConfigTableEntry enemyConfig)
            {
                if (Application.isPlaying && enemyFactory != null)
                {
                    var enemy = enemyFactory.GetEnemyByConfigIdSync(tableId);
                    return enemy != null ? enemy.gameObject : null;
                }
                if (!string.IsNullOrEmpty(enemyConfig.GameObjectKey))
                {
                    return sceneResourceProvider.GetInstanceSync(enemyConfig.GameObjectKey);
                }
            }
            else if (entry is PlayerConfigTableEntry playerConfig)
            {
                if (Application.isPlaying && playerFactory != null)
                {
                    var player = playerFactory.GetPlayerByConfigIdSync(tableId);
                    return player != null ? player.gameObject : null;
                }
                if (!string.IsNullOrEmpty(playerConfig.GameObjectKey))
                {
                    return sceneResourceProvider.GetInstanceSync(playerConfig.GameObjectKey);
                }
            }
            return null;
        }

        private static GameObject ResolveMiscObjectEntrySync(MiscObjectTableEntry miscObjEntry, IPlayerFactory playerFactory, IEnemyFactory enemyFactory, ISceneResourceProvider sceneResourceProvider)
        {
            // 에디터/비플레이 모드에서는 마커 프리팹을 배치해 시각화합니다.
            if (!Application.isPlaying)
            {
                if (!string.IsNullOrEmpty(miscObjEntry.GameObjectKey))
                {
                    return sceneResourceProvider.GetInstanceSync(miscObjEntry.GameObjectKey);
                }
                return new GameObject($"[MiscObject] {miscObjEntry.Name} (ID:{miscObjEntry.Id})");
            }

            switch (miscObjEntry.ObjectType)
            {
                case MiscObjectType.PlayerSpawnPoint:
                    if (Application.isPlaying && playerFactory != null)
                    {
                        var player = playerFactory.GetPlayerSync();
                        return player != null ? player.gameObject : null;
                    }
                    return null;
                case MiscObjectType.RandomEnemySpawn:
                    if (Application.isPlaying && enemyFactory != null)
                    {
                        var enemy = enemyFactory.GetRandomEnemySync();
                        return enemy != null ? enemy.gameObject : null;
                    }
                    return null;
                default:
                    return null;
            }
        }

        private static void PlaceObjectsInternal(Transform stageRoot, Vector3 worldOffset, List<ObjectData> objectDatas, List<GameObject> createdObjects)
        {
            var objectsParent = GetOrCreateChild(stageRoot, RoomConstants.OBJECTS_GAMEOBJECT_NAME);

            for (int i = 0; i < objectDatas.Count; i++)
            {
                var objectData = objectDatas[i];
                var instance = createdObjects[i];
                if (instance == null)
                {
                    continue;
                }
                instance.transform.SetParent(objectsParent);
                instance.transform.position = new Vector3(objectData.Position.x, objectData.Position.y, 0) + worldOffset;
                instance.transform.rotation = objectData.Rotation;
            }
        }
    }
}

