using System;
using UnityEngine;

namespace DungeonShooter
{
    public class ParticleSkillObject : SkillObjectBase
    {
        private ParticleSystem _particle;
        
        private void OnEnable()
        {
            _particle = GetComponent<ParticleSystem>();
            _particle.Play();
        }

        private void Update()
        {
            if (_particle.isPlaying == false)
            {
                Release();
            }
        }
        
        private void OnDisable()
        {
            _particle?.Stop();
        }
    }
}