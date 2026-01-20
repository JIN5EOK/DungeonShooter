using System;
using Cysharp.Threading.Tasks;

/// <summary>
/// 실행할 스킬 이펙트의 기본 추상 클래스
/// </summary>
[Serializable]
public abstract class EffectBase
{
    /// <summary>
    /// 이펙트를 실행합니다.
    /// </summary>
    /// <returns>실행 성공 여부</returns>
    public abstract UniTask<bool> Execute(EntityBase target);
}
