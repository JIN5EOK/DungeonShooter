using System;
using UnityEngine;

namespace DungeonShooter
{
    /// <summary>
    /// 플레이어(또는 다른 소유자)의 코인/골드 보유량을 관리한다.
    /// </summary>
    [DisallowMultipleComponent]
    public class CoinInventory 
    {
        [SerializeField, Min(0)]
        private int startingCoins = 0;

        public int CurrentCoins { get; private set; }

        /// <summary>
        /// 코인 보유량이 변경되었을 때 (변경 후 값 전달)
        /// </summary>
        public event Action<int> OnCoinsChanged;

        private void Awake()
        {
            CurrentCoins = Mathf.Max(0, startingCoins);
            NotifyChanged();
        }

        public void AddCoins(int amount)
        {
            if (amount <= 0)
                return;

            CurrentCoins += amount;
            NotifyChanged();
        }

        public bool TrySpendCoins(int amount)
        {
            if (amount <= 0)
                return true;

            if (CurrentCoins < amount)
                return false;

            CurrentCoins -= amount;
            NotifyChanged();
            return true;
        }

        private void NotifyChanged()
        {
            OnCoinsChanged?.Invoke(CurrentCoins);
        }
    }
}

