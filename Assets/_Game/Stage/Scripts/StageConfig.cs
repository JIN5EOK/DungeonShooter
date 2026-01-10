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
        [SerializeField] private AssetReferenceT<TileBase> _topTile;
        [SerializeField] public AssetLabelReference _stageEnemyLabel;
        [SerializeField] public AssetLabelReference _roomDataLabel;
        
        public AssetReferenceT<TileBase> WallTile => _wallTile;
        public AssetReferenceT<TileBase> GroundTile => _groundTile;
        public AssetReferenceT<TileBase> TopTile => _topTile;
        public AssetLabelReference StageEnemyLabel => _stageEnemyLabel;
        public AssetLabelReference RoomDataLabel => _roomDataLabel;
    }
}