using System.Runtime.CompilerServices;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using _MainMenu;

namespace DungeonShooter
{
    /// <summary>
    /// 메인 메뉴 씬에서 사용되는 서비스들을 등록하는 라이프타임 스코프
    /// </summary>
    public class MainMenuSceneLifeTimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            new CommonSceneInstaller().Install(builder);
            builder.Register<GameStartService>(Lifetime.Scoped).As<IGameStartService>();
        }
    }
}