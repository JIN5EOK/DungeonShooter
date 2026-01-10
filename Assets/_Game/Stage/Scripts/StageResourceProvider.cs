using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.Tilemaps;
using Jin5eok;
using Object = UnityEngine.Object;

namespace DungeonShooter
{

    public interface IStageResourceProvider : IDisposable
    {
        Awaitable<TileBase> GetTopTile();
        Awaitable<TileBase> GetWallTile();
        Awaitable<TileBase> GetGroundTile();
        Awaitable<TileBase> GetTile(string address);
        Awaitable<Enemy> GetRandomEnemy();
        Awaitable<GameObject> GetGameObject(string address);
    }
    /// <summary>
    /// 현재 스테이지에 적절한 타일, 캐릭터를 제공하는 클래스
    /// </summary>
    public class StageResourceProvider : IStageResourceProvider
    {
        private readonly AddressablesScope _addressablesScope;
        private readonly StageConfig _stageConfig;
        
        public StageResourceProvider(StageConfig config)
        {
            _addressablesScope = new AddressablesScope();
            _stageConfig = config;
        }
        
        
        /// <summary>
        /// Top 타일을 가져옵니다.
        /// </summary>
        public async Awaitable<TileBase> GetTopTile()
        {
            var handle = _addressablesScope.LoadAssetAsync<TileBase>(_stageConfig.TopTile);
            await handle.Task;
            return handle.Result;
        }

        /// <summary>
        /// Wall 타일을 가져옵니다.
        /// </summary>
        public async Awaitable<TileBase> GetWallTile()
        {
            var handle = _addressablesScope.LoadAssetAsync<TileBase>(_stageConfig.WallTile);
            await handle.Task;
            return handle.Result;
        }

        /// <summary>
        /// Ground 타일을 가져옵니다.
        /// </summary>
        public async Awaitable<TileBase> GetGroundTile()
        {
            var handle = _addressablesScope.LoadAssetAsync<TileBase>(_stageConfig.GroundTile);
            await handle.Task;
            return handle.Result;
        }

        /// <summary>
        /// 스테이지에 맞는 랜덤 적을 가져옵니다.
        /// </summary>
        public async Awaitable<Enemy> GetRandomEnemy()
        {
            // TODO: 구현 필요
            return null;
        }

        /// <summary>
        /// 주소에 해당하는 게임 오브젝트를 가져옵니다.
        /// </summary>
        public async Awaitable<GameObject> GetGameObject(string address)
        {
            // TODO: 구현 필요
            return null;
        }
        /// <summary>
        /// 주소에 해당하는 타일을 가져옵니다.
        /// </summary>
        public Awaitable<TileBase> GetTile(string address)
        {
            // TODO: 구현 필요
            return null;
        }

        public void Dispose()
        {
            _addressablesScope?.Dispose();
        }
    }
}