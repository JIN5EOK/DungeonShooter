namespace DungeonShooter
{
    /// <summary>
    /// 스크린 영역에 고정 표시되는 정보용 UI 베이스.
    /// </summary>
    public abstract class HudUI : UIBase
    {
        public override UIType Type => UIType.HudUI;
    }
}
