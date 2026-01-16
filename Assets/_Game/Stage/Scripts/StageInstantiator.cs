using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Tilemaps;
using Jin5eok;

namespace DungeonShooter
{
    /// <summary>
    /// RoomData와 Stage를 실제 게임오브젝트로 변환하는 유틸리티 클래스
    /// </summary>
    public static class StageInstantiator
    {
        /// <summary>
        /// Stage를 실제 게임오브젝트로 변환합니다.
        /// </summary>
        /// <param name="resourceProvider">리소스 제공자</param>
        /// <param name="stage">변환할 Stage</param>
        /// <param name="parent">부모 Transform (null이면 씬 루트)</param>
        /// <returns>생성된 게임오브젝트를 반환하는 Task</returns>
        public static async Task<GameObject> InstantiateStage(
            IStageResourceProvider resourceProvider,
            Stage stage,
            Transform parent = null)
        {
            if (stage == null)
            {
                Debug.LogError($"[{nameof(StageInstantiator)}] Stage가 null입니다.");
                return null;
            }

            if (resourceProvider == null)
            {
                Debug.LogError($"[{nameof(StageInstantiator)}] ResourceProvider가 null입니다.");
                return null;
            }

            var stageObj = new GameObject("Stage");
            if (parent != null)
            {
                stageObj.transform.SetParent(parent);
            }

            // Stage 레벨 타일맵 구조 생성
            var stageTilemapsParent = new GameObject(RoomConstants.TILEMAPS_GAMEOBJECT_NAME);
            stageTilemapsParent.transform.SetParent(stageObj.transform);
            stageTilemapsParent.AddComponent<Grid>();

            // Stage 레벨 오브젝트 구조 생성
            var stageObjectsParent = new GameObject(RoomConstants.OBJECTS_GAMEOBJECT_NAME);
            stageObjectsParent.transform.SetParent(stageObj.transform);

            // 타일맵 생성 (Ground, Deco)
            var groundTilemapObj = new GameObject(RoomConstants.TILEMAP_GROUND_NAME);
            groundTilemapObj.transform.SetParent(stageTilemapsParent.transform);
            var groundTilemap = groundTilemapObj.AddComponent<Tilemap>();
            groundTilemap.gameObject.AddComponent<TilemapCollider2D>().compositeOperation = Collider2D.CompositeOperation.Merge;
            var groundRenderer = groundTilemapObj.AddComponent<TilemapRenderer>();
            groundRenderer.sortingLayerName = RenderingLayers.Ground.LayerName;
            

            var decoTilemapObj = new GameObject(RoomConstants.TILEMAP_DECO_NAME);
            decoTilemapObj.transform.SetParent(stageTilemapsParent.transform);
            var decoTilemap = decoTilemapObj.AddComponent<Tilemap>();
            var decoRenderer = decoTilemapObj.AddComponent<TilemapRenderer>();
            decoTilemap.gameObject.AddComponent<TilemapCollider2D>().compositeOperation = Collider2D.CompositeOperation.Merge;
            decoRenderer.sortingLayerName = RenderingLayers.Deco.LayerName;

            // 모든 방의 타일과 오브젝트를 Stage 레벨에 배치
            foreach (var room in stage.Rooms.Values)
            {
                if (room.RoomData == null)
                {
                    Debug.LogWarning($"[{nameof(StageInstantiator)}] Room {room.Id}의 RoomData가 null입니다. 스킵합니다.");
                    continue;
                }

                var worldPosition = new Vector3(room.Position.x * RoomConstants.ROOM_SPACING, room.Position.y * RoomConstants.ROOM_SPACING, 0);

                // 방의 타일을 Stage 레벨 타일맵에 배치
                await PlaceRoomTilesAsync(resourceProvider, room, worldPosition, groundTilemap, decoTilemap);

                // 방의 오브젝트를 Stage 레벨 Objects에 배치
                await InstantiateObjectsAsync(room.RoomData, stageObjectsParent.transform, resourceProvider, worldPosition);
            }

            // 복도 생성
            await CreateCorridorsAsync(resourceProvider, stage, groundTilemap);

            Debug.Log($"[{nameof(StageInstantiator)}] 스테이지 생성 완료. 방 개수: {stage.Rooms.Count}");
            return stageObj;
        }

