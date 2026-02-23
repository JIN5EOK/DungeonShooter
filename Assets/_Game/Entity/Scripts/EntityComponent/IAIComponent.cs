using System.Collections.Generic;
using Jin5eok;

namespace DungeonShooter
{
    /// <summary>
    /// 행동트리를 실행하는 AI 컴포넌트 인터페이스.
    /// </summary>
    public interface IAIComponent
    {
        public void Initialize(AiBTBase aiBT, List<Skill> activeSkills);
    }
}
