namespace DungeonShooter
{
    /// <summary>
    /// 일반 UI. 버튼 등 상호작용이 가능한 팝업용 베이스.
    /// </summary>
    public abstract class PopupUI : UIBase
    {
        public override UIType Type => UIType.PopupUI;
    }
}
