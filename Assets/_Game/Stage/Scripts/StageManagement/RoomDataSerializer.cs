using System;
using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AddressableAssets;
#endif

namespace DungeonShooter
{
    /// <summary>
    /// RoomData의 직렬화/역직렬화를 담당하는 클래스
    /// </summary>
    public static class RoomDataSerializer
    {
        /// <summary>
        /// 에디터에서 배치한 게임 오브젝트를 RoomData로 직렬화합니다.
        /// </summary>
        public static RoomData SerializeRoom(GameObject room, int roomSizeX, int roomSizeY)
        {
            if (Application.isEditor == false)
            {
                LogHandler.LogError(nameof(RoomDataSerializer), "SerializeRoom은 에디터에서만 사용할 수 있습니다.");
                return null;
            }
            if (room == null)
            {
                LogHandler.LogError(nameof(RoomDataSerializer), "게임오브젝트가 null입니다.");
                return null;
            }

            var roomData = new RoomData();
            roomData.RoomSizeX = roomSizeX;
            roomData.RoomSizeY = roomSizeY;

            // 1. Tilemaps 하위의 타일맵 컴포넌트들을 찾아서 TileLayerData로 변환
            var tilemapsTransform = RoomCreateHelper.GetOrCreateTilemap(room.transform, RoomConstants.TILEMAP_DECO_NAME);
            if (tilemapsTransform != null)
            {
                SerializeTilemaps(tilemapsTransform, roomData);
            }
            else
            {
                LogHandler.LogWarning(nameof(RoomDataSerializer), $"'{room.name}'에 '{RoomConstants.TILEMAPS_GAMEOBJECT_NAME}' 자식이 없습니다.");
            }

            // 2. Objects 하위의 오브젝트들을 찾아서 ObjectData로 변환
            var objectsTransform = RoomCreateHelper.GetOrCreateChild(room.transform, RoomConstants.OBJECTS_GAMEOBJECT_NAME);
            if (objectsTransform != null)
            {
                SerializeObjects(objectsTransform, roomData);
            }
            else
            {
                LogHandler.LogWarning(nameof(RoomDataSerializer),$"'{room.name}'에 '{RoomConstants.OBJECTS_GAMEOBJECT_NAME}' 자식이 없습니다.");
            }

            return roomData;
        }

        /// <summary>
        /// 오브젝트의 어드레서블 주소를 가져옵니다.
        /// </summary>
        private static string GetAddressableAddress(Object obj)
        {
            if (obj == null)
            {
                return null;
            }

            // 1. 에셋 경로 얻기
            var assetPath = AssetDatabase.GetAssetPath(obj);
            if (string.IsNullOrEmpty(assetPath))
            {
                return null;
            }

            // 2. GUID 얻기
            var guid = AssetDatabase.AssetPathToGUID(assetPath);

            // 3. Addressables 설정에서 주소 찾기
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                LogHandler.LogWarning(nameof(RoomDataSerializer),"AddressableAssetSettings를 찾을 수 없습니다.");
                return null;
            }

            var entry = settings.FindAssetEntry(guid);
            if (entry != null)
            {
                return entry.address;
            }

            LogHandler.LogWarning(nameof(RoomDataSerializer),$"오브젝트 '{obj.name}'의 어드레서블 주소를 찾을 수 없습니다. 어드레서블로 등록되어 있는지 확인하세요.");
            return null;
        }

