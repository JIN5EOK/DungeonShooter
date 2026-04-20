using Jin5eok;

namespace DungeonShooter
{
    public interface IEntityAI
    {
        public void Tick();
    }

    public class EnemyBTAI : IEntityAI
    {
        private AiBTContext _context = new();
        private IBehaviourTreeNode<AiBTContext> _btNode;
        public EnemyBTAI(EntityBase self, AiBTBase aiBtBase)
        {
            _context.Self = self;
            _btNode = aiBtBase.GetTree();
        }
        public void Tick()
        {
            _btNode?.Execute(_context);
        }
    }
}