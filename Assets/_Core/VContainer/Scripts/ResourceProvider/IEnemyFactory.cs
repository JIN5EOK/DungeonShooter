using Cysharp.Threading.Tasks;
using UnityEngine;

namespace DungeonShooter
{
    /// <summary>
    /// 적 캐릭터를 생성하는 팩토리 인터페이스
    /// </summary>
    public interface IEnemyFactory
    {
        UniTask<Enemy> GetRandomEnemyAsync();
        Enemy GetRandomEnemySync();
    }
}
