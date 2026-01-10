using System;
using UnityEngine;
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

        public StageResourceProvider()
        {
            _addressablesScope = new AddressablesScope();
        }

        /// <summary>
        /// Top 타일을 가져옵니다.
        /// </summary>
        public async Awaitable<TileBase> GetTopTile()
        {
            // TODO: 구현 필요
            return null;
        }

        /// <summary>
        /// Wall 타일을 가져옵니다.
        /// </summary>
        public async Awaitable<TileBase> GetWallTile()
        {
            // TODO: 구현 필요
            return null;
        }

        /// <summary>
        /// Ground 타일을 가져옵니다.
        /// </summary>
        public async Awaitable<TileBase> GetGroundTile()
        {
            // TODO: 구현 필요
            return null;
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