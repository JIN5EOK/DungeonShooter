## 게임 결과/일시정지 UI 구상

### 룰
- UI에 쓰이는 텍스트는 ViewModel 혹은 Presenter에서 지정한다
  - 텍스트를 가져오는 것 자체가 일종의 기능에 해당하기 때문 (테이블에서 텍스트 가져오기)

### 구상
- 기능 서비스
  - `PauseManager` 
    - 일시정지 기능 서비스
  - `GameExitService` (구현 필요)
    - 씬 이동과 같은 게임 종료 기능 서비스
  - `PlayTimerService` (구현 필요)
    - 게임 시작 후 지난 시간 담당 서비스
- UI
  - **게임 일시정지 UI, MVP 패턴 사용**
    - `GamePauseView`
      - 필드/함수
        - event OnClickResumeButton
          - 재개 버튼 클릭 이벤트
        - event OnClickExitButton
          - 종료 버튼 클릭 이벤트
    - `GamePausePresenter`
      - 필드/함수
        - PauseGame()
          - 일시정지 활성화시 `PauseManager`로 게임 일시정지 요청, 게임 종료 요청시 `GameExitService`로 게임 종료 요청
        - ExitGame()
          - 게임 종료
      - 기능 설명
        - View를 참고하여 기능 실행
  - **게임 결과 UI, MVVM 패턴 사용**
    - `GameResultView`
      - 기능 설명
        - ViewModel 참고하여 UI 갱신
    - `GameResultViewModel`
    - 필드/함수
      - string EnemyKillCountText ("102마리")
      - string PlayTimeText ("25:39")
      - string MessageText : (게임 오버.. / 게임 클리어!)
      - string RetryButtonText : (다시하기)
      - string ExitButtonText : (종료하기)
      - event OnExit
      - event OnRetry
    - 기능 설명
      - 일시정지 활성화시 `PauseManager`로 게임 일시정지 요청, 게임 종료 요청시 `GameExitService`로 게임 종료 요청