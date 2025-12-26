using System;
using UnityEngine;

namespace DungeonShooter
{
    /// <summary>
    /// 타일 레이어 데이터
    /// </summary>
    [Serializable]
    public class TileLayerData
    {
        [SerializeField] private int index; // RoomData의 assetAddresses 인덱스 (TileBase 어드레서블 주소)
        [SerializeField] private int layer; // SortingLayer
        [SerializeField] private Vector2Int position; // 방 생성시 배치될 위치

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

        public Vector2Int Position
        {
            get => position;
            set => position = value;
        }
    }
}

