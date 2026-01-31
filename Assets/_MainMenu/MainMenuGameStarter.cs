using DungeonShooter;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class MainMenuGameStarter : IPostStartable
{
    public async void PostStart()
    {
        var loader = new SceneLoader();
        // TODO: 실제 플레이어 선택 및 스테이지 선택 로직으로 대체 필요
        var context = new StageContext(12000000, 13000001); // playerConfigTableId, stageConfigTableId
        await loader
            .AddContext(context)
            .LoadScene("StageScene");
    }
}
