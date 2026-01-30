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
        UIManager uiManager;
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponentOnNewGameObject<UIManager>(Lifetime.Singleton).DontDestroyOnLoad();
            builder.Register<InputManager>(Lifetime.Singleton);
            builder.Register<LocalTableRepository>(Lifetime.Singleton).AsImplementedInterfaces();
            base.Configure(builder);
        }
        
        protected override void Awake()
        {
            base.Awake();
            Container.Resolve<UIManager>();
            DontDestroyOnLoad(gameObject);
        }
    }
}

