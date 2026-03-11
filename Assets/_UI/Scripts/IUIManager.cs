using Cysharp.Threading.Tasks;

namespace DungeonShooter
{
    public interface IUIManager
    {
        public int SortingOrder { get; }
        UniTask<T> GetSingletonUIAsync<T>(string addressableKey, bool active = true) where T : UIBase;
        UniTask<T> CreateUIAsync<T>(string addressableKey, bool active = true) where T : UIBase;
        T GetSingletonUISync<T>(string addressableKey, bool active = true) where T : UIBase;
        T CreateUISync<T>(string addressableKey, bool active = true) where T : UIBase;
        bool RemoveUI(UIBase uiBase);
        int GetOrder(UIBase uiBase);
        void SetOrder(UIBase uiBase, int order);
    }
}
