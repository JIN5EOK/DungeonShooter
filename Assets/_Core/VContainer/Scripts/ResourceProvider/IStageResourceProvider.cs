using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace DungeonShooter
{
    public interface IStageResourceProvider : ISceneResourceProvider
    {
        UniTask<TileBase> GetGroundTileAsync();
        UniTask<Enemy> GetRandomEnemyAsync();
        UniTask<Player> GetPlayerAsync();
        TileBase GetGroundTileSync();
        Enemy GetRandomEnemySync();
        Player GetPlayerSync();
    }
}