using UnityEngine;

namespace DungeonShooter
{
    /// <summary> 4 방향을 나타내는 열거형 </summary>
    public enum Direction
    {
        Up,
        Down,
        Right,
        Left
    }
    
    /// <summary> 방의 타입을 나타내는 열거형 </summary>
    public enum RoomType
    {
        Start,
        Normal,
        Boss,
    }
    
    /// <summary> 아이템 타입 </summary>
    public enum ItemType
    {
        Weapon,
        Passive,
        Consume
    }
    
    /// <summary> UI 종류. 타입별로 캔버스가 생성되며, 열거 순서가 캔버스 정렬 순서가 된다. /// </summary>
    public enum UIType
    {
        HudUI,
        PopupUI
    }
    
    public enum SkillOwner
    {
        Caster = 0,
        LastHitTarget = 1
    }
}

