using System;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonShooter
{
    /// <summary>
    /// 방 데이터 - 타일과 오브젝트 배치 정보를 담는 설계도 클래스
    /// </summary>
    [Serializable]
    public class RoomData
    {
        [SerializeField] private List<string> assetAddresses = new List<string>(); // TileBase 어드레서블 주소, 게임 오브젝트 어드레서블 주소 등 동적 로드에 사용되는 주소들
        [SerializeField] private List<TileLayerData> tiles = new List<TileLayerData>(); // 타일 데이터
        [SerializeField] private List<ObjectData> objects = new List<ObjectData>();

        public List<string> AssetAddresses
        {
            get => assetAddresses;
            set => assetAddresses = value;
        }

        public List<TileLayerData> Tiles
        {
            get => tiles;
            set => tiles = value;
        }

        public List<ObjectData> Objects
        {
            get => objects;
            set => objects = value;
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

            int index = assetAddresses.IndexOf(address);
            if (index == -1)
            {
                assetAddresses.Add(address);
                index = assetAddresses.Count - 1;
            }

            return index;
        }

        /// <summary>
        /// 인덱스로 주소를 가져옵니다.
        /// </summary>
        public string GetAddress(int index)
        {
            if (index >= 0 && index < assetAddresses.Count)
            {
                return assetAddresses[index];
            }

            return null;
        }
    }
}

