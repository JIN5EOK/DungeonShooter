using System.IO;
using UnityEngine;

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
        public static RoomData SerializeRoom(GameObject room)
        {
            if (room == null)
            {
                Debug.LogError("[RoomDataSerializer] room이 null입니다.");
                return null;
            }

            RoomData roomData = new RoomData();

            // TODO: 타일맵과 오브젝트를 읽어서 RoomData에 저장
            // 1. 타일맵 컴포넌트들을 찾아서 TileLayerData로 변환
            // 2. 오브젝트들을 찾아서 ObjectData로 변환
            // 3. assetAddresses 테이블 구성

            Debug.LogWarning("[RoomDataSerializer] SerializeRoom은 아직 구현되지 않았습니다.");
            return roomData;
        }

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

