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
            builder.Register<InputManager>(Lifetime.Singleton);
            
            // 테이블 리포지토리 등록
            builder.Register<LocalTableRepository>(Lifetime.Singleton);
            builder.Register<ITableRepository>(provider => provider.Resolve<LocalTableRepository>(), Lifetime.Singleton);
            
            // 테이블 리포지토리 초기화
            builder.RegisterBuildCallback(container =>
            {
                var tableRepository = container.Resolve<LocalTableRepository>();
                tableRepository.Initialize();
            });
            
            base.Configure(builder);
        }
        
        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);
        }
    }
}

