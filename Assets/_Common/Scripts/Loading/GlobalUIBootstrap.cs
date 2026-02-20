using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using VContainer;
using VContainer.Unity;

namespace DungeonShooter
{
    /// <summary>
    /// 게임 내내 유지될 UI들을 생성합니다.
    /// </summary>
    public class GlobalUIBootstrap : IStartable
    {
        private IObjectResolver _resolver;

        private Canvas _canvas;
        [Inject]
        public void Construct(IObjectResolver resolver)
        {
            _resolver = resolver;
        }

        public async void Start()
        {
            _canvas = await CreateCanvas();
            await CreateLoadingUIAsync();
        }

        private async UniTask<Canvas> CreateCanvas()
        {
            await UniTask.SwitchToMainThread();
            
            var canvas = new GameObject("GlobalUICanvas").AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = short.MaxValue;
            Object.DontDestroyOnLoad(canvas);
            return canvas;
        }
        
        private async UniTask CreateLoadingUIAsync()
        {
            await UniTask.SwitchToMainThread();
            var handle = Addressables.InstantiateAsync(UIAddresses.UI_Loading, _canvas.transform);
            await handle.Task;

            if (handle.Status != AsyncOperationStatus.Succeeded || handle.Result == null)
            {
                LogHandler.LogError<GlobalUIBootstrap>($"로딩 UI 생성 실패: {UIAddresses.UI_Loading}");
                return;
            }
            
            var loadingView = handle.Result.GetComponent<LoadingView>();

            if (loadingView != null)
                _resolver.Inject(loadingView);
        }
    }
}
