using Jin5eok;
using UnityEngine;

/// <summary>
/// 세션 단위로 유지되는 싱글톤. 코인 인벤토리 등 전역 진행 데이터를 관리한다.
/// </summary>
[DisallowMultipleComponent]
public class GameSession : MonoSingleton<GameSession>
{
    [SerializeField] private CoinInventory _coinInventory;
    public CoinInventory CoinInventory => _coinInventory;

    protected override void Awake()
    {
        base.Awake();

        // 파괴 예정인 중복 인스턴스에서는 추가 초기화를 수행하지 않는다.
        if (Instance != this)
            return;

        if (_coinInventory == null)
        {
            _coinInventory = GetComponent<CoinInventory>();
            if (_coinInventory == null)
            {
                _coinInventory = gameObject.AddComponent<CoinInventory>();
            }
        }
    }
}

