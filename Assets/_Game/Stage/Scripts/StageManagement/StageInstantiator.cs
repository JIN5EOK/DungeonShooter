using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
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
        /// <param name="stageContext">스테이지 컨텍스트</param>
        /// <param name="playerFactory">플레이어 팩토리</param>
        /// <param name="enemyFactory">적 팩토리</param>
        /// <param name="sceneResourceProvider">씬 리소스 제공자</param>
        /// <param name="stage">변환할 Stage</param>
        /// <param name="parent">부모 Transform (null이면 씬 루트)</param>
        /// <returns>생성된 게임오브젝트를 반환하는 Task</returns>
        public static async Task<GameObject> InstantiateStage(
            StageContext stageContext,
            IPlayerFactory playerFactory,
            IEnemyFactory enemyFactory,
            ISceneResourceProvider sceneResourceProvider,
            Stage stage,
            Transform parent = null)
        {
            if (stage == null)
            {
                LogHandler.LogError(nameof(StageInstantiator), "Stage가 null입니다.");
                return null;
            }

            if (stageContext == null || playerFactory == null || enemyFactory == null || sceneResourceProvider == null)
            {
                LogHandler.LogError(nameof(StageInstantiator), "리소스 제공자가 null입니다.");
                return null;
            }

            var stageObj = new GameObject("Stage");
            if (parent != null)
            {
                stageObj.transform.SetParent(parent);
            }

            // Stage 레벨 타일맵 구조 생성
            RoomCreateHelper.GetOrCreateRoomStructure(stageObj.transform);

            var groundTile = await LoadGroundTileAsync(stageContext, sceneResourceProvider);
            // 모든 방의 타일과 오브젝트를 Stage 레벨에 배치
            foreach (var room in stage.Rooms.Values)
            {
                if (room.RoomData == null)
                {
                    LogHandler.LogWarning(nameof(StageInstantiator), $"Room {room.Id}의 RoomData가 null입니다. 스킵합니다.");
                    continue;
                }

                var worldPosition = new Vector3(room.Position.x * RoomConstants.ROOM_SPACING, room.Position.y * RoomConstants.ROOM_SPACING, 0);
                var centerPos = new Vector2(worldPosition.x, worldPosition.y);

                // 방의 타일을 Stage 레벨 타일맵에 배치
                RoomCreateHelper.PlaceBaseTiles(stageObj.transform, centerPos, room.RoomData, groundTile);
                await RoomCreateHelper.PlaceAdditionalTilesAsync(stageObj.transform, centerPos, room.RoomData, sceneResourceProvider);

                // 방의 오브젝트를 Stage 레벨 Objects에 배치
                var objects = await RoomCreateHelper.PlaceObjectsAsync(stageObj.transform, room.RoomData, playerFactory, enemyFactory, sceneResourceProvider, worldPosition);
                // 생성후 초기화 필요한 객체면 대기

                foreach (var go in objects)
                {
                    if (go != null && go.TryGetComponent(out IInitializationAwaiter initAwaiter))
                    {
                        LogHandler.Log(nameof(StageInstantiator), "초기화 필요한 객체, 대기합니다.");
                        await initAwaiter.InitializationTask;
                    }
                }

            }
            
            // 복도 생성
            CreateCorridors(groundTile, stage, RoomCreateHelper.GetOrCreateTilemap(stageObj.transform, RoomConstants.TILEMAP_GROUND_NAME));

            LogHandler.Log(nameof(StageInstantiator), $"스테이지 생성 완료. 방 개수: {stage.Rooms.Count}");
            return stageObj;
        }

        /// <summary>
        /// Ground 타일을 로드합니다.
        /// </summary>
        private static async UniTask<TileBase> LoadGroundTileAsync(StageContext stageContext, ISceneResourceProvider sceneResourceProvider)
        {
            if (stageContext?.StageConfig?.GroundTile == null || !stageContext.StageConfig.GroundTile.RuntimeKeyIsValid())
            {
                LogHandler.LogError(nameof(StageInstantiator), "Ground 타일이 설정되지 않았습니다.");
                return null;
            }

            var address = stageContext.StageConfig.GroundTile.RuntimeKey.ToString();
            return await sceneResourceProvider.GetAssetAsync<TileBase>(address);
        }

        /// <summary>
        /// 방들을 연결하는 복도를 생성합니다.
        /// </summary>
        private static void CreateCorridors(
            TileBase groundTile,
            Stage stage,
            Tilemap groundTilemap)
        {
            if (groundTile == null || stage == null || groundTilemap == null)
            {
                LogHandler.LogError(nameof(StageInstantiator), "파라미터가 올바르지 않습니다.");
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

    }
}

