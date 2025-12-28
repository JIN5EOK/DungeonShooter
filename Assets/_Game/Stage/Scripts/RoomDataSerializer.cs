using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AddressableAssets;
#endif

// TODO: RLE 압축 알고리즘 적용 필요
namespace DungeonShooter
{
    /// <summary>
    /// RoomData의 직렬화/역직렬화를 담당하는 클래스
    /// </summary>
    public static class RoomDataSerializer
    {
#if UNITY_EDITOR
        /// <summary>
        /// 오브젝트의 어드레서블 주소를 가져옵니다. (에디터 전용)
        /// </summary>
        public static string GetAddressableAddress(Object obj)
        {
            if (obj == null)
            {
                return null;
            }

            // 1. 에셋 경로 얻기
            string assetPath = AssetDatabase.GetAssetPath(obj);
            if (string.IsNullOrEmpty(assetPath))
            {
                return null;
            }

            // 2. GUID 얻기
            string guid = AssetDatabase.AssetPathToGUID(assetPath);

            // 3. Addressables 설정에서 주소 찾기
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                Debug.LogWarning("[RoomDataSerializer] AddressableAssetSettings를 찾을 수 없습니다.");
                return null;
            }

            var entry = settings.FindAssetEntry(guid);
            if (entry != null)
            {
                return entry.address;
            }

            Debug.LogWarning($"[RoomDataSerializer] 오브젝트 '{obj.name}'의 어드레서블 주소를 찾을 수 없습니다. 어드레서블로 등록되어 있는지 확인하세요.");
            return null;
        }
#endif
        /// <summary>
        /// 에디터에서 배치한 게임 오브젝트를 RoomData로 직렬화합니다.
        /// </summary>
        public static RoomData SerializeRoom(GameObject room)
        {
            if (room == null)
            {
                Debug.LogError("[RoomDataSerializer] room이 null입니다.");
                return null;
            }

            RoomData roomData = new RoomData();

#if UNITY_EDITOR
            // 1. Tilemaps 하위의 타일맵 컴포넌트들을 찾아서 TileLayerData로 변환
            Transform tilemapsTransform = room.transform.Find("Tilemaps");
            if (tilemapsTransform != null)
            {
                SerializeTilemaps(tilemapsTransform, roomData);
            }
            else
            {
                Debug.LogWarning($"[RoomDataSerializer] '{room.name}'에 'Tilemaps' 자식이 없습니다.");
            }

            // 2. Objects 하위의 오브젝트들을 찾아서 ObjectData로 변환
            Transform objectsTransform = room.transform.Find("Objects");
            if (objectsTransform != null)
            {
                SerializeObjects(objectsTransform, roomData);
            }
            else
            {
                Debug.LogWarning($"[RoomDataSerializer] '{room.name}'에 'Objects' 자식이 없습니다.");
            }
#else
            Debug.LogError("[RoomDataSerializer] SerializeRoom은 에디터에서만 사용할 수 있습니다.");
#endif

            return roomData;
        }

#if UNITY_EDITOR
        /// <summary>
        /// 타일맵들을 직렬화합니다.
        /// </summary>
        private static void SerializeTilemaps(Transform tilemapsParent, RoomData roomData)
        {
            // Tilemaps 하위의 모든 Tilemap 컴포넌트 찾기
            Tilemap[] tilemaps = tilemapsParent.GetComponentsInChildren<Tilemap>();

            foreach (Tilemap tilemap in tilemaps)
            {
                // TilemapRenderer에서 SortingLayer ID 가져오기
                TilemapRenderer renderer = tilemap.GetComponent<TilemapRenderer>();
                int sortingLayerId = renderer != null ? renderer.sortingLayerID : 0;

                // 타일맵의 모든 타일 순회
                BoundsInt bounds = tilemap.cellBounds;
                foreach (Vector3Int pos in bounds.allPositionsWithin)
                {
                    TileBase tile = tilemap.GetTile(pos);
                    if (tile == null)
                    {
                        continue;
                    }

                    // TileBase의 어드레서블 주소 얻기
                    string address = GetAddressableAddress(tile);
                    if (string.IsNullOrEmpty(address))
                    {
                        Debug.LogWarning($"[RoomDataSerializer] 타일 '{tile.name}'의 어드레서블 주소를 찾을 수 없습니다. 위치: {pos}");
                        continue;
                    }

                    // 주소 테이블에 추가하고 인덱스 얻기
                    int addressIndex = roomData.GetOrAddAddress(address);

                    // TileLayerData 생성
                    TileLayerData tileData = new TileLayerData
                    {
                        Index = addressIndex,
                        Layer = sortingLayerId,
                        Position = new Vector2Int(pos.x, pos.y)
                    };

                    roomData.Tiles.Add(tileData);
                }
            }
        }

