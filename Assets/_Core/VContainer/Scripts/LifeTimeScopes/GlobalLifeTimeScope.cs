using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace DungeonShooter
{
    /// <summary>
    /// 게임 전체 영역에서 사용되는 서비스들을 등록하는 라이프타임 스코프
    /// </summary>
    public class GlobalLifeTimeScope : LifetimeScope
    {
        [SerializeField] private InputManager _inputManager;
        
        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);
            builder.Register<PauseManager>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<GlobalResourceProvider>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
            builder.RegisterComponentOnNewGameObject<GlobalUIManager>(Lifetime.Singleton).DontDestroyOnLoad().AsImplementedInterfaces().AsSelf();
            builder.RegisterInstance(_inputManager);
            builder.Register<LocalTableRepository>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<EventBus>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<ItemFormatter>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<LoadingService>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<LoadingViewModel>(Lifetime.Singleton);
            builder.Register<SceneLoader>(Lifetime.Singleton);
            new GlobalUIInstaller().Install(builder);
        }

        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);
        }
    }
}

