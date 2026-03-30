using System;
using UnityEngine;
using VContainer;

namespace DungeonShooter
{
    public class VPSurvivalSceneInitializer : MonoBehaviour
    {
        private IPlayerFactory _playerFactory;
        [Inject]
        public void Construct(IPlayerFactory playerFactory)
        {
            _playerFactory = playerFactory;
             
        }

        public void Start()
        {
            _playerFactory.GetPlayerAsync(Vector2.zero);
        }
    }
}