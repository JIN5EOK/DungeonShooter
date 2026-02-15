using System;
using UnityEngine;

namespace DungeonShooter
{
    public class ParticleSkillObject : SkillObjectBase
    {
        private ParticleSystem _particle;

        private void Awake()
        {
            _particle = GetComponent<ParticleSystem>();
        }

        public void Play()
        {
            _particle?.Play();
        }

        public void Stop()
        {
            _particle?.Stop();
        }
        
        private void OnDisable()
        {
            _particle?.Stop();
        }
    }
}