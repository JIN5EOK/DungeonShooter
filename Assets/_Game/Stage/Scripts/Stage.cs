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
            int id = _nextRoomId++;
            Room room = new Room(id, roomData, position);
            _rooms[id] = room;
            return id;
        }

        /// <summary>
        /// 두 방을 연결합니다.
        /// </summary>
        public void ConnectRooms(int roomId1, int roomId2, Direction direction)
        {
            if (!_rooms.TryGetValue(roomId1, out Room room1) || 
                !_rooms.TryGetValue(roomId2, out Room room2))
            {
                Debug.LogWarning($"[Stage] 연결하려는 방 중 하나가 존재하지 않습니다. Room1: {roomId1}, Room2: {roomId2}");
                return;
            }

            // 양방향 연결
            room1.ConnectTo(direction, roomId2);
            room2.ConnectTo(GetOppositeDirection(direction), roomId1);
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
    }
}

