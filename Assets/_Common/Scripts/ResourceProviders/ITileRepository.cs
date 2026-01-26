using Cysharp.Threading.Tasks;
using UnityEngine.Tilemaps;

namespace DungeonShooter
{
    /// <summary>
    /// 타일을 제공하는 리포지토리 인터페이스
    /// </summary>
    public interface ITileRepository
    {
        UniTask<TileBase> GetGroundTileAsync();
        TileBase GetGroundTileSync();
    }
}
