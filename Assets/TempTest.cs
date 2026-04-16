using System;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;

namespace DungeonShooter
{
    public class Test : MonoBehaviour
    {
        private IPlayerFactory _playerFactory;
        private IPlayerContextManager _playerContextManager;
        private PlayerInputManager _playerInputManager;
        private ITableRepository _tableRepository;
        [Inject]
        public void Construct(IPlayerFactory factory, IPlayerContextManager playerContextManager, PlayerInputManager inputManager, ITableRepository tableRepository)
        {
            _tableRepository = tableRepository;
            _playerContextManager = playerContextManager;
            _playerFactory = factory;
            _playerInputManager = inputManager;
        }

        private void Start()
        {
            var testPlayerId = 12000001;
            var config = _tableRepository.GetTableEntry<PlayerConfigSo>(testPlayerId);
            _playerContextManager.Initialize(config);
            _playerContextManager.InitializeSkillsAsync();
            var player = _playerFactory.GetPlayerSync(config, Vector3.zero);
            _playerInputManager.BindControlledEntity(player);
        }
    }
}