        /// <summary>
        /// 방의 타일을 Stage 레벨 타일맵에 배치합니다.
        /// </summary>
        private static async Awaitable PlaceRoomTilesAsync(
            IStageResourceProvider resourceProvider,
            Room room,
            Vector3 worldPosition,
            Tilemap groundTilemap,
            Tilemap decoTilemap)
        {
            var roomData = room.RoomData;
            var roomSizeX = roomData.RoomSizeX;
            var roomSizeY = roomData.RoomSizeY;
            var worldPosInt = new Vector3Int((int)worldPosition.x, (int)worldPosition.y, 0);

            // 타일 로드
            var groundTile = await resourceProvider.GetGroundTile();

            if (groundTile == null)
            {
                Debug.LogError($"[{nameof(StageInstantiator)}] 필수 타일을 로드할 수 없습니다.");
                return;
            }

            // Ground 타일 배치 (방 내부)
            var startX = -roomSizeX / 2;
            var startY = -roomSizeY / 2;
            for (int x = 0; x < roomSizeX; x++)
            {
                for (int y = 0; y < roomSizeY; y++)
                {
                    var localPos = new Vector3Int(startX + x, startY + y, 0);
                    var worldTilePos = localPos + worldPosInt;
                    groundTilemap.SetTile(worldTilePos, groundTile);
                }
            }

            // Deco 타일 배치
            if (roomData.Tiles.Count > 0)
            {
                // 레이어별로 그룹화
                var tilesByLayer = roomData.Tiles.GroupBy(t => t.Layer);

                foreach (var layerGroup in tilesByLayer)
                {
                    // 타일 배치 (비동기 로드)
                    var uniqueTileIndices = layerGroup.Select(t => t.Index).Distinct().ToList();
                    var tileCache = new Dictionary<int, TileBase>();

                    // 모든 타일을 먼저 로드
                    foreach (var index in uniqueTileIndices)
                    {
                        var address = roomData.GetAddress(index);
                        if (string.IsNullOrEmpty(address)) continue;

                        var tileBase = await resourceProvider.GetAsset<TileBase>(address);
                        if (tileBase != null)
                        {
                            tileCache[index] = tileBase;
                        }
                    }

                    // 타일 배치 (로컬 좌표를 월드 좌표로 변환)
                    foreach (var tileData in layerGroup)
                    {
                        if (!tileCache.TryGetValue(tileData.Index, out var tileBase)) continue;

                        var localPos = new Vector3Int(tileData.Position.x, tileData.Position.y, 0);
                        var worldTilePos = localPos + worldPosInt;
                        decoTilemap.SetTile(worldTilePos, tileBase);
                    }
                }
            }
        }

        /// <summary>
        /// Room 구조를 가져오거나 생성합니다.
        /// </summary>
        private static (Transform tilemapsParent, Transform objectsParent) GetOrCreateRoomStructure(
            GameObject roomObj,
            Transform parent,
            string roomName)
        {
            if (roomObj != null)
            {
                // 기존 타일맵과 오브젝트 제거
                ClearExistingData(roomObj);
            }

            var (tilemapsParent, objectsParent) = RoomTilemapHelper.GetOrCreateRoomStructure(roomObj, parent, roomName);

            if (tilemapsParent == null || objectsParent == null)
            {
                Debug.LogError($"[{nameof(StageInstantiator)}] Room 구조를 생성할 수 없습니다.");
                return (null, null);
            }

            return (tilemapsParent, objectsParent);
        }

