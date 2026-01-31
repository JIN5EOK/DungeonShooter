using System.Threading.Tasks;

namespace DungeonShooter
{
    public interface IInitializationAwaiter
    {
        public Task<bool> InitializationTask { get; }
        public bool IsInitialized { get; }
    }
}