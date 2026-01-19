using UnityEngine;

/// <summary>
/// 실행할 스킬 이펙트의 기본 추상 클래스
/// </summary>
[System.Serializable]
public abstract class EffectBase
{
    /// <summary>
    /// 이펙트를 실행합니다.
    /// </summary>
    /// <param name="owner">스킬을 발동한 Entity</param>
    /// <returns>실행 성공 여부</returns>
    public abstract bool Execute(EntityBase owner);
}
