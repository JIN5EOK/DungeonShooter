using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace DungeonShooter
{
    public interface IStageTimer
    {
        float Remaining { get; }
        float Elapsed { get; }
        event Action OnTimerEnd;
        void Start(float duration);
        void Stop();
    }

    public class StageTimer : IStageTimer, IDisposable
    {
        public float Remaining { get; private set; }
        public float Elapsed { get; private set; }
        public event Action OnTimerEnd;

        private CancellationTokenSource _cts;

        public void Start(float duration)
        {
            Stop();
            Remaining = duration;
            Elapsed = 0f;
            _cts = new CancellationTokenSource();
            Tick(_cts.Token).Forget();
        }

        public void Stop()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }

        private async UniTaskVoid Tick(CancellationToken ct)
        {
            while (Remaining > 0f)
            {
                await UniTask.Yield(PlayerLoopTiming.Update, ct);
                var delta = Time.deltaTime;
                Remaining -= delta;
                Elapsed += delta;
            }

            Remaining = 0f;
            OnTimerEnd?.Invoke();
        }

        public void Dispose() => Stop();
    }
}
