using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using VContainer;
using VContainer.Unity;

namespace DungeonShooter
{
    /// <summary>
    /// 씬을 로드할 때 사용하는 일회성 객체
    /// 컨텍스트를 추가한 후 LoadScene을 호출하면 하나의 LifetimeScope.Enqueue 블록에서 모든 컨텍스트를 등록한 후 씬을 로드한다
    /// </summary>
    public class SceneLoader
    {
        private readonly List<Action<IContainerBuilder>> _contextRegistrations = new();

        /// <summary>
        /// 로드할 씬의 LifeTimeScope에 반영할 Context를 추가한다
        /// </summary>
        public SceneLoader AddContext<T>(T context) where T : class
        {
            _contextRegistrations.Add(builder => builder.RegisterInstance(context));
            return this;
        }

        /// <summary>
        /// Addressables를 통해 비동기로 씬을 로드한다
        /// 내부적으로 LifetimeScope.Enqueue 블록을 생성하여 이전에 AddContext로 추가한 모든 컨텍스트를 등록한 후 씬을 로드한다
        /// </summary>
        public async UniTask LoadScene(string sceneName)
        {
            using (LifetimeScope.Enqueue(builder =>
            {
                foreach (var registration in _contextRegistrations)
                {
                    registration?.Invoke(builder);
                }
            }))
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
                catch (Exception ex)
                {
                    LogHandler.LogError<SceneLoader>($"씬 로드에 실패했습니다: {sceneName}");
                    throw;
                }
            }
        }
    }
}
