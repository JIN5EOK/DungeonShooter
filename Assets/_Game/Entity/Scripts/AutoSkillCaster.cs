using System.Linq;
using Cysharp.Threading.Tasks;

namespace DungeonShooter
{
    public class AutoSkillCaster
    {
        public void Tick(IEntityContext context, EntityBase caster)
        {
            if (context == null)
                return;
            
            var skill = context.Skills.GetSkills()
                .FirstOrDefault(s => s.MaxCooldown > 0 && !s.IsCooldown);

            if (skill != null)
                context.Skills.ExecuteSkillAsync(skill.SkillTableEntrySo.Id, caster).Forget();
        }
    }
}
