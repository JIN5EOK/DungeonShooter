using UnityEngine;

namespace DungeonShooter
{
    /// <summary>
    /// 스테이지 생성 관련 세부 로직을 담당하는 클래스
    /// </summary>
    public class StageGenerator
    {
        /// <summary>
        /// 스테이지를 생성합니다.
        /// </summary>
        public Stage GenerateStage()
        {
            Stage stage = new Stage();

            // TODO: 스테이지 생성 로직 구현
            // 1. RoomDataSerializer를 통해 역직렬화된 RoomData를 가져오기
            // 2. 정해진 로직에 따라 각 방을 이어붙여 Stage 생성
            // 3. 랜덤 선택, 연결 규칙 등 적용

            Debug.LogWarning($"[{nameof(StageGenerator)}] GenerateStage는 아직 구현되지 않았습니다.");
            return stage;
        }
    }
}

