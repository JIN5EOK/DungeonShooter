using UnityEngine;

namespace DungeonShooter
{
    /// <summary>
    /// 렌더링 레이어 관련 데이터 묶음. 이름/ID/정렬값을 한 번에 관리한다.
    /// 타일맵이나 스프라이트 렌더러의 정렬 순서 레이어에 사용된다.
    /// </summary>
    public readonly struct RenderingLayer
    {
        public RenderingLayer(string layerName)
        {
            LayerName = layerName;
            LayerID = SortingLayer.NameToID(layerName);
            if (LayerID == 0 && !string.IsNullOrEmpty(layerName))
            {
                Debug.LogWarning($"[{nameof(RenderingLayer)}] '{layerName}' 렌더링 레이어가 정의되어 있지 않습니다.");
            }
            LayerValue = LayerID != 0 ? SortingLayer.GetLayerValueFromID(LayerID) : 0;
        }

        public string LayerName { get; }
        public int LayerID { get; }
        public int LayerValue { get; }
    }

    public static class RenderingLayers
    {
        public static readonly RenderingLayer Ground = new("Ground");
        public static readonly RenderingLayer Wall = new("Wall");
        public static readonly RenderingLayer Deco = new("Deco");

        /// <summary>
        /// SortingLayer ID로부터 이름을 가져옵니다.
        /// </summary>
        /// <param name="sortingLayerId">SortingLayer ID</param>
        /// <returns>레이어 이름, 찾을 수 없으면 "Layer_{ID}" 형식 반환</returns>
        public static string GetLayerName(int sortingLayerId)
        {
            var layers = SortingLayer.layers;
            foreach (var layer in layers)
            {
                if (layer.id == sortingLayerId)
                {
                    return layer.name;
                }
            }

            return $"Layer_{sortingLayerId}";
        }
    }
}