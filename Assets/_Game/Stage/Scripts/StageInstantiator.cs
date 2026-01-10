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
        /// <param name="stage">변환할 Stage</param>
        /// <param name="parent">부모 Transform (null이면 씬 루트)</param>
        /// <returns>생성된 게임오브젝트를 반환하는 Task</returns>
        public static async Task<GameObject> InstantiateStage(
            IStageResourceProvider resourceProvider,
            Stage stage,
            Transform parent = null)
        {
            //TODO : resourceProivder를 사용해 스테이지 생성하도록 수정하기
            if (stage == null)
            {
                Debug.LogError($"[{nameof(StageInstantiator)}] Stage가 null입니다.");
                return null;
            }

            var stageObj = new GameObject("Stage");
            if (parent != null)
            {
                stageObj.transform.SetParent(parent);
            }

            // StageComponent 추가 및 초기화
            var stageComponent = stageObj.AddComponent<StageComponent>();
            stageComponent.Initialize(stage);

            // 모든 방을 생성
            var roomTasks = new List<Task<GameObject>>();
            foreach (var room in stage.Rooms.Values)
            {
                if (room.RoomData == null)
                {
                    Debug.LogWarning($"[{nameof(StageInstantiator)}] Room {room.Id}의 RoomData가 null입니다. 스킵합니다.");
                    continue;
                }

                var roomName = $"Room_{room.Id}";

                var task = InstantiateRoomRuntime(
                    room,
                    stageObj.transform,
                    roomName);

                roomTasks.Add(task);
            }

            // 모든 방 생성 완료 대기
            var roomObjects = await Task.WhenAll(roomTasks);

            // 각 방의 위치 설정
            int index = 0;
            foreach (var room in stage.Rooms.Values)
            {
                if (room.RoomData == null) continue;

                if (index < roomObjects.Length && roomObjects[index] != null)
                {
                    var worldPosition = new Vector3(room.Position.x * RoomConstants.ROOM_SPACING, room.Position.y * RoomConstants.ROOM_SPACING, 0);
                    roomObjects[index].transform.position = worldPosition;
                }
                index++;
            }

            Debug.Log($"[{nameof(StageInstantiator)}] 스테이지 생성 완료. 방 개수: {roomObjects.Length}");
            return stageObj;
        }

        /// <summary>
        /// Room을 게임오브젝트로 변환합니다 (비동기).
        /// </summary>
        /// <param name="room">변환할 Room</param>
        /// <param name="parent">부모 Transform (null이면 씬 루트)</param>
        /// <param name="roomName">생성될 게임오브젝트 이름</param>
        /// <returns>생성된 게임오브젝트를 반환하는 Awaitable</returns>
        public static async Task<GameObject> InstantiateRoomRuntime(
            Room room,
            Transform parent = null,
            string roomName = "Room")
        {
            if (room == null)
            {
                Debug.LogError($"[{nameof(StageInstantiator)}] Room이 null입니다.");
                return null;
            }

            if (room.RoomData == null)
            {
                Debug.LogError($"[{nameof(StageInstantiator)}] Room의 RoomData가 null입니다.");
                return null;
            }

            var (tilemapsParent, objectsParent) = GetOrCreateRoomStructure(null, parent, roomName);
            if (tilemapsParent == null || objectsParent == null)
            {
                return null;
            }

            // RoomComponent 추가 및 초기화
            var roomObj = tilemapsParent.parent.gameObject;
            var roomComponent = roomObj.AddComponent<RoomComponent>();
            roomComponent.Initialize(room);

            // 타일맵 생성 및 배치 (비동기)
            await InstantiateTilemapsAsync(room.RoomData, tilemapsParent, roomComponent.AddressablesScope);

            // 오브젝트 생성 및 배치 (비동기)
            await InstantiateObjectsAsync(room.RoomData, objectsParent, roomComponent.AddressablesScope);

            return roomObj;
        }

        /// <summary>
        /// Room 구조를 가져오거나 생성합니다.
        /// </summary>
        private static (Transform tilemapsParent, Transform objectsParent) GetOrCreateRoomStructure(
            GameObject roomObj,
            Transform parent,
            string roomName)
        {
            Transform tilemapsParent;
            Transform objectsParent;

            if (roomObj == null)
            {
                // 새로 생성
                roomObj = new GameObject(roomName);
                if (parent != null)
                {
                    roomObj.transform.SetParent(parent);
                }

                // Tilemaps 구조 생성
                var tilemapsObj = new GameObject(RoomConstants.TILEMAPS_GAMEOBJECT_NAME);
                tilemapsObj.transform.SetParent(roomObj.transform);
                tilemapsObj.AddComponent<Grid>();
                tilemapsParent = tilemapsObj.transform;

                // Objects 구조 생성
                var objectsObj = new GameObject(RoomConstants.OBJECTS_GAMEOBJECT_NAME);
                objectsObj.transform.SetParent(roomObj.transform);
                objectsParent = objectsObj.transform;
            }
            else
            {
                // 기존 타일맵과 오브젝트 제거
                ClearExistingData(roomObj);

                // Tilemaps와 Objects 자식 찾기
                tilemapsParent = roomObj.transform.Find(RoomConstants.TILEMAPS_GAMEOBJECT_NAME);
                objectsParent = roomObj.transform.Find(RoomConstants.OBJECTS_GAMEOBJECT_NAME);

                if (tilemapsParent == null)
                {
                    Debug.LogError($"[{nameof(StageInstantiator)}] '{RoomConstants.TILEMAPS_GAMEOBJECT_NAME}' 자식이 없습니다.");
                    return (null, null);
                }

                if (objectsParent == null)
                {
                    Debug.LogError($"[{nameof(StageInstantiator)}] '{RoomConstants.OBJECTS_GAMEOBJECT_NAME}' 자식이 없습니다.");
                    return (null, null);
                }
            }

            return (tilemapsParent, objectsParent);
        }

        /// <summary>
        /// RoomData의 타일 데이터를 타일맵에 배치합니다.
        /// </summary>
        private static async Task InstantiateTilemapsAsync(
            RoomData roomData,
            Transform tilemapsParent,
            Jin5eok.AddressablesScope addressablesScope)
        {
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

                // 레이어별 Tilemap 생성
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

                    var handle = addressablesScope.LoadAssetAsync<TileBase>(address);
                    await handle.Task;

                    if (handle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
                    {
                        tileCache[index] = handle.Result;
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
            Jin5eok.AddressablesScope addressablesScope)
        {
            var uniqueObjectIndices = roomData.Objects.Select(o => o.Index).Distinct().ToList();
            var prefabCache = new Dictionary<int, GameObject>();

            // 모든 프리팹을 먼저 로드
            foreach (var index in uniqueObjectIndices)
            {
                var address = roomData.GetAddress(index);
                if (string.IsNullOrEmpty(address)) continue;

                var handle = addressablesScope.LoadAssetAsync<GameObject>(address);
                await handle.Task;

                if (handle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
                {
                    prefabCache[index] = handle.Result;
                }
            }

            // 오브젝트 인스턴스화
            foreach (var objectData in roomData.Objects)
            {
                if (!prefabCache.TryGetValue(objectData.Index, out var prefab)) continue;

                var position = new Vector3(objectData.Position.x, objectData.Position.y, 0);
                var instance = Object.Instantiate(prefab, objectsParent);
                instance.transform.position = position;
                instance.transform.rotation = objectData.Rotation;
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
        /// <returns>생성되거나 수정된 게임오브젝트</returns>
        public static GameObject InstantiateRoomEditor(
            RoomData roomData,
            GameObject roomObj = null,
            Transform parent = null,
            string roomName = "Room")
        {
            if (roomData == null)
            {
                Debug.LogError($"[{nameof(StageInstantiator)}] RoomData가 null입니다.");
                return null;
            }

            var (tilemapsParent, objectsParent) = GetOrCreateRoomStructure(roomObj, parent, roomName);
            if (tilemapsParent == null || objectsParent == null)
            {
                return null;
            }

            // AddressablesScope 생성 (에디터에서도 사용)
            using var addressablesScope = new AddressablesScope();

            // 타일맵 생성 및 배치 (비동기 메서드를 동기적으로 실행)
            var tilemapsTask = InstantiateTilemapsAsync(roomData, tilemapsParent, addressablesScope);
            tilemapsTask.Wait();

            // 오브젝트 생성 및 배치 (비동기 메서드를 동기적으로 실행)
            var objectsTask = InstantiateObjectsAsync(roomData, objectsParent, addressablesScope);
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

