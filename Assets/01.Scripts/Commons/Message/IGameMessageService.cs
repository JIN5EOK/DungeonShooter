using System;

namespace DungeonShooter
{
    public interface IGameMessageService
    {
        public event Action<string> OnAlertMessageRequested;
        public void ShowAlertMessage(string message);
    }
}
