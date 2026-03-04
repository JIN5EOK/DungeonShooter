using VContainer;

namespace DungeonShooter
{
    public class GameMessageService : IGameMessageService
    {
        private readonly AlertMessageViewModel _alertMessageViewModel;

        [Inject]
        public GameMessageService(AlertMessageViewModel alertMessageViewModel)
        {
            _alertMessageViewModel = alertMessageViewModel;
        }

        public void ShowAlertMessage(string message)
        {
            _alertMessageViewModel?.SetMessage(message);
        }
    }
}
