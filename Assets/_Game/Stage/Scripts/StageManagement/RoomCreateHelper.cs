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
        public static async Task PlaceAdditionalTilesAsync(Transform stageRoot, Vector2 centerPos, RoomData roomData, ISceneResourceProvider sceneResourceProvider)
        {
            if (!ValidatePlaceAdditionalTiles(stageRoot, roomData, sceneResourceProvider))
                return;
            var tileBases = await LoadTileBasesAsync(roomData, sceneResourceProvider);
            PlaceAdditionalTilesInternal(stageRoot, centerPos, roomData.Tiles, tileBases);
        }

        /// <summary>
        /// 추가 타일을 동기적으로 배치합니다.
        /// </summary>
        public static void PlaceAdditionalTilesSync(Transform stageRoot, Vector2 centerPos, RoomData roomData, ISceneResourceProvider sceneResourceProvider)
        {
            if (!ValidatePlaceAdditionalTiles(stageRoot, roomData, sceneResourceProvider))
                return;
            var tileBases = LoadTileBasesSync(roomData, sceneResourceProvider);
            PlaceAdditionalTilesInternal(stageRoot, centerPos, roomData.Tiles, tileBases);
        }

        private static bool ValidatePlaceAdditionalTiles(Transform stageRoot, RoomData roomData, ISceneResourceProvider sceneResourceProvider)
        {
            if (stageRoot == null || roomData == null || sceneResourceProvider == null)
            {
                LogHandler.LogError(nameof(RoomCreateHelper), "파라미터가 올바르지 않습니다.");
                return false;
            }
            return true;
        }

        private static async Task<List<TileBase>> LoadTileBasesAsync(RoomData roomData, ISceneResourceProvider sceneResourceProvider)
        {
            var list = new List<TileBase>();
            foreach (var tileData in roomData.Tiles)
            {
                var address = roomData.GetAddress(tileData.Index);
                list.Add(await sceneResourceProvider.GetAssetAsync<TileBase>(address));
            }
            return list;
        }

        private static List<TileBase> LoadTileBasesSync(RoomData roomData, ISceneResourceProvider sceneResourceProvider)
        {
            var list = new List<TileBase>();
            foreach (var tileData in roomData.Tiles)
            {
                var address = roomData.GetAddress(tileData.Index);
                list.Add(sceneResourceProvider.GetAssetSync<TileBase>(address));
            }
            return list;
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
        public static async Task<List<GameObject>> PlaceObjectsAsync(Transform stageRoot, RoomData roomData, IPlayerFactory playerFactory, IEnemyFactory enemyFactory, ISceneResourceProvider sceneResourceProvider, ITableRepository tableRepository, Vector3 worldOffset = default)
        {
            var instances = await ResolveAndCreateInstancesAsync(roomData, playerFactory, enemyFactory, sceneResourceProvider, tableRepository);
            PlaceObjectsInternal(stageRoot, worldOffset, roomData.Objects, instances);
            return instances;
        }

        /// <summary>
        /// 오브젝트를 동기적으로 배치합니다.
        /// ObjectData.TableId가 0이 아니면 테이블 ID 기반(팩토리/테이블)으로, 0이면 legacy 어드레스 기반으로 생성합니다.
        /// </summary>
        public static List<GameObject> PlaceObjectsSync(Transform stageRoot, RoomData roomData, IPlayerFactory playerFactory, IEnemyFactory enemyFactory, ISceneResourceProvider sceneResourceProvider, ITableRepository tableRepository, Vector3 worldOffset = default)
        {
            var instances = ResolveAndCreateInstancesSync(roomData, playerFactory, enemyFactory, sceneResourceProvider, tableRepository);
            PlaceObjectsInternal(stageRoot, worldOffset, roomData.Objects, instances);
            return instances;
        }

        private static async Task<List<GameObject>> ResolveAndCreateInstancesAsync(RoomData roomData, IPlayerFactory playerFactory, IEnemyFactory enemyFactory, ISceneResourceProvider sceneResourceProvider, ITableRepository tableRepository)
        {
            var instances = new List<GameObject>();
            foreach (var objectData in roomData.Objects)
            {
                var instance = objectData.TableId == 0 ? null : await ResolveByTableIdAsync(objectData.TableId, playerFactory, enemyFactory, sceneResourceProvider, tableRepository);
                if (instance != null && objectData.TableId != 0)
                {
                    var marker = instance.AddOrGetComponent<RoomObjectMarker>();
                    marker.TableId = objectData.TableId;
                }
                instances.Add(instance);
            }
            return instances;
        }

        private static List<GameObject> ResolveAndCreateInstancesSync(RoomData roomData, IPlayerFactory playerFactory, IEnemyFactory enemyFactory, ISceneResourceProvider sceneResourceProvider, ITableRepository tableRepository)
        {
            var instances = new List<GameObject>();
            foreach (var objectData in roomData.Objects)
            {
                var instance = objectData.TableId == 0 ? null : ResolveByTableIdSync(objectData.TableId, playerFactory, enemyFactory, sceneResourceProvider, tableRepository);
                if (instance != null && objectData.TableId != 0)
                {
                    var marker = instance.AddOrGetComponent<RoomObjectMarker>();
                    marker.TableId = objectData.TableId;
                }
                instances.Add(instance);
            }
            return instances;
        }

        private static async Task<GameObject> ResolveByTableIdAsync(int tableId, IPlayerFactory playerFactory, IEnemyFactory enemyFactory, ISceneResourceProvider sceneResourceProvider, ITableRepository tableRepository)
        {
            if (tableRepository == null) return null;
            
            var entry = tableRepository.GetTableEntry(tableId);
            if (entry == null) return null;
            
            if (entry is RoomEventTriggerTableEntry eventTriggerEntry)
                return await ResolveRoomEventTriggerEntryAsync(eventTriggerEntry, playerFactory, enemyFactory, sceneResourceProvider);
            if (entry is EnemyConfigTableEntry enemyConfig)
                return await ResolveEnemyEntryAsync(tableId, enemyConfig, enemyFactory, sceneResourceProvider);
            if (entry is PlayerConfigTableEntry playerConfig)
                return await ResolvePlayerEntryAsync(tableId, playerConfig, playerFactory, sceneResourceProvider);
            return null;
        }

        private static GameObject ResolveByTableIdSync(int tableId, IPlayerFactory playerFactory, IEnemyFactory enemyFactory, ISceneResourceProvider sceneResourceProvider, ITableRepository tableRepository)
        {
            if (tableRepository == null) return null;
            
            var entry = tableRepository.GetTableEntry(tableId);
            if (entry == null) return null;
            if (entry is RoomEventTriggerTableEntry eventTriggerEntry)
                return ResolveRoomEventTriggerEntrySync(eventTriggerEntry, playerFactory, enemyFactory, sceneResourceProvider);
            if (entry is EnemyConfigTableEntry enemyConfig)
                return ResolveEnemyEntrySync(tableId, enemyConfig, enemyFactory, sceneResourceProvider);
            if (entry is PlayerConfigTableEntry playerConfig)
                return ResolvePlayerEntrySync(tableId, playerConfig, playerFactory, sceneResourceProvider);
            return null;
        }

        private static async Task<GameObject> ResolveEnemyEntryAsync(int tableId, EnemyConfigTableEntry enemyConfig, IEnemyFactory enemyFactory, ISceneResourceProvider sceneResourceProvider)
        {
            if (Application.isPlaying && enemyFactory != null)
            {
                var enemy = await enemyFactory.GetEnemyByConfigIdAsync(tableId);
                return enemy != null ? enemy.gameObject : null;
            }
            return await sceneResourceProvider.GetInstanceAsync(enemyConfig.GameObjectKey);
        }

        private static GameObject ResolveEnemyEntrySync(int tableId, EnemyConfigTableEntry enemyConfig, IEnemyFactory enemyFactory, ISceneResourceProvider sceneResourceProvider)
        {
            if (Application.isPlaying && enemyFactory != null)
            {
                var enemy = enemyFactory.GetEnemyByConfigIdSync(tableId);
                return enemy != null ? enemy.gameObject : null;
            }
            return sceneResourceProvider.GetInstanceSync(enemyConfig.GameObjectKey);
        }

        private static async Task<GameObject> ResolvePlayerEntryAsync(int tableId, PlayerConfigTableEntry playerConfig, IPlayerFactory playerFactory, ISceneResourceProvider sceneResourceProvider)
        {
            if (Application.isPlaying && playerFactory != null)
            {
                var player = await playerFactory.GetPlayerByConfigIdAsync(tableId);
                return player != null ? player.gameObject : null;
            }
            return await sceneResourceProvider.GetInstanceAsync(playerConfig.GameObjectKey);
        }

        private static GameObject ResolvePlayerEntrySync(int tableId, PlayerConfigTableEntry playerConfig, IPlayerFactory playerFactory, ISceneResourceProvider sceneResourceProvider)
        {
            if (Application.isPlaying && playerFactory != null)
            {
                var player = playerFactory.GetPlayerByConfigIdSync(tableId);
                return player != null ? player.gameObject : null;
            }
            return sceneResourceProvider.GetInstanceSync(playerConfig.GameObjectKey);
        }

        private static async Task<GameObject> ResolveRoomEventTriggerEntryAsync(RoomEventTriggerTableEntry eventTriggerEntry, IPlayerFactory playerFactory, IEnemyFactory enemyFactory, ISceneResourceProvider sceneResourceProvider)
        {
            if (!Application.isPlaying)
            {
                var go = new GameObject($"[EventTrigger] {eventTriggerEntry.Name} (ID:{eventTriggerEntry.Id})");
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
                    return (await playerFactory.GetPlayerAsync()).gameObject;
                case RoomEventTriggerType.RandomEnemySpawn:
                    return (await enemyFactory.GetRandomEnemyAsync()).gameObject;
            }
            return null;
        }

        private static GameObject ResolveRoomEventTriggerEntrySync(RoomEventTriggerTableEntry eventTriggerEntry, IPlayerFactory playerFactory, IEnemyFactory enemyFactory, ISceneResourceProvider sceneResourceProvider)
        {
            if (!Application.isPlaying)
            {
                var go = new GameObject($"[EventTrigger] {eventTriggerEntry.Name} (ID:{eventTriggerEntry.Id})");
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
                    return playerFactory.GetPlayerSync().gameObject;
                case RoomEventTriggerType.RandomEnemySpawn:
                    return enemyFactory.GetRandomEnemySync().gameObject;
            }
            return null;
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

