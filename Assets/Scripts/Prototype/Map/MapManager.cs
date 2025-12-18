using System;
using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace DungeonShooter
{
    /// <summary>
    /// 맵 저장/불러오기를 관리하는 매니저
    /// </summary>
    public class MapManager : MonoBehaviour
    {
        [Header("맵 설정")]
        [SerializeField] private Grid _grid;
        [SerializeField] private string _defaultMapName = "DefaultMap";
        
        [Header("저장 경로 설정")]
        [SerializeField] private string _saveDirectory = "Maps";

        private string SaveDirectoryPath => Path.Combine(Application.dataPath, "..", _saveDirectory);

        private void Awake()
        {
            if (_grid == null)
            {
                _grid = FindFirstObjectByType<Grid>();
            }
        }
        
        /// <summary>
        /// 현재 타일맵을 저장합니다.
        /// </summary>
        public bool SaveMap(string mapName = null)
        {
            if (_grid == null)
            {
                Debug.LogError("[MapManager] Grid가 설정되지 않았습니다.");
                return false;
            }

            string name = mapName ?? _defaultMapName;
            MapData mapData = MapSerializer.SerializeTilemap(_grid, name);

            if (mapData == null)
            {
                Debug.LogError("[MapManager] 맵 직렬화에 실패했습니다.");
                return false;
            }

            string filePath = GetMapFilePath(name);
            bool success = MapSerializer.SaveToFile(mapData, filePath);

            if (success)
            {
                Debug.Log($"[MapManager] 맵 저장 완료: {name} -> {filePath}");
            }

            return success;
        }

        /// <summary>
        /// 맵을 불러옵니다.
        /// </summary>
        public bool LoadMap(string mapName)
        {
            if (_grid == null)
            {
                Debug.LogError("[MapManager] Grid가 설정되지 않았습니다.");
                return false;
            }

            string filePath = GetMapFilePath(mapName);
            MapData mapData = MapSerializer.LoadFromFile(filePath);

            if (mapData == null)
            {
                Debug.LogError($"[MapManager] 맵 불러오기 실패: {mapName}");
                return false;
            }

            bool success = MapSerializer.DeserializeToTilemap(mapData, _grid);

            if (success)
            {
                Debug.Log($"[MapManager] 맵 불러오기 완료: {mapName}");
            }

            return success;
        }

        /// <summary>
        /// 맵 파일 경로를 반환합니다.
        /// </summary>
        private string GetMapFilePath(string mapName)
        {
            string fileName = $"{mapName}.json";
            return Path.Combine(SaveDirectoryPath, fileName);
        }

        /// <summary>
        /// 저장된 맵 목록을 가져옵니다.
        /// </summary>
        public string[] GetSavedMapNames()
        {
            if (!Directory.Exists(SaveDirectoryPath))
            {
                return new string[0];
            }

            string[] files = Directory.GetFiles(SaveDirectoryPath, "*.json");
            string[] mapNames = new string[files.Length];

            for (int i = 0; i < files.Length; i++)
            {
                mapNames[i] = Path.GetFileNameWithoutExtension(files[i]);
            }

            return mapNames;
        }

        /// <summary>
        /// 맵 파일이 존재하는지 확인합니다.
        /// </summary>
        public bool MapExists(string mapName)
        {
            string filePath = GetMapFilePath(mapName);
            return File.Exists(filePath);
        }

        /// <summary>
        /// 맵 파일을 삭제합니다.
        /// </summary>
        public bool DeleteMap(string mapName)
        {
            string filePath = GetMapFilePath(mapName);

            if (!File.Exists(filePath))
            {
                Debug.LogWarning($"[MapManager] 맵 파일이 존재하지 않습니다: {mapName}");
                return false;
            }

            try
            {
                File.Delete(filePath);
                Debug.Log($"[MapManager] 맵 삭제 완료: {mapName}");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[MapManager] 맵 삭제 실패: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// 현재 맵을 MapData로 가져옵니다 (직렬화하지 않고 메모리에서만).
        /// </summary>
        public MapData GetCurrentMapData(string mapName = null)
        {
            if (_grid == null)
            {
                Debug.LogError("[MapManager] Grid가 설정되지 않았습니다.");
                return null;
            }

            string name = mapName ?? _defaultMapName;
            return MapSerializer.SerializeTilemap(_grid, name);
        }

        /// <summary>
        /// MapData를 현재 타일맵에 적용합니다.
        /// </summary>
        public bool ApplyMapData(MapData mapData)
        {
            if (_grid == null)
            {
                Debug.LogError("[MapManager] Grid가 설정되지 않았습니다.");
                return false;
            }

            if (mapData == null)
            {
                Debug.LogError("[MapManager] MapData가 null입니다.");
                return false;
            }

            return MapSerializer.DeserializeToTilemap(mapData, _grid);
        }
    }
}


