using System.Collections.Generic;
using UnityEngine;

namespace DungeonShooter
{
    /// <summary>
    /// 스테이지 - 여러 방들이 연결된 그래프 구조
    /// </summary>
    public class Stage
    {
        private Dictionary<int, Room> _rooms = new Dictionary<int, Room>();
        private int _nextRoomId = 0;

        public IReadOnlyDictionary<int, Room> Rooms => _rooms;

        /// <summary>
        /// 방을 추가합니다.
        /// </summary>
        /// <returns>생성된 방의 ID</returns>
        public int AddRoom(RoomData roomData, Vector2Int position)
        {
            var id = _nextRoomId++;
            var room = new Room(id, roomData, position);
            _rooms[id] = room;
            return id;
        }

        /// <summary>
        /// 특정 방의 지정된 방향에 있는 방과 연결합니다.
        /// 해당 방향에 방이 없으면 연결하지 않습니다.
        /// </summary>
        /// <param name="roomId">연결할 방의 ID</param>
        /// <param name="direction">연결할 방향</param>
        /// <returns>연결 성공 여부</returns>
        public bool ConnectRoomInDirection(int roomId, Direction direction)
        {
            if (!_rooms.TryGetValue(roomId, out Room room))
            {
                Debug.LogWarning($"[{nameof(Stage)}] 방을 찾을 수 없습니다. RoomId: {roomId}");
                return false;
            }

            // 이미 해당 방향에 연결이 있으면 스킵
            if (room.Connections.ContainsKey(direction))
            {
                return false;
            }

            // 방향에 따른 인접 위치 계산
            var adjacentPosition = room.Position + GetDirectionVector(direction);
            
            // 해당 위치에 방이 있는지 찾기
            var adjacentRoom = FindRoomAtPosition(adjacentPosition);
            if (adjacentRoom == null)
            {
                return false; // 해당 방향에 방이 없음
            }

            // 양방향 연결
            room.ConnectTo(direction, adjacentRoom.Id);
            adjacentRoom.ConnectTo(GetOppositeDirection(direction), roomId);
            
            return true;
        }

        /// <summary>
        /// 방을 가져옵니다.
        /// </summary>
        public Room GetRoom(int id)
        {
            _rooms.TryGetValue(id, out Room room);
            return room;
        }

        /// <summary>
        /// 방이 존재하는지 확인합니다.
        /// </summary>
        public bool HasRoom(int id)
        {
            return _rooms.ContainsKey(id);
        }

        /// <summary>
        /// 방의 RoomData를 교체합니다. (새로운 Room 객체로 교체)
        /// </summary>
        public bool ReplaceRoomData(int roomId, RoomData newRoomData)
        {
            if (!_rooms.TryGetValue(roomId, out Room oldRoom))
            {
                return false;
            }

            // 새 Room 객체 생성 (기존 연결 정보 유지)
            var newRoom = new Room(roomId, newRoomData, oldRoom.Position);
            newRoom.IsCleared = oldRoom.IsCleared;

            // 기존 연결 정보 복사
            foreach (var connection in oldRoom.Connections)
            {
                newRoom.ConnectTo(connection.Key, connection.Value);
            }

            _rooms[roomId] = newRoom;
            return true;
        }

        /// <summary>
        /// 특정 위치에 있는 방을 찾습니다.
        /// </summary>
        private Room FindRoomAtPosition(Vector2Int position)
        {
            foreach (var room in _rooms.Values)
            {
                if (room.Position == position)
                {
                    return room;
                }
            }
            return null;
        }

        /// <summary>
        /// 방향에 따른 Vector2Int 오프셋을 반환합니다.
        /// </summary>
        private Vector2Int GetDirectionVector(Direction direction)
        {
            return direction switch
            {
                Direction.Up => Vector2Int.up,      // (0, 1)
                Direction.Down => Vector2Int.down,    // (0, -1)
                Direction.Right => Vector2Int.right,    // (1, 0)
                Direction.Left => Vector2Int.left,    // (-1, 0)
                _ => Vector2Int.zero
            };
        }

        /// <summary>
        /// 반대 방향을 반환합니다.
        /// </summary>
        private Direction GetOppositeDirection(Direction direction)
        {
            return direction switch
            {
                Direction.Up => Direction.Down,
                Direction.Down => Direction.Up,
                Direction.Right => Direction.Left,
                Direction.Left => Direction.Right,
                _ => direction
            };
        }
    }
}

