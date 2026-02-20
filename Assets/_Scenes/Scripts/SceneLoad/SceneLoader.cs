using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using VContainer;
using VContainer.Unity;
using Object = UnityEngine.Object;

namespace DungeonShooter
{
    /// <summary>
    /// 씬을 로드할 때 사용하는 서비스. VContainer에 Singleton으로 등록해 주입받아 사용한다.
    /// LoadScene 호출 시 전달한 컨텍스트를 로드할 씬의 LifeTimeScope에 등록한 뒤 씬을 로드한다.
    /// ILoadingService가 주입된 경우 로딩 윈도우를 띄운 뒤 씬 로드 및 씬 초기화(ISceneInitializer) 완료까지 대기한다.
    /// </summary>
    public class SceneLoader
    {
        private readonly ILoadingService _loadingService;

        [Inject]
        public SceneLoader(ILoadingService loadingService)
        {
            _loadingService = loadingService;
        }

        /// <summary>
        /// Addressables를 통해 비동기로 씬을 로드한다. 컨텍스트 없이 로드할 때 사용한다.
        /// </summary>
        public async UniTask LoadScene(string sceneName)
        {
            if (_loadingService != null)
            {
                var task = new LoadingTask(LoadingType.LoadingWindow, async ct =>
                {
                    await LoadSceneInternal(sceneName);
                    await WaitForSceneInitializerAsync();
                });
                _loadingService.EnqueueTask(task);
                await task.CompletionTask;
            }
        }

        /// <summary>
        /// Addressables를 통해 비동기로 씬을 로드한다.
        /// context: 로드할 씬의 LifeTimeScope에 등록할 씬 초기화 정보 컨텍스트
        /// </summary>
        public async UniTask LoadScene<T>(string sceneName, T context) where T : class
        {
            if (_loadingService != null)
            {
                var task = new LoadingTask(LoadingType.LoadingWindow, async ct =>
                {
                    await LoadSceneInternal(sceneName, context);
                    await WaitForSceneInitializerAsync();
                });
                _loadingService.EnqueueTask(task);
                await task.CompletionTask;
            }
        }

        private static async UniTask LoadSceneInternal(string sceneName)
        {
            await LoadSceneAsync(sceneName);
        }

        private static async UniTask LoadSceneInternal<T>(string sceneName, T context) where T : class
        {
            // 다음 씬의 라이프타임 스코프에 컨텍스트 주입
            using (LifetimeScope.Enqueue(builder => builder.RegisterInstance(context)))
            {
                await LoadSceneAsync(sceneName);
            }
        }

        private static async UniTask LoadSceneAsync(string sceneName)
        {
            try
            {
                var handle = Addressables.LoadSceneAsync(sceneName);
                await handle.Task;

                if (handle.Status != AsyncOperationStatus.Succeeded)
                {
                    LogHandler.LogError<SceneLoader>($"씬 로드에 실패했습니다: {sceneName}");
                }
            }
            catch (Exception)
            {
                LogHandler.LogError<SceneLoader>($"씬 로드에 실패했습니다: {sceneName}");
                throw;
            }
        }

        private static async UniTask WaitForSceneInitializerAsync()
        {
            var initializer = Object.FindAnyObjectByType<SceneInitializerBase>();

            if (initializer == null)
                return;

            await UniTask.WaitUntil(() => initializer.IsSceneInitialized);
        }
    }
}
