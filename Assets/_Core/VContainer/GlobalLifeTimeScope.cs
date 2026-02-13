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
        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);
            builder.Register<EventBus>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<InputManager>(Lifetime.Singleton);
            builder.Register<LocalTableRepository>(Lifetime.Singleton).AsImplementedInterfaces();
        }

        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);
        }
    }
}

