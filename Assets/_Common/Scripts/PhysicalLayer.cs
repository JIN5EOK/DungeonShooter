using UnityEngine;

namespace DungeonShooter
{
    /// <summary>
    /// 레이어 관련 데이터 묶음. 이름/인덱스/마스크를 한 번에 관리한다.
    /// </summary>
    public readonly struct PhysicalLayer
    {
        public PhysicalLayer(string layerName)
        {
            LayerName = layerName;
            LayerIndex = LayerMask.NameToLayer(layerName);
            if (LayerIndex == -1)
            {
                Debug.LogWarning($"[{nameof(PhysicalLayer)}] '{layerName}' 레이어가 정의되어 있지 않습니다.");
            }
            Mask = LayerIndex >= 0 ? 1 << LayerIndex : 0;
        }

        public string LayerName { get; }
        public int LayerIndex { get; }
        public int Mask { get; }
    }

    public static class GameLayers
    {
        public static readonly PhysicalLayer Player = new("Player");
        public static readonly PhysicalLayer Enemy = new("Enemy");
    }
}