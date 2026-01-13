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
        /// <param name="createBaseTilemaps">BaseTilemaps 구조도 생성할지 여부</param>
        /// <returns>(tilemapsParent, objectsParent, baseTilemapsParent)</returns>
        public static (Transform tilemapsParent, Transform objectsParent, Transform baseTilemapsParent) 
            GetOrCreateRoomStructure(
                GameObject roomObj, 
                Transform parent, 
                string roomName,
                bool createBaseTilemaps = true)
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

            // BaseTilemaps 구조 생성 또는 가져오기 (Room GameObject의 직접 자식)
            Transform baseTilemapsParent = null;
            if (createBaseTilemaps)
            {
                baseTilemapsParent = GetOrCreateBaseTilemaps(roomObj.transform);
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

            return (tilemapsParent, objectsParent, baseTilemapsParent);
        }

        /// <summary>
        /// BaseTilemaps GameObject를 찾거나 생성합니다.
        /// </summary>
        /// <param name="roomParent">Room GameObject의 Transform</param>
        public static Transform GetOrCreateBaseTilemaps(Transform roomParent)
        {
            var baseTilemaps = GetOrCreateChild(roomParent, RoomConstants.BASE_TILEMAPS_GAMEOBJECT_NAME);
            if (baseTilemaps.GetComponent<Grid>() == null)
            {
                baseTilemaps.gameObject.AddComponent<Grid>();
            }
            return baseTilemaps;
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
        public static Tilemap GetOrCreateTilemap(Transform parent, string tilemapName, bool addRenderer = true)
        {
            var tilemapObj = GetOrCreateChild(parent, tilemapName);
            var tilemap = tilemapObj.GetComponent<Tilemap>();
            
            if (tilemap == null)
            {
                tilemap = tilemapObj.gameObject.AddComponent<Tilemap>();
            }
            
            if (addRenderer)
            {
                var renderer = tilemapObj.GetComponent<TilemapRenderer>();
                if (renderer == null)
                {
                    renderer = tilemapObj.gameObject.AddComponent<TilemapRenderer>();
                }
                
                // 타일맵 이름에 따라 적절한 렌더링 레이어 설정
                var renderingLayer = GetRenderingLayerForTilemap(tilemapName);
                renderer.sortingLayerName = renderingLayer.LayerName;
            }
            
            return tilemap;
        }

        /// <summary>
        /// BaseTilemap_Ground를 찾거나 생성합니다.
        /// </summary>
        /// <param name="roomParent">Room GameObject의 Transform</param>
        public static Tilemap GetOrCreateGroundTilemap(Transform roomParent)
        {
            var baseTilemaps = GetOrCreateBaseTilemaps(roomParent);
            return GetOrCreateTilemap(baseTilemaps, RoomConstants.BASE_TILEMAP_GROUND_NAME);
        }

        /// <summary>
        /// BaseTilemap_Wall을 찾거나 생성합니다.
        /// </summary>
        /// <param name="roomParent">Room GameObject의 Transform</param>
        public static Tilemap GetOrCreateWallTilemap(Transform roomParent)
        {
            var baseTilemaps = GetOrCreateBaseTilemaps(roomParent);
            return GetOrCreateTilemap(baseTilemaps, RoomConstants.BASE_TILEMAP_WALL_NAME);
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
        /// Room의 타일맵과 오브젝트를 모두 제거합니다.
        /// </summary>
        /// <param name="roomObj">Room GameObject</param>
        public static void ClearRoomObject(GameObject roomObj)
        {
            if (roomObj == null) return;

            // Tilemaps 하위의 모든 타일맵 제거 (BaseTilemaps는 제외)
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
    }
}

