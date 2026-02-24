using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace DungeonShooter
{
    /// <summary>
    /// 스킬 인스턴스 구현 클래스
    /// </summary>
    public class Skill
    {
        private readonly SkillData _skillData;
        private readonly SkillTableEntry _skillTableEntry;
        private readonly Sprite _icon;
        private readonly ISceneResourceProvider _resourceProvider;
        private readonly ISkillObjectFactory _skillObjectFactory;
        private readonly ITableRepository _tableRepository;

        public SkillData SkillData => _skillData;
        public SkillTableEntry SkillTableEntry => _skillTableEntry;
        public Sprite Icon => _icon;
        public bool IsCooldown { get; private set; }
        public float Cooldown { get; private set; }
        public float MaxCooldown => _skillTableEntry.Cooldown;
        public event Action OnExecute;
        public Action<float> OnCooldownChanged { get; set; }
        public Action OnCooldownEnded { get; set; }

        public Skill(SkillTableEntry skillTableEntry, SkillData skillData, Sprite icon, ISceneResourceProvider resourceProvider, ISkillObjectFactory skillObjectFactory, ITableRepository tableRepository = null)
        {
            _skillTableEntry = skillTableEntry;
            _skillData = skillData;
            _skillObjectFactory = skillObjectFactory;
            _icon = icon;
            _resourceProvider = resourceProvider;
            _tableRepository = tableRepository;
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
            if (IsCooldown)
            {
                var nameText = _tableRepository?.GetStringText(_skillTableEntry.SkillNameId) ?? _skillTableEntry.SkillNameId.ToString();
                LogHandler.Log<Skill>($"스킬 쿨다운 중: {SkillTableEntry.Id}({nameText})");
                return false;
            }

            var context = SkillExecutionContext.Create()
                .WithCaster(caster)
                .WithResourceProvider(_resourceProvider)
                .WithSkillObjectFactory(_skillObjectFactory);
            OnExecute?.Invoke();
            StartCooldown(MaxCooldown);
            
            return await ExecuteEffectsAsync(context);
        }

                
        /// <summary>
        /// 쿨다운을 시작합니다.
        /// </summary>
        public void StartCooldown(float cooldown)
        {
            if (_skillTableEntry == null || _skillTableEntry.Cooldown <= 0f)
                return;

            IsCooldown = true;
            Cooldown = cooldown;

            UpdateCooldownAsync().Forget();
        }
        
        /// <summary>
        /// 스킬 효과를 비동기로 실행합니다.
        /// </summary>
        private async UniTask<bool> ExecuteEffectsAsync(SkillExecutionContext context)
        {
            if (_skillData == null || _skillTableEntry == null)
                return false;
            
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
        public void Activate(EntityBase owner)
        {
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
        public void Deactivate(EntityBase owner)
        {
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
        /// 쿨다운을 업데이트합니다.
        /// </summary>
        private async UniTaskVoid UpdateCooldownAsync()
        {
            while (Cooldown > 0f)
            {
                await UniTask.Yield(PlayerLoopTiming.Update);

                Cooldown -= Time.deltaTime;
                Cooldown = Mathf.Max(0f, Cooldown);

                OnCooldownChanged?.Invoke(Cooldown);
            }

            IsCooldown = false;
            Cooldown = 0f;
            OnCooldownEnded?.Invoke();
        }
    }
}
