using UnityEngine;
using VContainer.Unity;

namespace DungeonShooter
{
    public static class Bootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Initialize()
        {
            // 런타임 시작 전 처리할 내용이 있다면 여기서 정의
        }       
    }
}