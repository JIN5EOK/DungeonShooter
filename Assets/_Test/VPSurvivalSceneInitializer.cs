using System;
using UnityEngine;
using VContainer;

namespace DungeonShooter
{
    public class VPSurvivalSceneInitializer : MonoBehaviour
    {
        private IPlayerFactory _playerFactory;
        [Inject]
        public void Construct(IPlayerContextManager playerContextManager, IPlayerFactory playerFactory)
        {
            playerContextManager.Initialize(12000000);
            _playerFactory = playerFactory;    
        }

        public void Start()
        {
            _playerFactory.GetPlayerAsync(12000000, Vector2.zero);
        }
    }
}