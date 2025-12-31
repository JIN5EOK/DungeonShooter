using UnityEngine;

namespace DungeonShooter
{
    /// <summary>
    /// 스테이지 게임오브젝트 컴포넌트
    /// </summary>
    public class StageComponent : MonoBehaviour
    {
        private Stage _stage;

        public Stage Stage => _stage;

        /// <summary>
        /// Stage 데이터와 연결합니다.
        /// </summary>
        public void Initialize(Stage stage)
        {
            if (stage == null)
            {
                Debug.LogError($"[{nameof(StageComponent)}] Stage가 null입니다.");
                return;
            }

            _stage = stage;
            _stage.StageComponent = this;
        }

        private void OnEnable()
        {
            if (_stage == null) return;

            // 모든 자식 RoomComponent 활성화
            var roomComponents = GetComponentsInChildren<RoomComponent>();
            foreach (var roomComponent in roomComponents)
            {
                roomComponent.gameObject.SetActive(true);
            }
        }

        private void OnDisable()
        {
            if (_stage == null) return;

            // 모든 자식 RoomComponent 비활성화
            var roomComponents = GetComponentsInChildren<RoomComponent>();
            foreach (var roomComponent in roomComponents)
            {
                roomComponent.gameObject.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            if (_stage != null)
            {
                _stage.StageComponent = null;
            }
        }

        /// <summary>
        /// Room ID로 RoomComponent를 찾습니다.
        /// </summary>
        public RoomComponent GetRoomComponent(int roomId)
        {
            if (_stage == null) return null;

            var room = _stage.GetRoom(roomId);
            if (room == null) return null;

            return room.RoomComponent;
        }
    }
}

