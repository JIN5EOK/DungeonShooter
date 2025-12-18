using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace DungeonShooter
{
    /// <summary>
    /// 직렬화 가능한 타일맵 데이터 구조
    /// </summary>
    [Serializable]
    public class MapData
    {
        public string MapName;
        
        /// <summary>
        /// Grid 설정 정보 (cellSize, cellLayout, cellGap은 Grid 레벨에서 관리됨)
        /// </summary>
        public GridData GridData = new GridData();
        
        public List<TilemapLayerData> Layers = new List<TilemapLayerData>();
        
        /// <summary>
        /// 나중에 확장을 위한 오브젝트 데이터 리스트 (현재는 비어있음)
        /// </summary>
        public List<MapObjectData> Objects = new List<MapObjectData>();
    }

    /// <summary>
    /// Grid 설정 데이터
    /// </summary>
    [Serializable]
    public class GridData
    {
        public Vector3 CellSize = Vector3.one;
        public int CellLayout; // GridLayout.CellLayout enum을 int로 저장
        public Vector3 CellGap = Vector3.zero;
    }

    /// <summary>
    /// 타일맵 레이어 데이터
    /// </summary>
    [Serializable]
    public class TilemapLayerData
    {
        public string LayerName;
        public Vector3Int Origin;
        public List<TileData> Tiles = new List<TileData>();
    }

    /// <summary>
    /// 개별 타일 데이터
    /// </summary>
    [Serializable]
    public class TileData
    {
        public Vector3Int Position;
        public string TileAssetGuid; // TileBase의 GUID
        public string TileAssetName; // TileBase의 이름 (백업용)
    }

    /// <summary>
    /// 맵에 배치된 오브젝트 데이터 (보물상자, 적 등)
    /// 주의: Unity JsonUtility는 Dictionary를 지원하지 않으므로, 
    /// CustomData는 나중에 필요시 SerializableDictionary나 별도 직렬화 로직이 필요합니다.
    /// </summary>
    [Serializable]
    public class MapObjectData
    {
        public string ObjectType; // "TreasureChest", "Enemy" 등
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Scale = Vector3.one;
        public string PrefabGuid; // 프리팹의 GUID
        // Dictionary는 JsonUtility로 직렬화되지 않으므로, 나중에 필요시 수정 필요
        // public Dictionary<string, string> CustomData = new Dictionary<string, string>();
    }
}

