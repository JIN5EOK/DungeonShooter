using System;
using VContainer;

namespace DungeonShooter
{
    public class GameMessageService : IGameMessageService
    {
        public event Action<string> OnAlertMessageRequested;

        public void ShowAlertMessage(string message)
        {
            if (message == null)
                return;

            OnAlertMessageRequested?.Invoke(message);
        }
    }
}
