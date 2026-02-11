using System;
using Cysharp.Threading.Tasks;
using Jin5eok;
using UnityEngine;
using VContainer;

namespace DungeonShooter
{
    /// <summary>
    /// 플레이어 고유 스킬을 담당합니다.
    /// SkillGroup을 소유하며, 엔티티 바인딩 시 데이터만 연동합니다.
    /// </summary>
    public class PlayerSkillController
    {
        public EntitySkillGroup SkillGroup { get; private set; } = new EntitySkillGroup();
        public EntityBase PlayerInstance => _playerInstance;

        private ISkillFactory _skillFactory;
        private Inventory _inventory;

        private Skill _skill1;
        private Skill _skill2;
        private EntityBase _playerInstance;

        [Inject]
        private void Construct(ISkillFactory skillFactory, Inventory inventory)
        {
            _skillFactory = skillFactory;
            _inventory = inventory;
        }

        /// <summary> 1번 액티브 스킬을 반환합니다. </summary>
        public Skill GetActiveSkill1() => _skill1;

        /// <summary>2번 액티브 스킬을 반환합니다. </summary>
        public Skill GetActiveSkill2() => _skill2;

        /// <summary>
        /// 선택한 플레이어 정보로 스킬 세션을 초기화합니다.
        /// </summary>
        public async UniTask InitializeAsync(PlayerConfigTableEntry config)
        {
            if (config == null)
            {
                LogHandler.LogWarning<PlayerSkillController>("PlayerConfigTableEntry가 null입니다.");
                return;
            }
            SkillGroup?.Clear();
            _skill1 = await _skillFactory.CreateSkillAsync(config.Skill1Id);
            _skill2 = await _skillFactory.CreateSkillAsync(config.Skill2Id);
            
            if (_skill1 != null) 
                SkillGroup?.Regist(_skill1);
            if (_skill2 != null) 
                SkillGroup?.Regist(_skill2);
        }

        /// <summary>
        /// 플레이어 엔티티에 SkillGroup만 바인딩합니다.
        /// </summary>
        public void BindPlayerInstance(EntityBase entity)
        {
            if (entity == null) return;

            _inventory.SetSkillGroup(SkillGroup);
            entity.SetSkillGroup(SkillGroup);
            _playerInstance = entity;
            entity.OnDestroyed += UnbindPlayerInstance;
        }

        /// <summary>
        /// 플레이어 엔티티 바인딩을 해제합니다.
        /// </summary>
        public void UnbindPlayerInstance(EntityBase entity)
        {
            if (_playerInstance != entity) return;
            _playerInstance = null;
        }

        /// <summary>
        /// 1번 액티브 스킬을 실행합니다.
        /// </summary>
        public void ExecuteActiveSkill1(EntityBase caster)
        {
            if (caster == null) return;
            _skill1?.Execute(caster).Forget();
        }

        /// <summary>
        /// 2번 액티브 스킬을 실행합니다.
        /// </summary>
        public void ExecuteActiveSkill2(EntityBase caster)
        {
            if (caster == null) return;
            _skill2?.Execute(caster).Forget();
        }
    }
}
