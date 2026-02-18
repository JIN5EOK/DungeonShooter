using System;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonShooter
{
    public interface IPauseService
    {
        public void PauseRequest(object requester);
        public void ResumeRequest(object requester);
    }
    
    /// <summary>
    /// 일시정지 요청을 관리 및 실제로 일시정지를 처리 담당
    /// </summary>
    public class PauseService : IPauseService, IDisposable
    {
        private readonly HashSet<object> _pauseOwners = new();
        private const float DefaultTimeScale = 1f;
        public bool IsPaused => _pauseOwners.Count > 0;
        
        public void PauseRequest(object requester)
        {
            if (requester == null)
                return;

            if (_pauseOwners.Add(requester))
            {
                ApplyPause();
            }
        }

        public void ResumeRequest(object requester)
        {
            if (requester == null)
                return;

            _pauseOwners.Remove(requester);

            if (_pauseOwners.Count == 0)
                ApplyResume();
        }

        public void Dispose()
        {
            if (_pauseOwners.Count > 0)
            {
                _pauseOwners.Clear();
                ApplyResume();
            }
        }

        private void ApplyPause()
        {
            Time.timeScale = 0f;
        }

        private void ApplyResume()
        {
            Time.timeScale = DefaultTimeScale;
        }
    }
}
