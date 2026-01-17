using System;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonShooter
{
    /// <summary>
    /// 직렬화 전용 RoomData - RLE 압축된 형태로 저장
    /// </summary>
    [Serializable]
    public class SerializedRoomData
    {
        [SerializeField] private int roomSizeX;
        [SerializeField] private int roomSizeY;
        [SerializeField] private List<string> assetAddresses = new List<string>();
        [SerializeField] private List<TileLayerDataRLE> tilesRLE = new List<TileLayerDataRLE>(); // RLE 압축된 타일
        [SerializeField] private List<ObjectData> objects = new List<ObjectData>();

        public int RoomSizeX
        {
            get => roomSizeX;
            set => roomSizeX = value;
        }

        public int RoomSizeY
        {
            get => roomSizeY;
            set => roomSizeY = value;
        }

        public List<string> AssetAddresses
        {
            get => assetAddresses;
            set => assetAddresses = value;
        }

        public List<TileLayerDataRLE> TilesRLE
        {
            get => tilesRLE;
            set => tilesRLE = value;
        }

        public List<ObjectData> Objects
        {
            get => objects;
            set => objects = value;
        }

        /// <summary>
        /// RoomData로 변환합니다 (RLE 압축 해제).
        /// </summary>
        public RoomData ToRoomData()
        {
            var roomData = new RoomData();
            roomData.RoomSizeX = roomSizeX;
            roomData.RoomSizeY = roomSizeY;
            roomData.AssetAddresses = new List<string>(assetAddresses);
            roomData.Objects = new List<ObjectData>(objects);

            // RLE 압축 해제
            foreach (TileLayerDataRLE rleData in tilesRLE)
            {
                for (int i = 0; i < rleData.Length; i++)
                {
                    var tileData = new TileLayerData
                    {
                        Index = rleData.Index,
                        Layer = rleData.Layer,
                        Position = new Vector2Int(rleData.StartPosition.x + i, rleData.StartPosition.y)
                    };

                    roomData.Tiles.Add(tileData);
                }
            }

            return roomData;
        }

        /// <summary>
        /// RoomData로부터 생성합니다 (RLE 압축).
        /// </summary>
        public static SerializedRoomData FromRoomData(RoomData roomData)
        {
            if (roomData == null)
            {
                return null;
            }

            var serialized = new SerializedRoomData();
            serialized.RoomSizeX = roomData.RoomSizeX;
            serialized.RoomSizeY = roomData.RoomSizeY;
            serialized.AssetAddresses = new List<string>(roomData.AssetAddresses);
            serialized.Objects = new List<ObjectData>(roomData.Objects);

            // RLE 압축
            CompressTiles(roomData.Tiles, serialized.TilesRLE);

            return serialized;
        }

        /// <summary>
        /// 타일 데이터를 RLE로 압축합니다.
        /// </summary>
        private static void CompressTiles(List<TileLayerData> tiles, List<TileLayerDataRLE> tilesRLE)
        {
            if (tiles == null || tiles.Count == 0)
            {
                return;
            }

            tilesRLE.Clear();

            // 타일들을 레이어, Y 좌표, index, X 좌표 순으로 정렬
            var sortedTiles = new List<TileLayerData>(tiles);
            sortedTiles.Sort((a, b) =>
            {
                var layerCompare = a.Layer.CompareTo(b.Layer);
                if (layerCompare != 0) return layerCompare;

                var yCompare = a.Position.y.CompareTo(b.Position.y);
                if (yCompare != 0) return yCompare;

                var indexCompare = a.Index.CompareTo(b.Index);
                if (indexCompare != 0) return indexCompare;

                return a.Position.x.CompareTo(b.Position.x);
            });

            // RLE 압축
            for (int i = 0; i < sortedTiles.Count; i++)
            {
                var current = sortedTiles[i];
                var startPos = current.Position;
                var length = 1;

                // 같은 레이어, 같은 Y, 같은 index인 연속된 타일 찾기
                while (i + length < sortedTiles.Count)
                {
                    var next = sortedTiles[i + length];
                    if (next.Layer != current.Layer ||
                        next.Index != current.Index ||
                        next.Position.y != current.Position.y ||
                        next.Position.x != startPos.x + length)
                    {
                        break;
                    }
                    length++;
                }

                // RLE 엔트리 생성
                var rleData = new TileLayerDataRLE
                {
                    Index = current.Index,
                    Layer = current.Layer,
                    StartPosition = startPos,
                    Length = length
                };

                tilesRLE.Add(rleData);
                i += length - 1; // 다음 그룹으로 이동
            }
        }
    }
}

