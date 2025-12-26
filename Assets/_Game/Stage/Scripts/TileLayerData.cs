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
        [SerializeField] private int index;
        [SerializeField] private int layer; // SortingLayer
        [SerializeField] private Vector2Int palettePosition; // 팔레트상의 위치
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

        public Vector2Int PalettePosition
        {
            get => palettePosition;
            set => palettePosition = value;
        }

        public Vector2Int Position
        {
            get => position;
            set => position = value;
        }
    }
}