        /// <summary>
        /// RoomData의 타일 데이터를 Tilemap_Deco에 배치합니다.
        /// </summary>
        private static async Task InstantiateDecoTilemapsAsync(
            RoomData roomData,
            Transform tilemapsParent,
            IStageResourceProvider resourceProvider)
        {
            if (roomData.Tiles.Count == 0)
            {
                return;
            }

            var grid = tilemapsParent.GetComponent<Grid>();
            if (grid == null)
            {
                Debug.LogError($"[{nameof(StageInstantiator)}] Grid 컴포넌트가 없습니다.");
                return;
            }

            // 레이어별로 그룹화
            var tilesByLayer = roomData.Tiles.GroupBy(t => t.Layer);

            foreach (var layerGroup in tilesByLayer)
            {
                var sortingLayerId = layerGroup.Key;
                var sortingLayerName = GetSortingLayerName(sortingLayerId);

                // 레이어별 Tilemap 생성 (Deco)
                var tilemapName = $"Tilemap_{sortingLayerName}";
                var tilemapObj = new GameObject(tilemapName);
                tilemapObj.transform.SetParent(tilemapsParent);
                var tilemap = tilemapObj.AddComponent<Tilemap>();
                var renderer = tilemapObj.AddComponent<TilemapRenderer>();
                renderer.sortingLayerID = sortingLayerId;

                // 타일 배치 (비동기 로드)
                var uniqueTileIndices = layerGroup.Select(t => t.Index).Distinct().ToList();
                var tileCache = new Dictionary<int, TileBase>();

                // 모든 타일을 먼저 로드
                foreach (var index in uniqueTileIndices)
                {
                    var address = roomData.GetAddress(index);
                    if (string.IsNullOrEmpty(address)) continue;

                    var tileBase = await resourceProvider.GetAsset<TileBase>(address);
                    if (tileBase != null)
                    {
                        tileCache[index] = tileBase;
                    }
                }

                // 타일 배치
                foreach (var tileData in layerGroup)
                {
                    if (!tileCache.TryGetValue(tileData.Index, out var tileBase)) continue;

                    var cellPosition = new Vector3Int(tileData.Position.x, tileData.Position.y, 0);
                    tilemap.SetTile(cellPosition, tileBase);
                }
            }
        }

        /// <summary>
        /// RoomData의 오브젝트 데이터를 배치합니다 (비동기).
        /// </summary>
        private static async Task InstantiateObjectsAsync(
            RoomData roomData,
            Transform objectsParent,
            IStageResourceProvider resourceProvider,
            Vector3 worldOffset = default)
        {
            foreach (var objectData in roomData.Objects)
            {
                var address = roomData.GetAddress(objectData.Index);
                if (string.IsNullOrEmpty(address)) continue;

                var instance = await resourceProvider.GetInstance(address);
                if (instance != null)
                {
                    instance.transform.SetParent(objectsParent);
                    instance.transform.position = new Vector3(objectData.Position.x, objectData.Position.y, 0) + worldOffset;
                    instance.transform.rotation = objectData.Rotation;
                }

                // 생성후 초기화 필요한 객체면 대기
                if (instance != null && instance.TryGetComponent(out IInitializationAwaiter initAwaiter))
                {
                    Debug.Log($"{nameof(StageInstantiator)} : 초기화 필요한 객체, 대기합니다.");
                    await initAwaiter.InitializationTask;
                }
            }
        }
        /// <summary>
        /// RoomData를 게임오브젝트로 변환합니다.
        /// roomObj가 null이면 새로 생성하고, null이 아니면 기존 GameObject에 데이터를 불러옵니다.
        /// </summary>
        /// <param name="roomData">변환할 RoomData</param>
        /// <param name="roomObj">대상 GameObject (null이면 새로 생성)</param>
        /// <param name="parent">부모 Transform (roomObj가 null일 때만 사용, null이면 씬 루트)</param>
        /// <param name="roomName">생성될 게임오브젝트 이름 (roomObj가 null일 때만 사용)</param>
        /// <param name="resourceProvider">리소스 제공자</param>
        /// <returns>생성되거나 수정된 게임오브젝트</returns>
        public static GameObject InstantiateRoomEditor(
            RoomData roomData,
            GameObject roomObj = null,
            Transform parent = null,
            string roomName = "Room",
            IStageResourceProvider resourceProvider = null)
        {
            if (roomData == null)
            {
                Debug.LogError($"[{nameof(StageInstantiator)}] RoomData가 null입니다.");
                return null;
            }

            if (resourceProvider == null)
            {
                Debug.LogError($"[{nameof(StageInstantiator)}] ResourceProvider가 null입니다.");
                return null;
            }

            var (tilemapsParent, objectsParent) = GetOrCreateRoomStructure(roomObj, parent, roomName);
            if (tilemapsParent == null || objectsParent == null)
            {
                return null;
            }

            // 타일맵 생성 및 배치 (비동기 메서드를 동기적으로 실행)
            var tilemapsTask = InstantiateDecoTilemapsAsync(roomData, tilemapsParent, resourceProvider);
            tilemapsTask.Wait();

            // 오브젝트 생성 및 배치 (비동기 메서드를 동기적으로 실행)
            var objectsTask = InstantiateObjectsAsync(roomData, objectsParent, resourceProvider);
            objectsTask.Wait();

            return roomObj ?? tilemapsParent.parent.gameObject;
        }

