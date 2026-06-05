using System;
using UnityEngine;

namespace DungeonShooter
{
    /// <summary>
    /// RLE 압축된 타일 레이어 데이터
    /// </summary>
    [Serializable]
    public class TileLayerDataRLE
    {
        [SerializeField] private int index; // RoomData의 assetAddresses 인덱스 (TileBase 어드레서블 주소)
        [SerializeField] private int layer; // SerializedRoomData.layerNames 인덱스 (문자열 레이어 컬렉션 참조)
        [SerializeField] private Vector2Int startPosition; // 연속 타일의 시작 위치
        [SerializeField] private int length; // 연속된 타일의 개수 (가로 방향)

        public int Index
        {
            get => index;
            set => index = value;
        }

        public int Layer
        {
            get => layer;
            set => layer = value;
        }

        public Vector2Int StartPosition
        {
            get => startPosition;
            set => startPosition = value;
        }

        public int Length
        {
            get => length;
            set => length = value;
        }
    }
}