        /// <summary>
        /// 타일맵들을 직렬화합니다.
        /// </summary>
        private static void SerializeTilemaps(Tilemap tilemap, RoomData roomData)
        {
            // TilemapRenderer에서 SortingLayer ID 가져오기
            var renderer = tilemap.GetComponent<TilemapRenderer>();
            var sortingLayerId = renderer != null ? renderer.sortingLayerID : 0;

            // 타일맵의 모든 타일 순회
            var bounds = tilemap.cellBounds;
            foreach (Vector3Int pos in bounds.allPositionsWithin)
            {
                var tile = tilemap.GetTile(pos);
                if (tile == null)
                {
                    continue;
                }

                // TileBase의 어드레서블 주소 얻기
                var address = GetAddressableAddress(tile);
                if (string.IsNullOrEmpty(address))
                {
                    LogHandler.LogWarning(nameof(RoomDataSerializer),$"타일 '{tile.name}'의 어드레서블 주소를 찾을 수 없습니다. 위치: {pos}");
                    continue;
                }

                // 주소 테이블에 추가하고 인덱스 얻기
                var addressIndex = roomData.GetOrAddAddress(address);

                // TileLayerData 생성
                var tileData = new TileLayerData
                {
                    Index = addressIndex,
                    Layer = sortingLayerId,
                    Position = new Vector2Int(pos.x, pos.y)
                };

                roomData.Tiles.Add(tileData);
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
                var child = objectsParent.GetChild(i);
                var obj = child.gameObject;

                // 프리팹 인스턴스인지 확인
                var prefabType = PrefabUtility.GetPrefabAssetType(obj);
                if (prefabType == PrefabAssetType.NotAPrefab)
                {
                    // 프리팹이 아니면 프리팹 루트를 찾기
                    var prefabRoot = PrefabUtility.GetCorrespondingObjectFromSource(obj);
                    if (prefabRoot == null)
                    {
                        LogHandler.LogWarning(nameof(RoomDataSerializer),$"오브젝트 '{obj.name}'는 프리팹이 아닙니다. 어드레서블로 등록된 프리팹을 사용해야 합니다.");
                        continue;
                    }

                    // 프리팹의 어드레서블 주소 얻기
                    var address = GetAddressableAddress(prefabRoot);
                    if (string.IsNullOrEmpty(address))
                    {
                        LogHandler.LogWarning(nameof(RoomDataSerializer),$"프리팹 '{prefabRoot.name}'의 어드레서블 주소를 찾을 수 없습니다.");
                        continue;
                    }

                    // 주소 테이블에 추가하고 인덱스 얻기
                    var addressIndex = roomData.GetOrAddAddress(address);

                    // ObjectData 생성
                    var position = child.position;
                    var objectData = new ObjectData
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
                    var prefabAsset = PrefabUtility.GetCorrespondingObjectFromSource(obj);
                    if (prefabAsset == null)
                    {
                        continue;
                    }

                    var address = GetAddressableAddress(prefabAsset);
                    if (string.IsNullOrEmpty(address))
                    {
                        LogHandler.LogWarning(nameof(RoomDataSerializer),$"프리팹 '{prefabAsset.name}'의 어드레서블 주소를 찾을 수 없습니다.");
                        continue;
                    }

                    var addressIndex = roomData.GetOrAddAddress(address);

                    var position = child.position;
                    var objectData = new ObjectData
                    {
                        Index = addressIndex,
                        Position = new Vector2(position.x, position.y),
                        Rotation = child.rotation
                    };

                    roomData.Objects.Add(objectData);
                }
            }
        }

        /// <summary>
        /// TextAsset에서 RoomData를 역직렬화합니다.
        /// </summary>
        public static RoomData DeserializeRoom(TextAsset textAsset)
        {
            if (textAsset == null)
            {
                LogHandler.LogError(nameof(RoomDataSerializer),"TextAsset이 null입니다.");
                return null;
            }

            try
            {
                var json = textAsset.text;
                var serialized = JsonUtility.FromJson<SerializedRoomData>(json);
                
                if (serialized == null)
                {
                    LogHandler.LogError(nameof(RoomDataSerializer),"역직렬화된 데이터가 null입니다.");
                    return null;
                }
                
                // RoomData로 변환 (RLE 압축 해제)
                return serialized.ToRoomData();
            }
            catch (Exception e)
            {
                LogHandler.LogError(nameof(RoomDataSerializer), e, "역직렬화 실패");
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
                LogHandler.LogError(nameof(RoomDataSerializer),"roomData가 null입니다.");
                return;
            }

            try
            {
                // RoomData를 RoomDataSerialized로 변환 (RLE 압축)
                var serialized = SerializedRoomData.FromRoomData(roomData);
                if (serialized == null)
                {
                    LogHandler.LogError(nameof(RoomDataSerializer),"직렬화 변환에 실패했습니다.");
                    return;
                }

                // JSON으로 직렬화 (pretty print 제거로 용량 절약)
                var json = JsonUtility.ToJson(serialized, false);
                var directory = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllText(path, json);
                LogHandler.Log(nameof(RoomDataSerializer),$"저장 완료: {path}");
            }
            catch (Exception e)
            {
                LogHandler.LogError(nameof(RoomDataSerializer), e, "저장 실패");
            }
        }

    }
}

