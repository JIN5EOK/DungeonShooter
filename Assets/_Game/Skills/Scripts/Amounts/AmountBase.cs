using System;

namespace DungeonShooter
{
    /// <summary>
    /// 스킬 수치의 비제네릭 베이스 클래스
    /// Dictionary 등에서 타입을 다루기 위한 계층
    /// </summary>
    [Serializable]
    public abstract class AmountBase { }
    
    /// <summary>
    /// 스킬 수치의 제네릭 베이스 클래스
    /// </summary>
    /// <typeparam name="T">수치의 타입 (int, float 등)</typeparam>
    [Serializable]
    public abstract class AmountBase<T> : AmountBase
    {
        /// <summary>
        /// 수치 값
        /// </summary>
        public T Amount { get; set; }

        protected AmountBase()
        {

        }

        protected AmountBase(T amount)
        {
            Amount = amount;
        }
    }
}
