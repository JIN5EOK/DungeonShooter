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
            var groundTilemap = GetOrCreateTilemap(stageRoot, RoomConstants.TILEMAP_GROUND_NAME);
            var decoTilemap = GetOrCreateTilemap(stageRoot, RoomConstants.TILEMAP_DECO_NAME);
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
                LogHandler.LogError(nameof(RoomCreateHelper), "파라미터가 올바르지 않습니다.");
                return;
            }

            var roomSizeX = roomData.RoomSizeX;
            var roomSizeY = roomData.RoomSizeY;

            var tilemapsParent = GetOrCreateChild(stageRoot, RoomConstants.TILEMAPS_GAMEOBJECT_NAME);
            var groundTilemap = GetOrCreateTilemap(stageRoot, RoomConstants.TILEMAP_GROUND_NAME);

            // Ground 타일 로드 (동기)
            var groundTile = await stageResourceProvider.GetGroundTile();

            if (groundTile == null)
            {
                LogHandler.LogError(nameof(RoomCreateHelper), "Ground 타일을 로드할 수 없습니다.");
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
                LogHandler.LogError(nameof(RoomCreateHelper), "파라미터가 올바르지 않습니다.");
                return;
            }

            if (roomData.Tiles.Count == 0)
            {
                return;
            }

            var centerPosInt = new Vector3Int((int)centerPos.x, (int)centerPos.y, 0);

            // 타일 배치
            foreach (var tileData in roomData.Tiles)
            {
                var address = roomData.GetAddress(tileData.Index);
                if (string.IsNullOrEmpty(address)) continue;

                var tileBase = await stageResourceProvider.GetAsset<TileBase>(address);
                if (tileBase == null) continue;

                var sortingLayerName = RenderingLayers.GetLayerName(tileData.Layer);
                var tilemapName = $"Tilemap_{sortingLayerName}";
                var tilemap = GetOrCreateTilemap(stageRoot, tilemapName);
                
                var renderer = tilemap.GetComponent<TilemapRenderer>();
                if (renderer != null)
                {
                    renderer.sortingLayerID = tileData.Layer;
                }

                var localPos = new Vector3Int(tileData.Position.x, tileData.Position.y, 0);
                var worldTilePos = localPos + centerPosInt;
                tilemap.SetTile(worldTilePos, tileBase);
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
                LogHandler.LogError(nameof(RoomCreateHelper), "파라미터가 올바르지 않습니다.");
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

