using UnityEngine;

/// <summary>
/// 프로젝트 전역에서 사용하는 태그/레이어 이름을 한 곳에서 관리한다.
/// 문자열 하드코딩을 피해서 추후 변경 시 리스크를 줄이기 위함.
/// </summary>
public static class GameTags
{
    public const string Player = "Player";
    public const string Enemy = "Enemy";
}

/// <summary>
/// 레이어 관련 데이터 묶음. 이름/인덱스/마스크를 한 번에 관리한다.
/// </summary>
public readonly struct GameLayer
{
    public GameLayer(string layerName)
    {
        LayerName = layerName;
        LayerIndex = LayerMask.NameToLayer(layerName);
        if (LayerIndex == -1)
        {
            Debug.LogWarning($"[GameLayer] '{layerName}' 레이어가 TagManager에 정의되어 있지 않습니다.");
        }
        Mask = LayerIndex >= 0 ? 1 << LayerIndex : 0;
    }

    public string LayerName { get; }
    public int LayerIndex { get; }
    public int Mask { get; }
}

public static class GameLayers
{
    public static readonly GameLayer Player = new("Player");
    public static readonly GameLayer Enemy = new("Enemy");
}

