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

            // Tilemaps 구조 생성 또는 가져오기
            var tilemapsParent = GetOrCreateChild(roomObj.transform, RoomConstants.TILEMAPS_GAMEOBJECT_NAME);
            if (tilemapsParent.GetComponent<Grid>() == null)
            {
                tilemapsParent.gameObject.AddComponent<Grid>();
            }

            // Objects 구조 생성 또는 가져오기
            var objectsParent = GetOrCreateChild(roomObj.transform, RoomConstants.OBJECTS_GAMEOBJECT_NAME);

            // BaseTilemaps 구조 생성 또는 가져오기
            Transform baseTilemapsParent = null;
            if (createBaseTilemaps)
            {
                baseTilemapsParent = GetOrCreateBaseTilemaps(tilemapsParent);
            }

            return (tilemapsParent, objectsParent, baseTilemapsParent);
        }

        /// <summary>
        /// BaseTilemaps GameObject를 찾거나 생성합니다.
        /// </summary>
        /// <param name="parent">부모 Transform (Tilemaps 또는 Room GameObject)</param>
        public static Transform GetOrCreateBaseTilemaps(Transform parent)
        {
            return GetOrCreateChild(parent, RoomConstants.BASE_TILEMAPS_GAMEOBJECT_NAME);
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
            
            if (addRenderer && tilemapObj.GetComponent<TilemapRenderer>() == null)
            {
                tilemapObj.gameObject.AddComponent<TilemapRenderer>();
            }
            
            return tilemap;
        }

        /// <summary>
        /// BaseTilemap_Ground를 찾거나 생성합니다.
        /// </summary>
        /// <param name="parent">부모 Transform (Tilemaps 또는 BaseTilemaps)</param>
        public static Tilemap GetOrCreateGroundTilemap(Transform parent)
        {
            // parent가 BaseTilemaps인지 확인
            if (parent.name == RoomConstants.BASE_TILEMAPS_GAMEOBJECT_NAME)
            {
                return GetOrCreateTilemap(parent, RoomConstants.BASE_TILEMAP_GROUND_NAME);
            }
            else
            {
                var baseTilemaps = GetOrCreateBaseTilemaps(parent);
                return GetOrCreateTilemap(baseTilemaps, RoomConstants.BASE_TILEMAP_GROUND_NAME);
            }
        }

        /// <summary>
        /// BaseTilemap_Wall을 찾거나 생성합니다.
        /// </summary>
        public static Tilemap GetOrCreateWallTilemap(Transform tilemapsParent)
        {
            var baseTilemaps = GetOrCreateBaseTilemaps(tilemapsParent);
            return GetOrCreateTilemap(baseTilemaps, RoomConstants.BASE_TILEMAP_WALL_NAME);
        }

        /// <summary>
        /// BaseTilemap_Top을 찾거나 생성합니다.
        /// </summary>
        public static Tilemap GetOrCreateTopTilemap(Transform tilemapsParent)
        {
            var baseTilemaps = GetOrCreateBaseTilemaps(tilemapsParent);
            return GetOrCreateTilemap(baseTilemaps, RoomConstants.BASE_TILEMAP_TOP_NAME);
        }
    }
}

