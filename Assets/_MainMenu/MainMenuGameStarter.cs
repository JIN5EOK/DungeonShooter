using DungeonShooter;
using UnityEngine;

public class MainMenuGameStarter : MonoBehaviour
{
    // TODO: 테스트용 코드로 개선 필요함
    private async void Start()
    {
        var loader = new SceneLoader();
        var context = new StageContext();
        var isloadSucceedConfig = await context.LoadConfigAsync("StageConfig_001");
        if(isloadSucceedConfig)
            await loader.AddContext(context).LoadScene("PrototypeScene");
    }
}
