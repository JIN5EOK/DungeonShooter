using Cysharp.Threading.Tasks;
using VContainer;

namespace DungeonShooter
{
    public interface IGameExitService
    {
        void ExitToMainMenu();
    }
    
    public class GameExitService : IGameExitService
    {
        private readonly SceneLoader _sceneLoader;

        [Inject]
        public GameExitService(SceneLoader sceneLoader)
        {
            _sceneLoader = sceneLoader;
        }

        public void ExitToMainMenu()
        {
            _sceneLoader.LoadScene(SceneNames.MainMenuScene).Forget();
        }
    }
}
