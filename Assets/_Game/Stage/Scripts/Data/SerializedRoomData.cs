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
        [SerializeField] private List<string> layerNames = new List<string>(); // 레이어 이름 컬렉션 (어드레스와 동일하게 인덱스 참조)
        [SerializeField] private List<TileLayerDataRLE> tilesRLE = new List<TileLayerDataRLE>(); // RLE 압축된 타일 (Layer는 layerNames 인덱스)
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

        public List<string> LayerNames
        {
            get => layerNames;
            set => layerNames = value;
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

            // RLE 압축 해제 (레이어 인덱스 -> SortingLayer ID 변환)
            foreach (TileLayerDataRLE rleData in tilesRLE)
            {
                var layerId = GetSortingLayerIdFromIndex(rleData.Layer);
                for (int i = 0; i < rleData.Length; i++)
                {
                    var tileData = new TileLayerData
                    {
                        Index = rleData.Index,
                        Layer = layerId,
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

            // RLE 압축 (레이어는 문자열 컬렉션 인덱스로 저장)
            CompressTiles(roomData.Tiles, serialized);

            return serialized;
        }

        /// <summary>
        /// 레이어 이름 컬렉션에 추가하고 인덱스를 반환합니다. 이미 존재하면 기존 인덱스를 반환합니다.
        /// </summary>
        private int GetOrAddLayerIndex(string layerName)
        {
            if (string.IsNullOrEmpty(layerName))
            {
                return -1;
            }

            var index = layerNames.IndexOf(layerName);
            if (index == -1)
            {
                layerNames.Add(layerName);
                index = layerNames.Count - 1;
            }

            return index;
        }

        /// <summary>
        /// layerNames 인덱스를 SortingLayer ID로 변환합니다.
        /// </summary>
        private int GetSortingLayerIdFromIndex(int layerIndex)
        {
            if (layerIndex < 0 || layerIndex >= layerNames.Count)
            {
                return 0;
            }

            var layerName = layerNames[layerIndex];
            return new RenderingLayer(layerName).LayerID;
        }

        /// <summary>
        /// 타일 데이터를 RLE로 압축합니다. 레이어는 layerNames 컬렉션 인덱스로 저장합니다.
        /// </summary>
        private static void CompressTiles(List<TileLayerData> tiles, SerializedRoomData serialized)
        {
            if (tiles == null || tiles.Count == 0)
            {
                return;
            }

            serialized.TilesRLE.Clear();

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

            // RLE 압축 (레이어는 SortingLayer ID -> 레이어 이름 -> 컬렉션 인덱스로 변환)
            for (int i = 0; i < sortedTiles.Count; i++)
            {
                var current = sortedTiles[i];
                var layerName = RenderingLayers.GetLayerName(current.Layer);
                var layerIndex = serialized.GetOrAddLayerIndex(layerName);
                var startPos = current.Position;
                var length = 1;

                // 같은 레이어, 같은 Y, 같은 index인 연속된 타일 찾기
                while (i + length < sortedTiles.Count)
                {
                    var next = sortedTiles[i + length];
                    var nextLayerName = RenderingLayers.GetLayerName(next.Layer);
                    var nextLayerIndex = serialized.GetOrAddLayerIndex(nextLayerName);
                    if (nextLayerIndex != layerIndex ||
                        next.Index != current.Index ||
                        next.Position.y != current.Position.y ||
                        next.Position.x != startPos.x + length)
                    {
                        break;
                    }
                    length++;
                }

                // RLE 엔트리 생성 (Layer는 layerNames 인덱스)
                var rleData = new TileLayerDataRLE
                {
                    Index = current.Index,
                    Layer = layerIndex,
                    StartPosition = startPos,
                    Length = length
                };

                serialized.TilesRLE.Add(rleData);
                i += length - 1; // 다음 그룹으로 이동
            }
        }
    }
}

