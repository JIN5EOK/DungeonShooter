using System;
using DungeonShooter;
using UnityEngine;
using VContainer;

namespace DefaultNamespace
{
    public class TempTestScript : MonoBehaviour
    {
        private ITableRepository _tableRepository;
        private IPlayerFactory _playerFactory;
        [Inject]
        public void Construct(IPlayerFactory playerFactory, ITableRepository tableRepository)
        {
            _playerFactory = playerFactory;
            _tableRepository = tableRepository;
        }

        public void Update()
        {
            if(Input.GetKeyDown(KeyCode.Space))
                _playerFactory.GetPlayerSync(_tableRepository.GetTableEntry<PlayerConfigSo>(12000001));
        }
    }
}