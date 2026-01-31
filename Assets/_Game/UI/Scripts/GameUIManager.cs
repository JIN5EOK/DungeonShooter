using System;
using Cysharp.Threading.Tasks;
using VContainer;

namespace DungeonShooter
{
    public class StageUIManager : IDisposable
    {
        private UIManager _uiManager;
        private readonly UICache<HealthBarHudUI> _healthBarCache = new();
        private readonly UICache<SkillCooldownHudUI> _skillCooldownHudCache = new();

        [Inject]
        public void Construct(UIManager uiManager)
        {
            _uiManager = uiManager;
        }

        public async UniTask<HealthBarHudUI> GetHealthBarUI() =>
            await _healthBarCache.GetOrCreateAsync(_uiManager, "UI_HpHud");

        public async UniTask<SkillCooldownHudUI> GetSkillCooldownHudUI() =>
            await _skillCooldownHudCache.GetOrCreateAsync(_uiManager, "UI_SkillCooldownHud");

        private class UICache<T> : IDisposable where T : UIBase
        {
            private T _cached;
            private UniTask<T>? _task;
            private UIManager _uiManager;
            public async UniTask<T> GetOrCreateAsync(UIManager uiManager, string addressableKey)
            {
                if (_cached != null)
                    return _cached;
                if (_task.HasValue)
                    return await _task.Value;

                _uiManager = uiManager;
                _task = uiManager.CreateUIAsync<T>(addressableKey);
                _cached = await _task.Value;
                _task = null;
                return _cached;
            }

            public void Dispose()
            {
                if (_cached != null)
                {
                    _uiManager?.RemoveUI(_cached);
                }
                // 일단은 생성된 UI만 파괴하도록 함, 나중에 취소토큰 등으로 캐시를 취소할 수 있도록 구현해야 할수도 있음
            }
        }

        public void Dispose()
        {
            _healthBarCache.Dispose();
            _skillCooldownHudCache.Dispose();
        }
    }
}
