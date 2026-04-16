using UnityEngine;

namespace DungeonShooter
{
    public enum InputActionTypes
    {
        Move,
        WeaponAttack,
        Skill1,
        Skill2,
        Dash,
        Interact,
    }
    
    /// <summary> 4 방향을 나타내는 열거형 </summary>
    public enum Direction
    {
        Up,
        Down,
        Right,
        Left
    }
    
    /// <summary> Sorting Order 정의 /// </summary>
    public enum BaseSortingOrder
    {
        SceneUI = 1000,
        GlobalUI = 2000,
    }
    
    /// <summary> UI 종류. 타입별로 캔버스가 생성되며, 열거 순서가 캔버스 정렬 순서가 된다. /// </summary>
    public enum UIType
    {
        HudUI,
        PopupUI,
    }
    
    /// <summary> 로딩 UI 종류. 묵직한 로딩(윈도우) / 가벼운 로딩(스피너) 구분 </summary>
    public enum LoadingType
    {
        LoadingWindow,
        LoadingSpinner
    }
    
    public enum SkillOwner
    {
        Caster = 0,
        LastHitTarget = 1
    }

    /// <summary> 추적 카메라 종류 </summary>
    public enum CameraTrackType
    {
        PlayerChaseCamera
    }
}

