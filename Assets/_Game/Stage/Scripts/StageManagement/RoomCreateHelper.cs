using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace DungeonShooter
{
    /// <summary>
    /// Room 타일맵 및 계층 구조 생성 유틸리티
    /// </summary>
    public static class RoomTilemapHelper
    {
        /// <summary>
        /// Room의 전체 계층 구조를 생성하거나 가져옵니다.
        /// </summary>
        /// <param name="roomObj">Room GameObject (null이면 새로 생성)</param>
        /// <param name="parent">부모 Transform</param>
        /// <param name="roomName">Room 이름</param>
        /// <returns>(tilemapsParent, objectsParent, baseTilemapsParent)</returns>
        public static (Transform tilemapsParent, Transform objectsParent) 
            GetOrCreateRoomStructure(
                GameObject roomObj, 
                Transform parent, 
                string roomName)
        {
            // Room GameObject 생성 또는 가져오기
            if (roomObj == null)
            {
                roomObj = new GameObject(roomName);
                if (parent != null)
                {
                    roomObj.transform.SetParent(parent);
                }
            }

            // Tilemaps 구조 생성 또는 가져오기 (사용자가 배치하는 타일맵들)
            var tilemapsParent = GetOrCreateChild(roomObj.transform, RoomConstants.TILEMAPS_GAMEOBJECT_NAME);
            if (tilemapsParent.GetComponent<Grid>() == null)
            {
                tilemapsParent.gameObject.AddComponent<Grid>();
            }

            // Tilemap_Deco 생성
            GetOrCreateDecoTilemap(roomObj.transform);

            // Objects 구조 생성 또는 가져오기
            var objectsParent = GetOrCreateChild(roomObj.transform, RoomConstants.OBJECTS_GAMEOBJECT_NAME);

            return (tilemapsParent, objectsParent);
        }

        /// <summary>
        /// 지정된 이름의 자식 GameObject를 찾거나 생성합니다.
        /// </summary>
        private static Transform GetOrCreateChild(Transform parent, string childName)
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
            if (tilemapName.Contains(RenderingLayers.Ground.LayerName))
            {
                return RenderingLayers.Ground;
            }
            else if (tilemapName.Contains(RenderingLayers.Wall.LayerName))
            {
                return RenderingLayers.Wall;
            }
            else if (tilemapName.Contains(RenderingLayers.Deco.LayerName))
            {
                return RenderingLayers.Deco;
            }
            
            // 기본값은 Ground
            return RenderingLayers.Ground;
        }

        /// <summary>
        /// 지정된 이름의 Tilemap을 찾거나 생성합니다.
        /// </summary>
        public static Tilemap GetOrCreateTilemap(Transform parent, string tilemapName)
        {
            var tilemapObj = GetOrCreateChild(parent, tilemapName);
            var tilemap = tilemapObj.GetComponent<Tilemap>();
            
            if (tilemap == null)
            {
                tilemap = tilemapObj.gameObject.AddComponent<Tilemap>();
            }
            
            var renderer = tilemapObj.GetComponent<TilemapRenderer>();
            if (renderer == null)
            {
                renderer = tilemapObj.gameObject.AddComponent<TilemapRenderer>();
            }
            
            // 타일맵 이름에 따라 적절한 렌더링 레이어 설정
                var renderingLayer = GetRenderingLayerForTilemap(tilemapName);
            renderer.sortingLayerName = renderingLayer.LayerName;
        
            
            return tilemap;
        }


        /// <summary>
        /// Tilemap_Deco를 찾거나 생성합니다.
        /// </summary>
        /// <param name="roomParent">Room GameObject의 Transform</param>
        public static Tilemap GetOrCreateDecoTilemap(Transform roomParent)
        {
            var tilemapsParent = GetOrCreateChild(roomParent, RoomConstants.TILEMAPS_GAMEOBJECT_NAME);
            if (tilemapsParent.GetComponent<Grid>() == null)
            {
                tilemapsParent.gameObject.AddComponent<Grid>();
            }
            return GetOrCreateTilemap(tilemapsParent, RoomConstants.TILEMAP_DECO_NAME);
        }

        /// <summary>
        /// Stage 레벨 타일맵 구조를 생성하거나 가져옵니다.
        /// </summary>
        /// <param name="stageObj">Stage GameObject</param>
        /// <returns>(tilemapsParent, objectsParent, groundTilemap, decoTilemap)</returns>
        public static (Transform tilemapsParent, Transform objectsParent, Tilemap groundTilemap, Tilemap decoTilemap) 
            GetOrCreateStageStructure(GameObject stageObj)
        {
            if (stageObj == null)
            {
                Debug.LogError($"[{nameof(RoomTilemapHelper)}] Stage GameObject가 null입니다.");
                return (null, null, null, null);
            }

            // Stage 레벨 타일맵 구조 생성
            var tilemapsParent = GetOrCreateChild(stageObj.transform, RoomConstants.TILEMAPS_GAMEOBJECT_NAME);
            if (tilemapsParent.GetComponent<Grid>() == null)
            {
                tilemapsParent.gameObject.AddComponent<Grid>();
            }

            // Stage 레벨 오브젝트 구조 생성
            var objectsParent = GetOrCreateChild(stageObj.transform, RoomConstants.OBJECTS_GAMEOBJECT_NAME);

            // Ground 타일맵 생성
            var groundTilemapObj = GetOrCreateChild(tilemapsParent, RoomConstants.TILEMAP_GROUND_NAME);
            var groundTilemap = groundTilemapObj.GetComponent<Tilemap>();
            if (groundTilemap == null)
            {
                groundTilemap = groundTilemapObj.gameObject.AddComponent<Tilemap>();
            }

            var groundCollider = groundTilemapObj.GetComponent<TilemapCollider2D>();
            if (groundCollider == null)
            {
                groundCollider = groundTilemapObj.gameObject.AddComponent<TilemapCollider2D>();
                groundCollider.compositeOperation = Collider2D.CompositeOperation.Merge;
            }

            var groundRenderer = groundTilemapObj.GetComponent<TilemapRenderer>();
            if (groundRenderer == null)
            {
                groundRenderer = groundTilemapObj.gameObject.AddComponent<TilemapRenderer>();
                groundRenderer.sortingLayerName = RenderingLayers.Ground.LayerName;
            }

            // Deco 타일맵 생성
            var decoTilemapObj = GetOrCreateChild(tilemapsParent, RoomConstants.TILEMAP_DECO_NAME);
            var decoTilemap = decoTilemapObj.GetComponent<Tilemap>();
            if (decoTilemap == null)
            {
                decoTilemap = decoTilemapObj.gameObject.AddComponent<Tilemap>();
            }

            var decoCollider = decoTilemapObj.GetComponent<TilemapCollider2D>();
            if (decoCollider == null)
            {
                decoCollider = decoTilemapObj.gameObject.AddComponent<TilemapCollider2D>();
                decoCollider.compositeOperation = Collider2D.CompositeOperation.Merge;
            }

            var decoRenderer = decoTilemapObj.GetComponent<TilemapRenderer>();
            if (decoRenderer == null)
            {
                decoRenderer = decoTilemapObj.gameObject.AddComponent<TilemapRenderer>();
                decoRenderer.sortingLayerName = RenderingLayers.Deco.LayerName;
            }

            return (tilemapsParent, objectsParent, groundTilemap, decoTilemap);
        }

        /// <summary>
        /// Room의 타일맵과 오브젝트를 모두 제거합니다.
        /// </summary>
        /// <param name="roomObj">Room GameObject</param>
        public static void ClearRoomObject(GameObject roomObj)
        {
            if (roomObj == null) return;

            var tilemapsParent = roomObj.transform.Find(RoomConstants.TILEMAPS_GAMEOBJECT_NAME);
            if (tilemapsParent != null)
            {
                for (int i = tilemapsParent.childCount - 1; i >= 0; i--)
                {
                    var child = tilemapsParent.GetChild(i);
                    if (Application.isEditor && !Application.isPlaying)
                    {
                        Object.DestroyImmediate(child.gameObject);
                    }
                    else
                    {
                        Object.Destroy(child.gameObject);
                    }
                }
            }

            // Objects 하위의 모든 오브젝트 제거
            var objectsParent = roomObj.transform.Find(RoomConstants.OBJECTS_GAMEOBJECT_NAME);
            if (objectsParent != null)
            {
                for (int i = objectsParent.childCount - 1; i >= 0; i--)
                {
                    var child = objectsParent.GetChild(i);
                    if (Application.isEditor && !Application.isPlaying)
                    {
                        Object.DestroyImmediate(child.gameObject);
                    }
                    else
                    {
                        Object.Destroy(child.gameObject);
                    }
                }
            }
        }

        public static void ClearTiles(Transform stageRoot)
        {
            var tilemapsParent = GetOrCreateChild(stageRoot, RoomConstants.TILEMAPS_GAMEOBJECT_NAME);
            var groundTilemap = GetOrCreateTilemap(tilemapsParent, RoomConstants.TILEMAP_GROUND_NAME);
            var decoTilemap = GetOrCreateTilemap(tilemapsParent, RoomConstants.TILEMAP_DECO_NAME);
            groundTilemap.ClearAllTiles();
            decoTilemap.ClearAllTiles();
        }
        /// <summary>
        /// 베이스 타일을 배치합니다.
        /// </summary>
        /// <param name="stageRoot">스테이지 루트 Transform</param>
        /// <param name="centerPos">방의 중심 위치 (월드 좌표)</param>
        /// <param name="roomData">방 데이터</param>
        /// <param name="stageResourceProvider">리소스 제공자</param>
        public static async Task PlaceBaseTiles(Transform stageRoot, Vector2 centerPos, RoomData roomData, IStageResourceProvider stageResourceProvider)
        {
            if (stageRoot == null || roomData == null || stageResourceProvider == null)
            {
                Debug.LogError($"[{nameof(RoomTilemapHelper)}] 파라미터가 올바르지 않습니다.");
                return;
            }

            var roomSizeX = roomData.RoomSizeX;
            var roomSizeY = roomData.RoomSizeY;

            var tilemapsParent = GetOrCreateChild(stageRoot, RoomConstants.TILEMAPS_GAMEOBJECT_NAME);
            var groundTilemap = GetOrCreateTilemap(tilemapsParent, RoomConstants.TILEMAP_GROUND_NAME);

            // Ground 타일 로드 (동기)
            var groundTile = await stageResourceProvider.GetGroundTile();

            if (groundTile == null)
            {
                Debug.LogError($"[{nameof(RoomTilemapHelper)}] Ground 타일을 로드할 수 없습니다.");
                return;
            }

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
        /// 추가 타일을 배치합니다.
        /// </summary>
        /// <param name="stageRoot">스테이지 루트 Transform</param>
        /// <param name="centerPos">방의 중심 위치 (월드 좌표)</param>
        /// <param name="roomData">방 데이터</param>
        /// <param name="stageResourceProvider">리소스 제공자</param>
        public static async Task PlaceAdditionalTiles(Transform stageRoot, Vector2 centerPos, RoomData roomData, IStageResourceProvider stageResourceProvider)
        {
            if (stageRoot == null || roomData == null || stageResourceProvider == null)
            {
                Debug.LogError($"[{nameof(RoomTilemapHelper)}] 파라미터가 올바르지 않습니다.");
                return;
            }

            if (roomData.Tiles.Count == 0)
            {
                return;
            }

            var tilemapsParent = GetOrCreateChild(stageRoot, RoomConstants.TILEMAPS_GAMEOBJECT_NAME);
            var centerPosInt = new Vector3Int((int)centerPos.x, (int)centerPos.y, 0);

            // 레이어별로 그룹화
            var tilesByLayer = roomData.Tiles.GroupBy(t => t.Layer);

            foreach (var layerGroup in tilesByLayer)
            {
                var sortingLayerId = layerGroup.Key;
                var sortingLayerName = RenderingLayers.GetLayerName(sortingLayerId);
                var tilemapName = $"Tilemap_{sortingLayerName}";

                var tilemap = GetOrCreateTilemap(tilemapsParent, tilemapName);
                var renderer = tilemap.GetComponent<TilemapRenderer>();
                if (renderer != null)
                {
                    renderer.sortingLayerID = sortingLayerId;
                }

                var uniqueTileIndices = layerGroup.Select(t => t.Index).Distinct().ToList();
                var tileCache = new Dictionary<int, TileBase>();

                foreach (var index in uniqueTileIndices)
                {
                    var address = roomData.GetAddress(index);
                    if (string.IsNullOrEmpty(address)) continue;

                    var tileBase = await stageResourceProvider.GetAsset<TileBase>(address);

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
                    var worldTilePos = localPos + centerPosInt;
                    tilemap.SetTile(worldTilePos, tileBase);
                }
            }
        }

        /// <summary>
        /// 오브젝트를 배치합니다.
        /// </summary>
        /// <param name="stageRoot">스테이지 루트 Transform</param>
        /// <param name="roomData">방 데이터</param>
        /// <param name="stageResourceProvider">리소스 제공자</param>
        /// <param name="worldOffset">월드 오프셋 (기본값: Vector3.zero)</param>
        /// <returns>생성된 오브젝트 리스트를 반환하는 Task</returns>
        public static async Task<List<GameObject>> PlaceObjectsAsync(Transform stageRoot, RoomData roomData, IStageResourceProvider stageResourceProvider, Vector3 worldOffset = default)
        {
            if (stageRoot == null || roomData == null || stageResourceProvider == null)
            {
                Debug.LogError($"[{nameof(RoomTilemapHelper)}] 파라미터가 올바르지 않습니다.");
                return new List<GameObject>();
            }

            var objectsParent = GetOrCreateChild(stageRoot, RoomConstants.OBJECTS_GAMEOBJECT_NAME);
            var createdObjects = new List<GameObject>();

            foreach (var objectData in roomData.Objects)
            {
                var address = roomData.GetAddress(objectData.Index);
                if (string.IsNullOrEmpty(address)) continue;

                var instance = await stageResourceProvider.GetInstance(address);
                if (instance != null)
                {
                    instance.transform.SetParent(objectsParent);
                    instance.transform.position = new Vector3(objectData.Position.x, objectData.Position.y, 0) + worldOffset;
                    instance.transform.rotation = objectData.Rotation;
                    createdObjects.Add(instance);
                }
            }

            return createdObjects;
        }

    }
}

