using Cysharp.Threading.Tasks;

namespace DungeonShooter
{
    public interface IUIManager
    {
        public int SortingOrder { get; }
        public UniTask<T> GetSingletonUIAsync<T>(string addressableKey, bool active = true) where T : UIBase;
        public UniTask<T> CreateUIAsync<T>(string addressableKey, bool active = true) where T : UIBase;
        public T GetSingletonUISync<T>(string addressableKey, bool active = true) where T : UIBase;
        public T CreateUISync<T>(string addressableKey, bool active = true) where T : UIBase;
        public bool RemoveUI(UIBase uiBase);
        public int GetOrder(UIBase uiBase);
        public void SetOrder(UIBase uiBase, int order);
    }

    public interface IGlobalUIManager : IUIManager { }
    public interface ISceneUIManager : IUIManager { }
}
