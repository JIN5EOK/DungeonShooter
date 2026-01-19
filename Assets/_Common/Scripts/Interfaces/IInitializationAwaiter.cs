using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace DungeonShooter
{
    public interface IInitializationAwaiter
    {
        public Task<bool> InitializationTask { get; }
        public bool IsInitialized { get; }
    }
}