using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace DungeonShooter
{
    /// <summary>
    /// 스킬 인스턴스 구현 클래스
    /// </summary>
    public class Skill : ISkill
    {
    private readonly SkillData _skillData;
    private CancellationTokenSource _cooldownCancellationTokenSource;
    
    public SkillData SkillData => _skillData;
    public bool IsCooldown { get; private set; }
    public float Cooldown { get; private set; }
    
    public event Action OnExecute;
    public Action<float> OnCooldownChanged { get; set; }
    public Action OnCooldownEnded { get; set; }
    
    public Skill(SkillData skillData)
    {
        if (skillData == null)
        {
            LogHandler.LogError<Skill>("SkillData가 null입니다.");
            return;
        }
        
        _skillData = skillData;
        Cooldown = 0f;
        IsCooldown = false;
    }
    
    /// <summary>
    /// 스킬을 실행합니다.
    /// </summary>
    /// <param name="target">스킬을 적용할 Entity</param>
    /// <returns>실행 성공 여부</returns>
    public async UniTask<bool> Execute(EntityBase target)
    {
        if (IsCooldown)
        {
            LogHandler.Log<Skill>($"스킬 쿨다운 중: {_skillData.SkillName}");
            return false;
        }
        
        if (_skillData == null)
        {
            LogHandler.LogError<Skill>("SkillData가 null입니다.");
            return false;
        }
                
        // 스킬 효과 실행 (비동기로 완료까지 대기)
        bool success = await ExecuteEffectsAsync(target);
        LogHandler.Log<Skill>($"스킬 실행 : {_skillData.SkillName}");
        OnExecute?.Invoke();
        StartCooldown();

        return success;
    }
    
    /// <summary>
    /// 스킬 효과를 비동기로 실행합니다.
    /// </summary>
    private async UniTask<bool> ExecuteEffectsAsync(EntityBase target)
    {
        bool allSuccess = true;
        
        foreach (var effect in _skillData.ActiveEffects)
        {
            if (effect != null)
            {
                try
                {
                    bool result = await effect.Execute(target);
                    if (!result)
                    {
                        allSuccess = false;
                    }
                }
                catch (Exception e)
                {
                    LogHandler.LogError<Skill>(e, "이펙트 실행 중 오류 발생");
                    allSuccess = false;
                }
            }
        }
        
        return allSuccess;
    }
    
    /// <summary>
    /// 패시브 효과를 활성화합니다.
    /// </summary>
    /// <param name="owner">스킬 소유자</param>
    public void Activate(EntityBase owner)
    {
        if (_skillData == null || owner == null)
        {
            return;
        }
        
        foreach (var effect in _skillData.PassiveEffects)
        {
            if (effect != null)
            {
                try
                {
                    effect.Activate(owner);
                }
                catch (Exception e)
                {
                    LogHandler.LogError<Skill>(e, "패시브 이펙트 활성화 중 오류 발생");
                }
            }
        }
    }
    
    /// <summary>
    /// 패시브 효과를 비활성화합니다.
    /// </summary>
    /// <param name="owner">스킬 소유자</param>
    public void Deactivate(EntityBase owner)
    {
        if (_skillData == null || owner == null)
        {
            return;
        }
        
        foreach (var effect in _skillData.PassiveEffects)
        {
            if (effect != null)
            {
                try
                {
                    effect.Deactivate(owner);
                }
                catch (Exception e)
                {
                    LogHandler.LogError<Skill>(e, "패시브 이펙트 비활성화 중 오류 발생");
                }
            }
        }
    }
    
    /// <summary>
    /// 쿨다운을 시작합니다.
    /// </summary>
    private void StartCooldown()
    {
        if (_skillData.Cooldown <= 0f)
        {
            return;
        }
        
        // 기존 쿨다운 업데이트 취소
        _cooldownCancellationTokenSource?.Cancel();
        _cooldownCancellationTokenSource?.Dispose();
        
        IsCooldown = true;
        Cooldown = _skillData.Cooldown;
        
        // 새로운 취소 토큰 생성
        _cooldownCancellationTokenSource = new CancellationTokenSource();
        
        // 쿨다운 업데이트 시작
        UpdateCooldownAsync(_cooldownCancellationTokenSource.Token).Forget();
    }
    
    /// <summary>
    /// 쿨다운을 업데이트합니다. Unity 플레이어 루프와 동기화됩니다.
    /// </summary>
    private async UniTaskVoid UpdateCooldownAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (Cooldown > 0f && !cancellationToken.IsCancellationRequested)
            {
                // Unity Update 루프와 동기화
                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
                
                Cooldown -= Time.deltaTime;
                Cooldown = Mathf.Max(0f, Cooldown);
                
                // 쿨다운 변경 이벤트 호출
                OnCooldownChanged?.Invoke(Cooldown);
            }
            
            // 쿨다운 종료 처리
            if (!cancellationToken.IsCancellationRequested)
            {
                IsCooldown = false;
                Cooldown = 0f;
                OnCooldownEnded?.Invoke();
            }
        }
        catch (OperationCanceledException)
        {
            // 취소된 경우 정상 종료
        }
    }
    
    /// <summary>
    /// 리소스를 정리합니다.
    /// </summary>
    public void Dispose()
    {
        _cooldownCancellationTokenSource?.Cancel();
        _cooldownCancellationTokenSource?.Dispose();
        _cooldownCancellationTokenSource = null;
    }
    }
}
