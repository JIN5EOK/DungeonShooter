using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Tilemaps;
using Object = UnityEngine.Object;

namespace DungeonShooter
{
    public interface IStageResourceProvider : IDisposable
    {
        Task<TileBase> GetGroundTile();
        Task<Enemy> GetRandomEnemy();
        Task<Player> GetPlayer();
        Task<GameObject> GetInstance(string address);
        Task<T> GetAsset<T>(string address) where T : Object;
    }
}