        /// <summary>
        /// 기존 타일과 오브젝트를 제거합니다.
        /// </summary>
        private static void ClearExistingData(GameObject roomObj)
        {
            // Tilemaps 하위의 모든 타일맵 제거
            var tilemapsParent = roomObj.transform.Find(RoomConstants.TILEMAPS_GAMEOBJECT_NAME);
            if (tilemapsParent != null)
            {
                for (int i = tilemapsParent.childCount - 1; i >= 0; i--)
                {
                    if (Application.isEditor)
                    {
                        Object.DestroyImmediate(tilemapsParent.GetChild(i).gameObject);
                    }
                    else
                    {
                        Object.Destroy(tilemapsParent.GetChild(i).gameObject);
                    }
                }
            }

            // Objects 하위의 모든 오브젝트 제거
            var objectsParent = roomObj.transform.Find(RoomConstants.OBJECTS_GAMEOBJECT_NAME);
            if (objectsParent != null)
            {
                for (int i = objectsParent.childCount - 1; i >= 0; i--)
                {
                    if (Application.isEditor)
                    {
                        Object.DestroyImmediate(objectsParent.GetChild(i).gameObject);
                    }
                    else
                    {
                        Object.Destroy(objectsParent.GetChild(i).gameObject);
                    }
                }
            }
        }