        /// <summary>
        /// 오브젝트들을 직렬화합니다.
        /// </summary>
        private static void SerializeObjects(Transform objectsParent, RoomData roomData)
        {
            // Objects 하위의 모든 자식 오브젝트 순회
            for (int i = 0; i < objectsParent.childCount; i++)
            {
                Transform child = objectsParent.GetChild(i);
                GameObject obj = child.gameObject;

                // 프리팹 인스턴스인지 확인
#if UNITY_EDITOR
                PrefabAssetType prefabType = PrefabUtility.GetPrefabAssetType(obj);
                if (prefabType == PrefabAssetType.NotAPrefab)
                {
                    // 프리팹이 아니면 프리팹 루트를 찾기
                    GameObject prefabRoot = PrefabUtility.GetCorrespondingObjectFromSource(obj);
                    if (prefabRoot == null)
                    {
                        Debug.LogWarning($"[RoomDataSerializer] 오브젝트 '{obj.name}'는 프리팹이 아닙니다. 어드레서블로 등록된 프리팹을 사용해야 합니다.");
                        continue;
                    }

                    // 프리팹의 어드레서블 주소 얻기
                    string address = GetAddressableAddress(prefabRoot);
                    if (string.IsNullOrEmpty(address))
                    {
                        Debug.LogWarning($"[RoomDataSerializer] 프리팹 '{prefabRoot.name}'의 어드레서블 주소를 찾을 수 없습니다.");
                        continue;
                    }

                    // 주소 테이블에 추가하고 인덱스 얻기
                    int addressIndex = roomData.GetOrAddAddress(address);

                    // ObjectData 생성
                    Vector3 position = child.position;
                    ObjectData objectData = new ObjectData
                    {
                        Index = addressIndex,
                        Position = new Vector2(position.x, position.y),
                        Rotation = child.rotation
                    };

                    roomData.Objects.Add(objectData);
                }
                else
                {
                    // 프리팹 인스턴스인 경우
                    GameObject prefabAsset = PrefabUtility.GetCorrespondingObjectFromSource(obj);
                    if (prefabAsset == null)
                    {
                        continue;
                    }

                    string address = GetAddressableAddress(prefabAsset);
                    if (string.IsNullOrEmpty(address))
                    {
                        Debug.LogWarning($"[RoomDataSerializer] 프리팹 '{prefabAsset.name}'의 어드레서블 주소를 찾을 수 없습니다.");
                        continue;
                    }

                    int addressIndex = roomData.GetOrAddAddress(address);

                    Vector3 position = child.position;
                    ObjectData objectData = new ObjectData
                    {
                        Index = addressIndex,
                        Position = new Vector2(position.x, position.y),
                        Rotation = child.rotation
                    };

                    roomData.Objects.Add(objectData);
                }
#endif
            }
        }
#endif

        /// <summary>
        /// 파일에서 RoomData를 역직렬화합니다.
        /// </summary>
        public static RoomData DeserializeRoom(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                Debug.LogError($"[RoomDataSerializer] 파일을 찾을 수 없습니다: {path}");
                return null;
            }

            try
            {
                string json = File.ReadAllText(path);
                RoomData roomData = JsonUtility.FromJson<RoomData>(json);
                return roomData;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[RoomDataSerializer] 역직렬화 실패: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// RoomData를 파일로 저장합니다.
        /// </summary>
        public static void SaveToFile(RoomData roomData, string path)
        {
            if (roomData == null)
            {
                Debug.LogError("[RoomDataSerializer] roomData가 null입니다.");
                return;
            }

            try
            {
                string json = JsonUtility.ToJson(roomData, true);
                string directory = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllText(path, json);
                Debug.Log($"[RoomDataSerializer] 저장 완료: {path}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[RoomDataSerializer] 저장 실패: {e.Message}");
            }
        }
    }
}

