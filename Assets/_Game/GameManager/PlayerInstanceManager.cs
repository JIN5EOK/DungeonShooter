using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

namespace DungeonShooter
{
/// <summary>
/// 플레이어 엔티티 게임오브젝트 인스턴스에 대한 UI/입력/이벤트 연동 담당
/// </summary>
    public class PlayerInstanceManager
    {
        private readonly PlayerInputController _playerInputController;

        private EntityBase _currentPlayerEntity;

        [Inject]
        public PlayerInstanceManager(PlayerInputController playerInputController)
        {
            _playerInputController = playerInputController;
        }
    }
}