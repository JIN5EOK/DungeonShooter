using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DungeonShooter
{
    /// <summary>
    /// 스테이지 생성 관련 세부 로직을 담당하는 클래스
    /// </summary>
    public static class StageGenerator
    {
        private const int DEFAULT_ROOM_COUNT = 15;
        private const int DEFAULT_GRID_SIZE = 10;

        /// <summary>
        /// 스테이지를 생성합니다.
        /// </summary>
        /// <param name="roomCount">생성할 방의 개수</param>
        /// <param name="normalRoomDataPaths">일반 방 데이터 파일 경로 리스트</param>
        /// <param name="bossRoomDataPath">보스 방 데이터 파일 경로 (선택)</param>
        /// <param name="startRoomDataPath">시작 방 데이터 파일 경로 (선택)</param>
        public static Stage GenerateStage(
            int roomCount = DEFAULT_ROOM_COUNT,
            List<string> normalRoomDataPaths = null,
            string bossRoomDataPath = null,
            string startRoomDataPath = null)
        {
            var stage = new Stage();

            // 1. 빈 노드들을 2차원 평면에 배치 (시작 방 설정 포함)
            var roomIds = PlaceRoomsOnGrid(stage, roomCount);

            if (roomIds.Count == 0)
            {
                Debug.LogError($"[{nameof(StageGenerator)}] 방 배치에 실패했습니다.");
                return stage;
            }

            // 2. 시작 방에서 거리상 가장 먼 방을 보스 방으로 설정
            SetStartAndBossRooms(stage, roomIds, startRoomDataPath, bossRoomDataPath);

            // 3. 방들이 끊어지지 않고 최소 신장 트리 형태를 갖추도록 연결
            BuildSpanningTree(stage, roomIds);

            // 4. 자연스러워 보이도록 랜덤 엣지 추가 (보스 방 제외)
            AddRandomEdges(stage, roomIds);

            // 5. 각 노드를 적절한 방으로 설정 (RoomData 할당)
            AssignRoomData(stage, roomIds, normalRoomDataPaths);

            Debug.Log($"[{nameof(StageGenerator)}] 스테이지 생성 완료. 방 개수: {roomIds.Count}");
            LogStageMap(stage, roomIds);
            return stage;
        }

        /// <summary>
        /// 방들을 2차원 평면에 배치합니다.
        /// (0,0) 위치에 루트 노드를 생성하고 이를 시작 방으로 설정합니다.
        /// </summary>
        private static List<int> PlaceRoomsOnGrid(Stage stage, int roomCount)
        {
            var roomIds = new List<int>();
            var usedPositions = new HashSet<Vector2Int>();

            // 임시 RoomData (나중에 실제 RoomData로 교체됨)
            var tempRoomData = new RoomData();

            // (0,0) 위치에 루트 노드 생성, 이 노드를 시작 방으로 설정
            var startPos = Vector2Int.zero;
            var startRoomId = stage.AddRoom(tempRoomData, startPos);
            roomIds.Add(startRoomId);
            usedPositions.Add(startPos);

            var directions = new List<Vector2Int>
            {
                Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
            };

            int count = 0;

            while (roomIds.Count < roomCount)
            {
                count++;

                // 인접 빈 위치가 있는 방들과 유효한 방향 계산 
                var candidates = new List<(int roomId, List<Vector2Int> availableDirs)>();
                foreach (int roomId in roomIds)
                {
                    var room = stage.GetRoom(roomId);
                    if (room == null) continue;

                    var dirs = directions
                        .Where(direction =>
                        {
                            var newPos = room.Position + direction;
                            return Mathf.Abs(newPos.x) <= DEFAULT_GRID_SIZE &&
                                   Mathf.Abs(newPos.y) <= DEFAULT_GRID_SIZE &&
                                   !usedPositions.Contains(newPos);
                        })
                        .ToList();

                    if (dirs.Count > 0)
                    {
                        candidates.Add((roomId, dirs));
                    }
                }

                // 인접 빈 위치가 있는 방이 없으면 종료
                if (candidates.Count == 0)
                {
                    Debug.LogWarning($"[{nameof(StageGenerator)}] 더 이상 배치할 수 있는 위치가 없습니다.");
                    break;
                }

                // 랜덤 선택 및 방 추가
                var (selectedRoomId, availableDirs) = candidates[Random.Range(0, candidates.Count)];
                var selectedRoom = stage.GetRoom(selectedRoomId);
                if (selectedRoom == null) continue;

                var direction = availableDirs[Random.Range(0, availableDirs.Count)];
                var newPos = selectedRoom.Position + direction;

                var newRoomId = stage.AddRoom(tempRoomData, newPos);
                roomIds.Add(newRoomId);
                usedPositions.Add(newPos);
            }

            return roomIds;
        }

        /// <summary>
        /// 방들이 끊어지지 않고 신장 트리 형태를 갖추도록 연결합니다.
        /// BFS 탐색을 통해 연결합니다.
        /// </summary>
        private static void BuildSpanningTree(Stage stage, List<int> roomIds)
        {
            if (roomIds.Count <= 1)
            {
                return;
            }

            var visited = new HashSet<int>();
            var queue = new Queue<int>();
            var directionVectors = new Dictionary<Direction, Vector2Int>
            {
                { Direction.North, Vector2Int.up },
                { Direction.South, Vector2Int.down },
                { Direction.East, Vector2Int.right },
                { Direction.West, Vector2Int.left }
            };

            // 시작 방(첫 번째 방)에서 BFS 시작
            var startRoomId = roomIds[0];
            queue.Enqueue(startRoomId);
            visited.Add(startRoomId);

            var connectedCount = 0;
            var targetConnections = roomIds.Count - 1;

            while (queue.Count > 0 && connectedCount < targetConnections)
            {
                var currentRoomId = queue.Dequeue();
                var currentRoom = stage.GetRoom(currentRoomId);
                if (currentRoom == null) continue;

                // 인접한 방들을 랜덤한 순서로 확인
                var directions = new List<Direction>(directionVectors.Keys);
                for (int i = 0; i < directions.Count; i++)
                {
                    int randomIndex = Random.Range(i, directions.Count);
                    var temp = directions[i];
                    directions[i] = directions[randomIndex];
                    directions[randomIndex] = temp;
                }

                foreach (var direction in directions)
                {
                    if (connectedCount >= targetConnections)
                    {
                        break;
                    }

                    var adjacentPos = currentRoom.Position + directionVectors[direction];
                    var adjacentRoom = stage.Rooms.Values.FirstOrDefault(r => r.Position == adjacentPos);
                    
                    // 인접 방이 있고 아직 방문하지 않은 경우 연결
                    if (adjacentRoom != null && !visited.Contains(adjacentRoom.Id))
                    {
                        if (stage.ConnectRoomInDirection(currentRoomId, direction))
                        {
                            visited.Add(adjacentRoom.Id);
                            queue.Enqueue(adjacentRoom.Id);
                            connectedCount++;
                        }
                    }
                }
            }

            if (connectedCount < targetConnections)
            {
                Debug.LogWarning($"[{nameof(StageGenerator)}] 모든 방이 연결되지 않았습니다. 연결된 엣지: {connectedCount}/{targetConnections}");
            }
        }

        /// <summary>
        /// 시작 방에서 거리상 가장 먼 방을 보스 방으로 설정합니다.
        /// </summary>
        private static void SetStartAndBossRooms(Stage stage, List<int> roomIds, string startRoomDataPath, string bossRoomDataPath)
        {
            if (roomIds.Count < 2)
            {
                return;
            }

            // BFS로 시작 방에서 가장 먼 방 찾기 (보스 방)
            var startRoomId = roomIds[0];
            var bossRoomId = FindFarthestRoom(stage, startRoomId);

            // 시작 방의 RoomData 교체
            if (!string.IsNullOrEmpty(startRoomDataPath))
            {
                var startRoomData = RoomDataSerializer.DeserializeRoom(startRoomDataPath);
                if (startRoomData != null)
                {
                    stage.ReplaceRoomData(startRoomId, startRoomData);
                }
                else
                {
                    Debug.LogWarning($"[{nameof(StageGenerator)}] 시작 방 RoomData 로드 실패: {startRoomDataPath}");
                }
            }

            // 보스 방의 RoomData 교체
            if (!string.IsNullOrEmpty(bossRoomDataPath))
            {
                var bossRoomData = RoomDataSerializer.DeserializeRoom(bossRoomDataPath);
                if (bossRoomData != null)
                {
                    stage.ReplaceRoomData(bossRoomId, bossRoomData);
                }
                else
                {
                    Debug.LogWarning($"[{nameof(StageGenerator)}] 보스 방 RoomData 로드 실패: {bossRoomDataPath}");
                }
            }
        }

        /// <summary>
        /// BFS로 가장 먼 방을 찾습니다.
        /// </summary>
        private static int FindFarthestRoom(Stage stage, int startRoomId)
        {
            var queue = new Queue<int>();
            var distances = new Dictionary<int, int>();
            var visited = new HashSet<int>();

            queue.Enqueue(startRoomId);
            distances[startRoomId] = 0;
            visited.Add(startRoomId);

            var farthestRoomId = startRoomId;
            var maxDistance = 0;

            while (queue.Count > 0)
            {
                var currentRoomId = queue.Dequeue();
                var currentRoom = stage.GetRoom(currentRoomId);
                if (currentRoom == null) continue;

                var currentDistance = distances[currentRoomId];
                if (currentDistance > maxDistance)
                {
                    maxDistance = currentDistance;
                    farthestRoomId = currentRoomId;
                }

                foreach (var connectedRoomId in currentRoom.Connections.Values)
                {
                    if (!visited.Contains(connectedRoomId))
                    {
                        visited.Add(connectedRoomId);
                        distances[connectedRoomId] = currentDistance + 1;
                        queue.Enqueue(connectedRoomId);
                    }
                }
            }

            return farthestRoomId;
        }

        /// <summary>
        /// 자연스러워 보이도록 랜덤 엣지를 추가합니다.
        /// 보스 방 등 특수 방에는 엣지를 추가하지 않습니다.
        /// </summary>
        private static void AddRandomEdges(Stage stage, List<int> roomIds)
        {
            if (roomIds.Count < 2)
            {
                return;
            }

            // 시작 방 찾기 (첫 번째 방)
            var startRoomId = roomIds[0];
            var distances = CalculateDistancesFromStart(stage, startRoomId);

            // 보스 방 찾기 (시작 방에서 가장 먼 방)
            var bossRoomId = roomIds.Count >= 2 ? FindFarthestRoom(stage, startRoomId) : -1;
            var specialRoomIds = new HashSet<int> { bossRoomId }; // 추후 상점 등 다른 특수방 추가 가능

            var candidateEdges = new List<(int roomId, Direction direction)>();
            var directionVectors = new Dictionary<Direction, Vector2Int>
            {
                { Direction.North, Vector2Int.up },
                { Direction.South, Vector2Int.down },
                { Direction.East, Vector2Int.right },
                { Direction.West, Vector2Int.left }
            };
            // 실제로 인접한 방이 있고, 아직 연결되지 않은 엣지만 생성 (특수 방 제외)
            foreach (int roomId in roomIds)
            {
                // 특수 방은 엣지 추가 대상에서 제외
                if (specialRoomIds.Contains(roomId))
                {
                    continue;
                }

                var room = stage.GetRoom(roomId);
                if (room == null) continue;

                foreach (var (dir, vector) in directionVectors)
                {
                    // 아직 연결되어 있지 않고, 실제로 인접한 방이 있는 경우 엣지 추가
                    if (!room.Connections.ContainsKey(dir))
                    {
                        var adjacentPos = room.Position + vector;
                        var adjacentRoom = stage.Rooms.Values.FirstOrDefault(r => r.Position == adjacentPos);
                        // 인접 방이 특수 방이 아닌 경우에만 엣지 추가
                        if (adjacentRoom != null && !specialRoomIds.Contains(adjacentRoom.Id))
                        {
                            candidateEdges.Add((roomId, dir));
                        }
                    }
                }
            }

            // 엣지를 랜덤하게 섞기
            for (int i = 0; i < candidateEdges.Count; i++)
            {
                int randomIndex = Random.Range(i, candidateEdges.Count);
                (candidateEdges[i], candidateEdges[randomIndex]) = (candidateEdges[randomIndex], candidateEdges[i]);
            }

            // 거리에 따라 확률적으로 연결
            foreach (var (roomId, direction) in candidateEdges)
            {
                var room = stage.GetRoom(roomId);
                if (room == null) continue;

                // 거리가 멀수록 연결 확률 낮음
                var distance = distances.ContainsKey(roomId) ? distances[roomId] : 0;
                var probability = Mathf.Max(0.1f, 1.0f - (distance * 0.15f)); // 거리당 15% 감소, 최소 10%

                if (Random.value < probability)
                {
                    stage.ConnectRoomInDirection(roomId, direction);
                }
            }
        }

        /// <summary>
        /// 시작 방에서 각 방까지의 거리를 계산합니다.
        /// </summary>
        private static Dictionary<int, int> CalculateDistancesFromStart(Stage stage, int startRoomId)
        {
            var distances = new Dictionary<int, int>();
            var queue = new Queue<int>();
            var visited = new HashSet<int>();

            queue.Enqueue(startRoomId);
            distances[startRoomId] = 0;
            visited.Add(startRoomId);

            while (queue.Count > 0)
            {
                var currentRoomId = queue.Dequeue();
                var currentRoom = stage.GetRoom(currentRoomId);
                if (currentRoom == null) continue;

                var currentDistance = distances[currentRoomId];

                foreach (var connectedRoomId in currentRoom.Connections.Values)
                {
                    if (!visited.Contains(connectedRoomId))
                    {
                        visited.Add(connectedRoomId);
                        distances[connectedRoomId] = currentDistance + 1;
                        queue.Enqueue(connectedRoomId);
                    }
                }
            }

            return distances;
        }

        /// <summary>
        /// 각 방에 RoomData를 할당합니다.
        /// </summary>
        private static void AssignRoomData(Stage stage, List<int> roomIds, List<string> normalRoomDataPaths)
        {
            if (normalRoomDataPaths == null || normalRoomDataPaths.Count == 0)
            {
                Debug.LogWarning($"[{nameof(StageGenerator)}] RoomData 경로가 제공되지 않았습니다. 임시 RoomData를 사용합니다.");
                return;
            }

            foreach (int roomId in roomIds)
            {
                var room = stage.GetRoom(roomId);
                if (room == null) continue;

                // 이미 RoomData가 할당되어 있으면 스킵 (시작 방, 보스 방 등)
                if (room.RoomData != null && room.RoomData.Tiles.Count > 0)
                {
                    continue;
                }

                // 랜덤하게 RoomData 선택
                var randomPath = normalRoomDataPaths[Random.Range(0, normalRoomDataPaths.Count)];
                var roomData = RoomDataSerializer.DeserializeRoom(randomPath);

                if (roomData != null)
                {
                    stage.ReplaceRoomData(roomId, roomData);
                }
                else
                {
                    Debug.LogWarning($"[{nameof(StageGenerator)}] RoomData 로드 실패: {randomPath}");
                }
            }
        }

        /// <summary>
        /// 방이 특정 방향으로 실제로 연결되어 있는지 확인합니다.
        /// </summary>
        private static bool IsConnected(Room room, Direction direction, Dictionary<Vector2Int, Room> roomMap, Dictionary<Direction, Vector2Int> dirToVector)
        {
            if (!room.Connections.ContainsKey(direction))
            {
                return false;
            }

            var connectedRoomId = room.Connections[direction];
            var expectedPos = room.Position + dirToVector[direction];

            return roomMap.ContainsKey(expectedPos) &&
                   roomMap[expectedPos].Id == connectedRoomId;
        }

        /// <summary>
        /// 반대 방향을 반환합니다.
        /// </summary>
        private static Direction GetOppositeDirection(Direction direction)
        {
            return direction switch
            {
                Direction.North => Direction.South,
                Direction.South => Direction.North,
                Direction.East => Direction.West,
                Direction.West => Direction.East,
                _ => direction
            };
        }
        
                /// <summary>
        /// 스테이지 맵을 텍스트로 출력합니다.
        /// </summary>
        private static void LogStageMap(Stage stage, List<int> roomIds)
        {
            if (roomIds.Count == 0)
            {
                return;
            }

            // 시작 방과 보스 방 ID 찾기
            var startRoomId = roomIds[0];
            var bossRoomId = roomIds.Count >= 2 ? FindFarthestRoom(stage, startRoomId) : -1;

            // 방 위치 범위 계산
            var minX = int.MaxValue;
            var maxX = int.MinValue;
            var minY = int.MaxValue;
            var maxY = int.MinValue;

            var roomMap = new Dictionary<Vector2Int, Room>();
            foreach (int roomId in roomIds)
            {
                var room = stage.GetRoom(roomId);
                if (room == null) continue;

                roomMap[room.Position] = room;
                minX = Mathf.Min(minX, room.Position.x);
                maxX = Mathf.Max(maxX, room.Position.x);
                minY = Mathf.Min(minY, room.Position.y);
                maxY = Mathf.Max(maxY, room.Position.y);
            }

            // Direction을 Vector2Int로 변환
            var dirToVector = new Dictionary<Direction, Vector2Int>
            {
                { Direction.North, Vector2Int.up },
                { Direction.South, Vector2Int.down },
                { Direction.East, Vector2Int.right },
                { Direction.West, Vector2Int.left }
            };

            var mapLines = new List<string>();
            mapLines.Add($"[{nameof(StageGenerator)}] 맵 레이아웃:");

            // Y축을 역순으로 (위에서 아래로)
            for (int y = maxY; y >= minY; y--)
            {
                var nodeLine = new System.Text.StringBuilder();
                var verticalLine = new System.Text.StringBuilder();

                for (int x = minX; x <= maxX; x++)
                {
                    var pos = new Vector2Int(x, y);
                    var hasRoom = roomMap.ContainsKey(pos);

                    if (hasRoom)
                    {
                        var room = roomMap[pos];
                        
                        // 연결 상태 확인
                        var hasWest = IsConnected(room, Direction.West, roomMap, dirToVector);
                        var hasEast = IsConnected(room, Direction.East, roomMap, dirToVector);
                        var hasNorth = IsConnected(room, Direction.North, roomMap, dirToVector);
                        var hasSouth = IsConnected(room, Direction.South, roomMap, dirToVector);

                        // 노드 문자 (시작 방과 보스 방 우선 표시)
                        string nodeChar;
                        if (room.Id == startRoomId && room.Id == bossRoomId)
                        {
                            nodeChar = "※"; // 시작 방과 보스 방이 같은 경우
                        }
                        else if (room.Id == startRoomId)
                        {
                            nodeChar = "S"; // 시작 방
                        }
                        else if (room.Id == bossRoomId)
                        {
                            nodeChar = "B"; // 보스 방
                        }
                        else
                        {
                            var connectionCount = (hasWest ? 1 : 0) + (hasEast ? 1 : 0) + 
                                                (hasNorth ? 1 : 0) + (hasSouth ? 1 : 0);
                            nodeChar = connectionCount switch
                            {
                                0 => "○",
                                1 => "●",
                                2 => "■",
                                _ => "★"
                            };
                        }

                        // 노드
                        nodeLine.Append(nodeChar);
                        // 오른쪽 연결선만 표시
                        nodeLine.Append(hasEast ? "─" : "");

                        // 수직 연결선 (아래로 연결되어 있을 때만 표시, 위쪽 연결선은 위 행에서 표시됨)
                        verticalLine.Append(hasSouth ? " │ " : "   ");
                    }
                    else
                    {
                        // 빈 공간
                        // 왼쪽 방이 오른쪽으로 연결되어 있는지 확인
                        var posLeft = new Vector2Int(x - 1, y);
                        var hasLeftConnection = false;
                        if (roomMap.ContainsKey(posLeft))
                        {
                            var roomLeft = roomMap[posLeft];
                            hasLeftConnection = IsConnected(roomLeft, Direction.East, roomMap, dirToVector);
                        }

                        if (hasLeftConnection)
                        {
                            nodeLine.Append("  ─"); // 왼쪽 방의 오른쪽 연결선이 지나감
                        }
                        else
                        {
                            nodeLine.Append("   ");
                        }

                        // 위쪽 방이 아래로 연결되어 있는지 확인
                        var posAbove = new Vector2Int(x, y + 1);
                        if (roomMap.ContainsKey(posAbove))
                        {
                            var roomAbove = roomMap[posAbove];
                            var posBelow = new Vector2Int(x, y - 1);
                            if (IsConnected(roomAbove, Direction.South, roomMap, dirToVector) &&
                                roomMap.ContainsKey(posBelow))
                            {
                                verticalLine.Append(" │ ");
                            }
                            else
                            {
                                verticalLine.Append("   ");
                            }
                        }
                        else
                        {
                            verticalLine.Append("   ");
                        }
                    }
                }

                mapLines.Add(nodeLine.ToString());
                if (y > minY)
                {
                    mapLines.Add(verticalLine.ToString());
                }
            }

            Debug.Log(string.Join("\n", mapLines));
        }
    }
}


