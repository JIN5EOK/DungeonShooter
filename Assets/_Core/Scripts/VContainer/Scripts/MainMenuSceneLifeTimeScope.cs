using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace DungeonShooter
{
    /// <summary>
    /// 게임 스테이지 씬에서 사용되는 서비스들을 등록하는 라이프타임 스코프
    /// </summary>
    public class MainMenuSceneLifeTimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<MainMenuGameStarter>(Lifetime.Scoped);
            base.Configure(builder);
        }
    }
}