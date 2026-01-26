using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Tilemaps;
using VContainer;

namespace DungeonShooter
{
    /// <summary>
    /// 타일을 제공하는 리포지토리
    /// </summary>
    public class TileRepository : ITileRepository
    {
        private readonly StageConfig _stageConfig;
        private readonly ISceneResourceProvider _sceneResourceProvider;

        [Inject]
        public TileRepository(StageContext context, ISceneResourceProvider sceneResourceProvider)
        {
            _stageConfig = context.StageConfig;
            _sceneResourceProvider = sceneResourceProvider;
        }

        /// <summary>
        /// Ground 타일을 가져옵니다.
        /// </summary>
        public async UniTask<TileBase> GetGroundTileAsync()
        {
            var groundTileAddress = GetGroundTileAddress();
            if (groundTileAddress == null)
            {
                return null;
            }

            return await _sceneResourceProvider.GetAssetAsync<TileBase>(groundTileAddress);
        }

        /// <summary>
        /// Ground 타일을 동기적으로 가져옵니다.
        /// </summary>
        public TileBase GetGroundTileSync()
        {
            var groundTileAddress = GetGroundTileAddress();
            if (groundTileAddress == null)
            {
                return null;
            }

            return _sceneResourceProvider.GetAssetSync<TileBase>(groundTileAddress);
        }

        /// <summary>
        /// Ground 타일 어드레스 추출 및 검증
        /// </summary>
        private string GetGroundTileAddress()
        {
            if (_stageConfig.GroundTile == null || !_stageConfig.GroundTile.RuntimeKeyIsValid())
            {
                Debug.LogWarning($"[{nameof(TileRepository)}] Ground 타일이 설정되지 않았습니다.");
                return null;
            }

            return _stageConfig.GroundTile.RuntimeKey.ToString();
        }
    }
}
