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

            var tilemapsParent = room.transform.Find(RoomConstants.TILEMAPS_GAMEOBJECT_NAME);
            var decoTilemap = tilemapsParent?.Find(RoomConstants.TILEMAP_DECO_NAME)?.GetComponent<Tilemap>();
            if (decoTilemap != null)
            {
                SerializeTilemaps(decoTilemap, roomData);
            }
            else
            {
                LogHandler.LogWarning(nameof(RoomDataSerializer), $"'{room.name}'에 '{RoomConstants.TILEMAPS_GAMEOBJECT_NAME}' 자식이 없습니다.");
            }

            var objectsTransform = room.transform.Find(RoomConstants.OBJECTS_GAMEOBJECT_NAME);
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
        /// RoomObjectMarker(테이블 ID)가 있는 오브젝트만 저장합니다.
        /// </summary>
        private static void SerializeObjects(Transform objectsParent, RoomData roomData)
        {
            for (int i = 0; i < objectsParent.childCount; i++)
            {
                var child = objectsParent.GetChild(i);
                var obj = child.gameObject;
                if (!obj.TryGetComponent(out RoomObjectMarker marker) || marker.TableId == 0)
                {
                    LogHandler.LogWarning(nameof(RoomDataSerializer), $"오브젝트 '{obj.name}'는 RoomObjectMarker(테이블 ID)가 없어 저장되지 않습니다. 인스펙터에서 배치할 ID를 선택 후 Ctrl+클릭으로 배치하세요.");
                    continue;
                }

                var position = child.position;
                var rotation = child.rotation;
                var position2 = new Vector2(position.x, position.y);
                roomData.Objects.Add(new ObjectData
                {
                    TableId = marker.TableId,
                    Position = position2,
                    Rotation = rotation
                });
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
                LogHandler.LogException(nameof(RoomDataSerializer), e, "역직렬화 실패");
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

                var json = JsonUtility.ToJson(serialized, true);
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
                LogHandler.LogException(nameof(RoomDataSerializer), e, "저장 실패");
            }
        }

    }
}

