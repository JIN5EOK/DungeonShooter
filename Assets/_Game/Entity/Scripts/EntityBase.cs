using System;
using UnityEngine;
using Jin5eok;

namespace DungeonShooter
{
    public class EntityBase : MonoBehaviour
    {
        public event Action<EntityBase> OnDestroyed;

        private EntityContext _entityContext;

        public IEntityContext EntityContext => _entityContext;

        /// <summary>
        /// 엔티티를 해제 혹은 제거합니다. PoolableComponent가 있으면 풀에 반환하고, 없으면 게임오브젝트를 파괴합니다.
        /// </summary>
        public void Release()
        {
            var poolable = GetComponent<PoolableComponent>();
            if (poolable != null)
            {
                poolable.Release();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// 팩토리에서 생성한 EntityContext를 주입합니다.
        /// </summary>
        public void SetContext(EntityContext context)
        {
            if (_entityContext?.Skill != null)
            {
                foreach (var s in _entityContext.Skill.GetRegistedSkills())
                {
                    UnapplySkill(s);
                }

                _entityContext.Skill.OnSkillRegisted -= ApplySkill;
                _entityContext.Skill.OnSkillUnregisted -= UnapplySkill;
            }

            _entityContext = context;

            if (context?.Skill != null)
            {
                context.Skill.OnSkillRegisted += ApplySkill;
                context.Skill.OnSkillUnregisted += UnapplySkill;

                foreach (var s in context.Skill.GetRegistedSkills())
                {
                    ApplySkill(s);
                }
            }
        }

        private void ApplySkill(Skill skill)
        {
            if (skill?.SkillData != null && skill.SkillData.IsPassiveSkill)
            {
                skill.Activate(this);
            }
        }
        
        private void UnapplySkill(Skill skill)
        {
            if (skill?.SkillData != null && skill.SkillData.IsPassiveSkill)
            {
                skill.Deactivate(this);
            }
        }

        private void OnDestroy()
        {
            OnDestroyed?.Invoke(this);
        }
    }
}
