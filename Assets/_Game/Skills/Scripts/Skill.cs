using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace DungeonShooter
{
    /// <summary>
    /// 스킬 인스턴스 구현 클래스
    /// </summary>
    public class Skill
    {
        private bool _isDisposed = false;
        
        private readonly SkillData _skillData;
        private readonly SkillTableEntry _skillTableEntry;
        private readonly Sprite _icon;
        private readonly ISceneResourceProvider _resourceProvider;
        private readonly CancellationTokenSource _cooldownCancellationTokenSource = new CancellationTokenSource();

        public SkillData SkillData => _skillData;
        public SkillTableEntry SkillTableEntry => _skillTableEntry;
        public Sprite Icon => _icon;
        public bool IsCooldown { get; private set; }
        public float Cooldown { get; private set; }
        public float MaxCooldown => _skillTableEntry.Cooldown;
        public event Action OnExecute;
        public Action<float> OnCooldownChanged { get; set; }
        public Action OnCooldownEnded { get; set; }
        
        public Skill(SkillTableEntry skillTableEntry, SkillData skillData, Sprite icon, ISceneResourceProvider resourceProvider)
        {
            if (skillTableEntry == null || SkillData == null || icon == null || resourceProvider == null)
            {
                throw new ArgumentNullException(nameof(Skill), "생성자 파라미터가 올바르지 않습니다.");
            }

            _skillTableEntry = skillTableEntry;
            _skillData = skillData;
            _icon = icon;
            _resourceProvider = resourceProvider;
            Cooldown = 0f;
            IsCooldown = false;
        }
        
        /// <summary>
        /// 스킬을 실행합니다.
        /// </summary>
        /// <param name="caster">스킬 시전자</param>
        /// <returns>실행 성공 여부</returns>
        public async UniTask<bool> Execute(EntityBase caster)
        {
            ThrowIfDisposed();
            
            if (IsCooldown)
            {
                LogHandler.Log<Skill>($"스킬 쿨다운 중: {SkillTableEntry.Id}({_skillTableEntry.SkillName})");
                return false;
            }

            var context = SkillExecutionContext.Create()
                .WithCaster(caster)
                .WithResourceProvider(_resourceProvider);

            OnExecute?.Invoke();
            StartCooldown();
            
            return await ExecuteEffectsAsync(context);
        }

        /// <summary>
        /// 스킬 효과를 비동기로 실행합니다.
        /// </summary>
        private async UniTask<bool> ExecuteEffectsAsync(SkillExecutionContext context)
        {
            ThrowIfDisposed();
            
            bool allSuccess = true;

            foreach (var effect in _skillData.ActiveEffects)
            {
                if (effect == null)
                    continue;
                
                bool result = await effect.Execute(context, _skillTableEntry);
                allSuccess &= result;
            }

            return allSuccess;
        }
        
        /// <summary>
        /// 패시브 효과를 활성화합니다.
        /// </summary>
        /// <param name="owner">스킬 소유자</param>
        public void Activate(EntityBase owner)
        {
            ThrowIfDisposed();
            
            if (_skillData == null || owner == null || _skillTableEntry == null)
                return;
            
            foreach (var effect in _skillData.PassiveEffects)
            {
                if (effect == null)
                    continue;
                
                effect.Activate(owner, _skillTableEntry);
            }
        }
        
        /// <summary>
        /// 패시브 효과를 비활성화합니다.
        /// </summary>
        /// <param name="owner">스킬 소유자</param>
        public void Deactivate(EntityBase owner)
        {
            ThrowIfDisposed();
            
            if (_skillData == null || owner == null || _skillTableEntry == null)
                return;
            
            foreach (var effect in _skillData.PassiveEffects)
            {
                if (effect == null)
                    continue;
                
                effect.Deactivate(owner, _skillTableEntry);
            }
        }
        
        /// <summary>
        /// 쿨다운을 시작합니다.
        /// </summary>
        private void StartCooldown()
        {
            if (_skillTableEntry == null || _skillTableEntry.Cooldown <= 0f)
                return;

            IsCooldown = true;
            Cooldown = _skillTableEntry.Cooldown;

            UpdateCooldownAsync(_cooldownCancellationTokenSource.Token).Forget();
        }

        /// <summary>
        /// 쿨다운을 업데이트합니다.
        /// </summary>
        private async UniTaskVoid UpdateCooldownAsync(CancellationToken cancellationToken)
        {
            while (Cooldown > 0f && !cancellationToken.IsCancellationRequested)
            {
                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);

                Cooldown -= Time.deltaTime;
                Cooldown = Mathf.Max(0f, Cooldown);

                OnCooldownChanged?.Invoke(Cooldown);
            }

            if (!cancellationToken.IsCancellationRequested)
            {
                IsCooldown = false;
                Cooldown = 0f;
                OnCooldownEnded?.Invoke();
            }
        }
        
        /// <summary>
        /// 리소스를 정리합니다.
        /// </summary>
        public void Dispose()
        {
            ThrowIfDisposed();

            _isDisposed = true;
            _cooldownCancellationTokenSource.Cancel();
            _cooldownCancellationTokenSource.Dispose();
        }

        private void ThrowIfDisposed()
        {
            if(_isDisposed)
                throw new ObjectDisposedException(nameof(Skill));
        }
    }
}
