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
        private readonly SkillTableEntrySo _skillTableEntrySo;
        private readonly int _skillLevelIndex;
        private readonly SkillData _skillData;
        private readonly Sprite _icon;
        private readonly ISkillObjectFactory _skillObjectFactory;
        private readonly ISoundSfxService _soundSfxService;

        public SkillData SkillData => _skillData;
        public SkillTableEntrySo SkillTableEntrySo => _skillTableEntrySo;
        public int SkillLevelIndex => _skillLevelIndex;
        public Sprite Icon => _icon;
        public bool IsCooldown { get; private set; }
        public float Cooldown { get; private set; }
        public float MaxCooldown => _skillTableEntrySo != null && _skillTableEntrySo.SkillLevels != null && _skillLevelIndex >= 0 && _skillLevelIndex < _skillTableEntrySo.SkillLevels.Count
            ? _skillTableEntrySo.SkillLevels[_skillLevelIndex].Cooldown
            : 0f;
        public event Action OnExecute;
        public Action<float> OnCooldownChanged { get; set; }
        public Action OnCooldownEnded { get; set; }

        public Skill(SkillTableEntrySo skillTableEntrySo, int skillLevelIndex, SkillData skillData, Sprite icon, ISkillObjectFactory skillObjectFactory, ISoundSfxService soundSfxService)
        {
            _skillTableEntrySo = skillTableEntrySo;
            _skillLevelIndex = skillLevelIndex;
            _skillData = skillData;
            _skillObjectFactory = skillObjectFactory;
            _icon = icon;
            _soundSfxService = soundSfxService;
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
                var nameText = _skillTableEntrySo != null ? _skillTableEntrySo.SkillName : string.Empty;
                LogHandler.Log<Skill>($"스킬 쿨다운 중: {_skillTableEntrySo?.Id}({nameText})");
                return false;
            }

            var context = SkillExecutionContext.Create()
                .WithCaster(caster)
                .WithSkillObjectFactory(_skillObjectFactory)
                .WithSoundSfxService(_soundSfxService);
            OnExecute?.Invoke();
            StartCooldown(MaxCooldown);

            return await ExecuteEffectsAsync(context);
        }

        /// <summary>
        /// 쿨다운을 시작합니다.
        /// </summary>
        public void StartCooldown(float cooldown)
        {
            if (_skillTableEntrySo == null || MaxCooldown <= 0f)
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
            if (_skillData == null || _skillTableEntrySo == null)
                return false;

            var levelData = _skillTableEntrySo.SkillLevels[_skillLevelIndex];
            bool allSuccess = true;

            foreach (var effect in _skillData.ActiveEffects)
            {
                if (effect == null)
                    continue;

                bool result = await effect.Execute(context, levelData);
                allSuccess &= result;
            }

            return allSuccess;
        }

        /// <summary>
        /// 패시브 효과를 활성화합니다.
        /// </summary>
        public void Activate(EntityBase owner)
        {
            if (_skillData == null || owner == null || _skillTableEntrySo == null)
                return;

            var levelData = _skillTableEntrySo.SkillLevels[_skillLevelIndex];

            foreach (var effect in _skillData.PassiveEffects)
            {
                if (effect == null)
                    continue;

                effect.Activate(owner, levelData);
            }
        }

        /// <summary>
        /// 패시브 효과를 비활성화합니다.
        /// </summary>
        public void Deactivate(EntityBase owner)
        {
            if (_skillData == null || owner == null || _skillTableEntrySo == null)
                return;

            var levelData = _skillTableEntrySo.SkillLevels[_skillLevelIndex];

            foreach (var effect in _skillData.PassiveEffects)
            {
                if (effect == null)
                    continue;

                effect.Deactivate(owner, levelData);
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
