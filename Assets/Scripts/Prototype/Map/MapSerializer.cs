using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DungeonShooter
{
    /// <summary>
    /// 타일맵 직렬화/역직렬화 유틸리티
    /// </summary>
    public static class MapSerializer
    {

        /// <summary>
        /// 타일맵을 MapData로 직렬화합니다.
        /// </summary>
        public static MapData SerializeTilemap(Grid grid, string mapName = "NewMap")
        {
            if (grid == null)
            {
                Debug.LogError("[MapSerializer] Grid가 null입니다.");
                return null;
            }

            MapData mapData = new MapData
            {
                MapName = mapName,
                GridData = new GridData
                {
                    CellSize = grid.cellSize,
                    CellLayout = (int)grid.cellLayout, // enum을 int로 변환
                    CellGap = grid.cellGap
                }
            };

            // Grid의 모든 Tilemap 컴포넌트를 가져옵니다
            Tilemap[] tilemaps = grid.GetComponentsInChildren<Tilemap>();

            foreach (Tilemap tilemap in tilemaps)
            {
                if (!tilemap.gameObject.activeInHierarchy)
                    continue;

                TilemapLayerData layerData = SerializeTilemapLayer(tilemap);
                if (layerData != null)
                {
                    mapData.Layers.Add(layerData);
                }
            }

            return mapData;
        }

        /// <summary>
        /// 단일 타일맵 레이어를 직렬화합니다.
        /// </summary>
        private static TilemapLayerData SerializeTilemapLayer(Tilemap tilemap)
        {
            TilemapLayerData layerData = new TilemapLayerData
            {
                LayerName = tilemap.name,
                Origin = tilemap.origin
            };

            // 타일맵의 모든 타일을 순회합니다
            BoundsInt bounds = tilemap.cellBounds;
            
            for (int x = bounds.xMin; x < bounds.xMax; x++)
            {
                for (int y = bounds.yMin; y < bounds.yMax; y++)
                {
                    for (int z = bounds.zMin; z < bounds.zMax; z++)
                    {
                        Vector3Int position = new Vector3Int(x, y, z);
                        TileBase tile = tilemap.GetTile(position);

                        if (tile != null)
                        {
                            TileData tileData = new TileData
                            {
                                Position = position
                            };

                            // TileBase의 GUID와 이름을 저장
#if UNITY_EDITOR
                            string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(tile));
                            tileData.TileAssetGuid = guid;
                            tileData.TileAssetName = tile.name;
#else
                            tileData.TileAssetName = tile.name;
#endif

                            layerData.Tiles.Add(tileData);
                        }
                    }
                }
            }

            return layerData;
        }

        /// <summary>
        /// MapData를 JSON 문자열로 직렬화합니다.
        /// </summary>
        public static string ToJson(MapData mapData)
        {
            if (mapData == null)
            {
                Debug.LogError("[MapSerializer] MapData가 null입니다.");
                return null;
            }

            try
            {
                return JsonUtility.ToJson(mapData, true);
            }
            catch (Exception e)
            {
                Debug.LogError($"[MapSerializer] JSON 직렬화 실패: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// JSON 문자열에서 MapData를 역직렬화합니다.
        /// </summary>
        public static MapData FromJson(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                Debug.LogError("[MapSerializer] JSON 문자열이 비어있습니다.");
                return null;
            }

            try
            {
                return JsonUtility.FromJson<MapData>(json);
            }
            catch (Exception e)
            {
                Debug.LogError($"[MapSerializer] JSON 역직렬화 실패: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// MapData를 파일로 저장합니다.
        /// </summary>
        public static bool SaveToFile(MapData mapData, string filePath)
        {
            if (mapData == null)
            {
                Debug.LogError("[MapSerializer] MapData가 null입니다.");
                return false;
            }

            try
            {
                string json = ToJson(mapData);
                if (string.IsNullOrEmpty(json))
                {
                    return false;
                }

                // 디렉토리가 없으면 생성
                string directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllText(filePath, json);
                Debug.Log($"[MapSerializer] 맵 저장 완료: {filePath}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[MapSerializer] 파일 저장 실패: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// 파일에서 MapData를 불러옵니다.
        /// </summary>
        public static MapData LoadFromFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Debug.LogError($"[MapSerializer] 파일이 존재하지 않습니다: {filePath}");
                return null;
            }

            try
            {
                string json = File.ReadAllText(filePath);
                MapData mapData = FromJson(json);
                
                if (mapData != null)
                {
                    Debug.Log($"[MapSerializer] 맵 불러오기 완료: {filePath}");
                }
                
                return mapData;
            }
            catch (Exception e)
            {
                Debug.LogError($"[MapSerializer] 파일 불러오기 실패: {e.Message}");
                return null;
            }
        }


        /// <summary>
        /// MapData를 타일맵으로 복원합니다.
        /// </summary>
        public static bool DeserializeToTilemap(MapData mapData, Grid grid)
        {
            if (mapData == null)
            {
                Debug.LogError("[MapSerializer] MapData가 null입니다.");
                return false;
            }

            if (grid == null)
            {
                Debug.LogError("[MapSerializer] Grid가 null입니다.");
                return false;
            }

            // Grid 설정 복원 (에디터에서만 가능)
#if UNITY_EDITOR
            if (mapData.GridData != null)
            {
                // Grid의 속성은 읽기 전용이므로 SerializedObject를 사용하여 설정
                SerializedObject serializedGrid = new SerializedObject(grid);
                serializedGrid.FindProperty("m_CellSize").vector3Value = mapData.GridData.CellSize;
                serializedGrid.FindProperty("m_CellLayout").enumValueIndex = mapData.GridData.CellLayout;
                serializedGrid.FindProperty("m_CellGap").vector3Value = mapData.GridData.CellGap;
                serializedGrid.ApplyModifiedProperties();
            }
#else
            // 런타임에서는 Grid 설정을 변경할 수 없으므로 경고만 출력
            if (mapData.GridData != null)
            {
                Debug.LogWarning("[MapSerializer] 런타임에서는 Grid 설정을 변경할 수 없습니다. Grid는 이미 설정된 값을 사용합니다.");
            }
#endif

            foreach (TilemapLayerData layerData in mapData.Layers)
            {
                DeserializeTilemapLayer(layerData, grid);
            }

            return true;
        }

        /// <summary>
        /// 단일 타일맵 레이어를 복원합니다.
        /// </summary>
        private static void DeserializeTilemapLayer(TilemapLayerData layerData, Grid grid)
        {
            // 해당 이름의 타일맵을 찾거나 생성
            Tilemap tilemap = null;
            Transform layerTransform = grid.transform.Find(layerData.LayerName);
            
            if (layerTransform != null)
            {
                tilemap = layerTransform.GetComponent<Tilemap>();
            }

            if (tilemap == null)
            {
                GameObject tilemapObject = new GameObject(layerData.LayerName);
                tilemapObject.transform.SetParent(grid.transform);
                tilemap = tilemapObject.AddComponent<Tilemap>();
                tilemapObject.AddComponent<TilemapRenderer>();
            }

            // 타일맵 설정 복원 (origin만 설정 가능)
            tilemap.origin = layerData.Origin;

            // 타일 복원
            foreach (TileData tileData in layerData.Tiles)
            {
                TileBase tile = LoadTileAsset(tileData);
                if (tile != null)
                {
                    tilemap.SetTile(tileData.Position, tile);
                }
            }

            tilemap.RefreshAllTiles();
        }

        /// <summary>
        /// TileData에서 TileBase를 로드합니다.
        /// </summary>
        private static TileBase LoadTileAsset(TileData tileData)
        {
#if UNITY_EDITOR
            // GUID를 사용하여 에셋 로드
            if (!string.IsNullOrEmpty(tileData.TileAssetGuid))
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(tileData.TileAssetGuid);
                if (!string.IsNullOrEmpty(assetPath))
                {
                    TileBase tile = AssetDatabase.LoadAssetAtPath<TileBase>(assetPath);
                    if (tile != null)
                    {
                        return tile;
                    }
                }
            }
#endif

            // GUID로 찾지 못한 경우 이름으로 검색 (런타임에서도 작동)
            if (!string.IsNullOrEmpty(tileData.TileAssetName))
            {
                // Resources 폴더에서 로드 시도
                TileBase tile = Resources.Load<TileBase>(tileData.TileAssetName);
                if (tile != null)
                {
                    return tile;
                }

#if UNITY_EDITOR
                // 에디터에서는 AssetDatabase로 검색
                string[] guids = AssetDatabase.FindAssets($"{tileData.TileAssetName} t:TileBase");
                if (guids.Length > 0)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                    return AssetDatabase.LoadAssetAtPath<TileBase>(assetPath);
                }
#endif
            }

            Debug.LogWarning($"[MapSerializer] 타일을 찾을 수 없습니다: {tileData.TileAssetName} (GUID: {tileData.TileAssetGuid})");
            return null;
        }
    }

}

