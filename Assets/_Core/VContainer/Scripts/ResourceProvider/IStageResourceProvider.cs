using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace DungeonShooter
{
    public interface IStageResourceProvider : ISceneResourceProvider
    {
        UniTask<TileBase> GetGroundTile();
        UniTask<Enemy> GetRandomEnemy();
        UniTask<Player> GetPlayer();
    }
}