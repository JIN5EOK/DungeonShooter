using UnityEngine;

namespace DungeonShooter
{
    /// <summary>
    /// 방 에디터에서 테이블 ID 기반으로 배치된 오브젝트를 표시하기 위한 마커.
    /// 저장 시 ObjectData.TableId로 직렬화됩니다.
    /// </summary>
    public class RoomObjectMarker : MonoBehaviour
    {
        [SerializeField] private int _tableId;

        public int TableId
        {
            get => _tableId;
            set => _tableId = value;
        }
    }
}
