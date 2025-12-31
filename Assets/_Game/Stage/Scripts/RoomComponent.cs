using Jin5eok;
using UnityEngine;

namespace DungeonShooter
{
    /// <summary>
    /// 방 게임오브젝트 컴포넌트
    /// </summary>
    public class RoomComponent : MonoBehaviour
    {
        private Room _room;
        private AddressablesScope _addressablesScope;

        public Room Room => _room;
        public AddressablesScope AddressablesScope => _addressablesScope;

        /// <summary>
        /// Room 데이터와 연결합니다.
        /// </summary>
        public void Initialize(Room room)
        {
            if (room == null)
            {
                Debug.LogError($"[{nameof(RoomComponent)}] Room이 null입니다.");
                return;
            }

            _room = room;
            _room.RoomComponent = this;
            _addressablesScope = new AddressablesScope();
        }

        private void OnDestroy()
        {
            if (_room != null)
            {
                _room.RoomComponent = null;
            }

            if (_addressablesScope != null)
            {
                _addressablesScope.Dispose();
                _addressablesScope = null;
            }
        }

        /// <summary>
        /// 특정 방향으로 연결된 RoomComponent를 가져옵니다.
        /// </summary>
        public RoomComponent GetConnectedRoomComponent(Direction direction)
        {
            if (_room == null) return null;

            if (!_room.TryGetConnectedRoom(direction, out int connectedRoomId))
            {
                return null;
            }

            var stageComponent = GetComponentInParent<StageComponent>();
            if (stageComponent == null) return null;

            return stageComponent.GetRoomComponent(connectedRoomId);
        }
    }
}

