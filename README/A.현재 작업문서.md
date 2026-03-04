### 클래스 다이어그램

# 개요
- 게임 종료처리, 결과처리, 중단처리가 미구현 됨
- 관련 서비스 클래스를 추가하고 UI를 만들기

# 다이어그램 및 구상
```mermaid
classDiagram
    class PauseManager {
        +일시정지 기능
    }

    class GameExitService {
        +씬 이동/게임 종료
    }

    class GamePauseView {
        +event OnClickResumeButton
        +event OnClickExitButton
    }

    class GamePausePresenter {
        +PauseGame()
        +ExitGame()
    }

    class GameResultView {
        +event OnExitClickedEvent
        +ShowResult(string, string, string, string)
    }

    class GameResultModel {
        +Result : GameResult // 결과 enum
        +EnemyKillCount : int
        +PlayTimeSecond : int
    }

    class GameResultPresenter {
        +BindView(GameResultView view)
        +ExitGame()
    }
    
    class GameResultService{
        +OnGameResult : event Action~GameResultModel~
        +ExecuteGameResult(GameResult result) void
    }
    
    GamePauseView <-- GamePausePresenter : 참조
    GamePausePresenter --> PauseManager : UI 활성화시 일시정지 요청
    GamePausePresenter --> GameExitService : 종료기능 사용
    GameResultService <-- GameResultPresenter : 구독하여 모델 기반으로 뷰 갱신
    GameResultModel <.. GameResultService : 이벤트 파라미터, 게임 결과 요청시 생성
    PauseManager <-- GameResultService : 게임 결과 나오면 일시정지 요청
    GameResultView <-- GameResultPresenter : 참조
    GameExitService <-- GameResultPresenter : 버튼 클릭시 종료 기능 참조
```
### 구상
- 기능 서비스
  - `PauseManager` 
    - 일시정지 기능 서비스
  - `GameExitService` (구현 필요)
    - 씬 이동과 같은 게임 종료 기능 서비스
  - `PlayTimerService` (구현 필요)
    - 게임 시작 후 지난 시간 담당 서비스