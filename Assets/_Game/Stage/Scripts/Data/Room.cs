using System.Collections.Generic;
using UnityEngine;

namespace DungeonShooter
{
    /// <summary>
    /// 방 - 스테이지를 구성하는 단위
    /// </summary>
    public class Room
    {
        private readonly int _id;
        private readonly RoomData _roomData;
        private Vector2Int _position;
        private bool _isCleared;
        private Dictionary<Direction, int> _connections; // 방들간 연결 표현, Direction은 문이 위치하는 방향

        public int Id => _id;
        public RoomData RoomData => _roomData;
        public Vector2Int Position
        {
            get => _position;
            set => _position = value;
        }

        public bool IsCleared
        {
            get => _isCleared;
            set => _isCleared = value;
        }

        public Dictionary<Direction, int> Connections => _connections;

        public Room(int id, RoomData roomData, Vector2Int position)
        {
            _id = id;
            _roomData = roomData;
            _position = position;
            _isCleared = false;
            _connections = new Dictionary<Direction, int>();
        }

        /// <summary>
        /// 다른 방과 연결합니다.
        /// </summary>
        public void ConnectTo(Direction direction, int targetRoomId)
        {
            _connections[direction] = targetRoomId;
        }

        /// <summary>
        /// 특정 방향으로 연결된 방의 ID를 가져옵니다.
        /// </summary>
        public bool TryGetConnectedRoom(Direction direction, out int roomId)
        {
            return _connections.TryGetValue(direction, out roomId);
        }

        /// <summary>
        /// 연결을 제거합니다.
        /// </summary>
        public void Disconnect(Direction direction)
        {
            _connections.Remove(direction);
        }
    }
}

