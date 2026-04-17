using Cysharp.Threading.Tasks;
using UnityEngine;

namespace DungeonShooter
{
    public interface ICameraManager
    {
        public UniTask BindAsync(Transform target);
    }
}

