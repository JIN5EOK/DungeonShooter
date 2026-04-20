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
        private IEnemyFactory _enemyFactory;
        [Inject]
        public void Construct(IPlayerFactory playerFactory, ITableRepository tableRepository, IEnemyFactory enemyFactory)
        {
            _playerFactory = playerFactory;
            _tableRepository = tableRepository;
            _enemyFactory = enemyFactory;
        }

        public void Update()
        {
            if(Input.GetKeyDown(KeyCode.Space))
                _playerFactory.GetPlayerSync(_tableRepository.GetTableEntry<PlayerConfigSo>(12000001));
            
            if(Input.GetKeyDown(KeyCode.RightShift))
                _enemyFactory.GetEnemyByConfigIdSync(_tableRepository.GetTableEntry<EnemyConfigSo>(18000000).Id);
        }
    }
}