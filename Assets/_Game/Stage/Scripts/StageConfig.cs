using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Tilemaps;

namespace DungeonShooter
{
    [CreateAssetMenu(fileName = "New StageConfig", menuName = "Stage/Stage Config")]
    public class StageConfig : ScriptableObject
    {
        [SerializeField] private AssetReferenceT<TileBase> _wallTile;
        [SerializeField] private AssetReferenceT<TileBase> _groundTile;
        [SerializeField] public AssetLabelReference _stageEnemyLabel;
        
        [SerializeField] public AssetLabelReference _startRoomDataLabel;
        [SerializeField] public AssetLabelReference _normalRoomDataLabel;
        [SerializeField] public AssetLabelReference _bossRoomDataLabel;
        public AssetReferenceT<TileBase> WallTile => _wallTile;
        public AssetReferenceT<TileBase> GroundTile => _groundTile;
        public AssetLabelReference StageEnemyLabel => _stageEnemyLabel;
        public AssetLabelReference StartRoomDataLabel => _startRoomDataLabel;
        public AssetLabelReference NormalRoomDataLabel => _normalRoomDataLabel;
        public AssetLabelReference BossRoomDataLabel => _bossRoomDataLabel;
    }
}