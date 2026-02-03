using System;
using UnityEngine;

namespace DungeonShooter
{
    /// <summary>
    /// 오브젝트 배치 데이터
    /// </summary>
    [Serializable]
    public class ObjectData
    {
        [SerializeField] private int tableId; // 테이블 엔트리 ID
        [SerializeField] private Vector2 position; // 방 생성시 배치될 위치
        [SerializeField] private Quaternion rotation; // 방 생성시 배치될 회전값

        public int TableId
        {
            get => tableId;
            set => tableId = value;
        }

        public Vector2 Position
        {
            get => position;
            set => position = value;
        }

        public Quaternion Rotation
        {
            get => rotation;
            set => rotation = value;
        }
    }
}

