using DungeonShooter;
using UnityEngine;

public class MainMenuGameStarter : MonoBehaviour
{
    // TODO: 테스트용 코드로 개선 필요함
    private async void Start()
    {
        var loader = new SceneLoader();
        // TODO: 실제 플레이어 선택 및 스테이지 선택 로직으로 대체 필요
        var context = new StageContext("Player_001", "1"); // playerPrefabKey, stageConfigTableId
        await loader.AddContext(context).LoadScene("PrototypeScene");
    }
}
