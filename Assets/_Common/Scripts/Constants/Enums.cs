using UnityEngine;

namespace DungeonShooter
{
    /// <summary>
    /// 4 방향을 나타내는 열거형
    /// </summary>
    public enum Direction
    {
        Up,
        Down,
        Right,
        Left
    }
    
    /// <summary>
    /// 방의 타입을 나타내는 열거형
    /// </summary>
    public enum RoomType
    {
        Start,
        Normal,
        Boss,
    }
}