        /// <summary>
        /// 방들을 연결하는 복도를 생성합니다.
        /// </summary>
        private static async Awaitable CreateCorridorsAsync(
            IStageResourceProvider resourceProvider,
            Stage stage,
            Tilemap groundTilemap)
        {
            if (resourceProvider == null || stage == null || groundTilemap == null)
            {
                Debug.LogError($"[{nameof(StageInstantiator)}] 파라미터가 올바르지 않습니다.");
                return;
            }

            var groundTile = await resourceProvider.GetGroundTile();
            if (groundTile == null)
            {
                Debug.LogError($"[{nameof(StageInstantiator)}] Ground 타일을 로드할 수 없습니다.");
                return;
            }

            // 각 방의 연결을 따라 복도 생성
            var corridorHalfWidth = RoomConstants.ROOM_CORRIDOR_SIZE / 2;
            var processedConnections = new HashSet<(int, int)>();

            foreach (var room in stage.Rooms.Values)
            {
                if (room.RoomData == null)
                {
                    continue;
                }

                var worldPosition = new Vector3(room.Position.x * RoomConstants.ROOM_SPACING, room.Position.y * RoomConstants.ROOM_SPACING, 0);
                var roomData = room.RoomData;
                var roomSizeX = roomData.RoomSizeX;
                var roomSizeY = roomData.RoomSizeY;
                // 방의 중심점 계산 (방의 위치는 중앙 기준이므로)
                var roomCenterX = (int)worldPosition.x;
                var roomCenterY = (int)worldPosition.y;

                foreach (var connection in room.Connections)
                {
                    var direction = connection.Key;
                    var targetRoomId = connection.Value;

                    // 중복 처리 방지
                    var connectionKey = room.Id < targetRoomId ? (room.Id, targetRoomId) : (targetRoomId, room.Id);
                    if (processedConnections.Contains(connectionKey))
                    {
                        continue;
                    }
                    processedConnections.Add(connectionKey);

                    var targetRoom = stage.GetRoom(targetRoomId);
                    if (targetRoom == null || targetRoom.RoomData == null)
                    {
                        continue;
                    }

                    var targetWorldPosition = new Vector3(targetRoom.Position.x * RoomConstants.ROOM_SPACING, targetRoom.Position.y * RoomConstants.ROOM_SPACING, 0);
                    var targetRoomData = targetRoom.RoomData;
                    var targetRoomSizeX = targetRoomData.RoomSizeX;
                    var targetRoomSizeY = targetRoomData.RoomSizeY;
                    var targetRoomCenterX = (int)targetWorldPosition.x;
                    var targetRoomCenterY = (int)targetWorldPosition.y;

                    // 복도 시작점과 끝점 계산 (방 안쪽으로 충분히 들어가도록)
                    Vector3Int corridorStart, corridorEnd;
                    var corridorExtension = 2; // 방 안쪽으로 들어가는 거리
                    switch (direction)
                    {
                        case Direction.Up:
                            corridorStart = new Vector3Int(roomCenterX - corridorHalfWidth, roomCenterY + roomSizeY / 2 - corridorExtension, 0);
                            corridorEnd = new Vector3Int(targetRoomCenterX - corridorHalfWidth, targetRoomCenterY - targetRoomSizeY / 2 + corridorExtension, 0);
                            break;
                        case Direction.Down:
                            corridorStart = new Vector3Int(roomCenterX - corridorHalfWidth, roomCenterY - roomSizeY / 2 + corridorExtension, 0);
                            corridorEnd = new Vector3Int(targetRoomCenterX - corridorHalfWidth, targetRoomCenterY + targetRoomSizeY / 2 - corridorExtension, 0);
                            break;
                        case Direction.Right:
                            corridorStart = new Vector3Int(roomCenterX + roomSizeX / 2 - corridorExtension, roomCenterY - corridorHalfWidth, 0);
                            corridorEnd = new Vector3Int(targetRoomCenterX - targetRoomSizeX / 2 + corridorExtension, targetRoomCenterY - corridorHalfWidth, 0);
                            break;
                        case Direction.Left:
                            corridorStart = new Vector3Int(roomCenterX - roomSizeX / 2 + corridorExtension, roomCenterY - corridorHalfWidth, 0);
                            corridorEnd = new Vector3Int(targetRoomCenterX + targetRoomSizeX / 2 - corridorExtension, targetRoomCenterY - corridorHalfWidth, 0);
                            break;
                        default:
                            continue;
                    }

                    // 복도 타일 배치
                    var dx = corridorEnd.x - corridorStart.x;
                    var dy = corridorEnd.y - corridorStart.y;
                    var steps = Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dy));

                    for (int step = 0; step <= steps; step++)
                    {
                        var t = steps > 0 ? (float)step / steps : 0;
                        var x = Mathf.RoundToInt(corridorStart.x + dx * t);
                        var y = Mathf.RoundToInt(corridorStart.y + dy * t);

                        // 복도 너비만큼 타일 배치
                        for (int w = 0; w < RoomConstants.ROOM_CORRIDOR_SIZE; w++)
                        {
                            Vector3Int tilePos;
                            if (direction == Direction.Up || direction == Direction.Down)
                            {
                                tilePos = new Vector3Int(x + w, y, 0);
                            }
                            else
                            {
                                tilePos = new Vector3Int(x, y + w, 0);
                            }
                            groundTilemap.SetTile(tilePos, groundTile);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// SortingLayer ID로부터 이름을 가져옵니다.
        /// </summary>
        private static string GetSortingLayerName(int sortingLayerId)
        {
            var layers = SortingLayer.layers;
            foreach (var layer in layers)
            {
                if (layer.id == sortingLayerId)
                {
                    return layer.name;
                }
            }

            return $"Layer_{sortingLayerId}";
        }
    }
}

