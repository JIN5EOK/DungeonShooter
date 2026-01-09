using System;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonShooter
{
    public enum RoomType
    {
        Start,
        Normal,
        Boss,
    }
    /// <summary>
    /// 방 데이터 - 타일과 오브젝트 배치 정보를 담는 설계도 클래스
    /// </summary>
    [Serializable]
    public class RoomData
    {
        [SerializeField] private RoomType _roomType;
        [SerializeField] private int _roomSizeX;
        [SerializeField] private int _roomSizeY;
        [SerializeField] private List<string> _assetAddresses = new List<string>(); // TileBase 어드레서블 주소, 게임 오브젝트 어드레서블 주소 등 동적 로드에 사용되는 주소들
        [SerializeField] private List<TileLayerData> _tiles = new List<TileLayerData>(); // 타일 데이터
        [SerializeField] private List<ObjectData> _objects = new List<ObjectData>();

        public int RoomSizeX => Mathf.Clamp(_roomSizeX, RoomConstants.ROOM_SIZE_MIN_X, RoomConstants.ROOM_SIZE_MAX_X);
        public int RoomSizeY => Mathf.Clamp(_roomSizeY, RoomConstants.ROOM_SIZE_MIN_Y, RoomConstants.ROOM_SIZE_MAX_Y);
        public List<string> AssetAddresses
        {
            get => _assetAddresses;
            set => _assetAddresses = value;
        }
        public List<TileLayerData> Tiles => _tiles;
        public List<ObjectData> Objects
        {
            get => _objects;
            set => _objects = value;
        }
        /// <summary>
        /// 주소를 테이블에 추가하고 인덱스를 반환합니다. 이미 존재하면 기존 인덱스를 반환합니다.
        /// </summary>
        public int GetOrAddAddress(string address)
        {
            if (string.IsNullOrEmpty(address))
            {
                return -1;
            }

            var index = _assetAddresses.IndexOf(address);
            if (index == -1)
            {
                _assetAddresses.Add(address);
                index = _assetAddresses.Count - 1;
            }

            return index;
        }

        /// <summary>
        /// 인덱스로 주소를 가져옵니다.
        /// </summary>
        public string GetAddress(int index)
        {
            if (index >= 0 && index < _assetAddresses.Count)
            {
                return _assetAddresses[index];
            }

            return null;
        }
    }
}

