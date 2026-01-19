using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Tilemaps;
using Object = UnityEngine.Object;

namespace DungeonShooter
{
    public interface IStageResourceProvider : IDisposable
    {
        UniTask<TileBase> GetGroundTile();
        UniTask<Enemy> GetRandomEnemy();
        UniTask<Player> GetPlayer();
        UniTask<GameObject> GetInstance(string address);
        UniTask<T> GetAsset<T>(string address) where T : Object;
    }
}