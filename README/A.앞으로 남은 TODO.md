# 미해결 이슈
- 오브젝트 컬링 구조 적용과 스테이지,방 생성 로직의 충돌문제 해결 #7
  - https://github.com/JIN5EOK/DungeonShooter/issues/7
## 필수 구현
- [ ] 추가 아이템 및 스킬, 플레이어, 적 추가
  - [ ] 플레이어블 캐릭터 1,2 완성
  - [ ] 무기와 패시브 아이템, 사용 아이템 추가

- [ ] 게임 플로우
  - [ ] 게임 종료시 메인화면으로 이동

- [ ] 적 이동 AI 개선
    - [ ] 현재 적이 플레이어를 추적하는 로직은 단순히 일직선으로 다가갈 뿐임, 장애물이 있으면 막힘

- [ ] 보스몬스터와 보스방 추가

## 최적화 및 구조 개선
### UI 구조 관련
- 뷰, 기능, 모델의 본격적인 분리 필요

### 그외 기타 정리사항
- [ ] 지저분한, 성능저하를 유발하는 코드나 에셋 정리
  - GetComponent 남발 등..
  - 어드레서블 에셋 그룹 정리
  - 스프라이트 아틀라스 처리



# 해결 완료 이슈
- ~~타일맵 직렬화 데이터 용량 감량~~
  - https://github.com/JIN5EOK/DungeonShooter/issues/2
- ~~스테이지,방 생성 로직의 분리 필요~~
  - https://github.com/JIN5EOK/DungeonShooter/issues/3
- ~~방 저장 구조와 방 에디터 구조 개선~~
  - https://github.com/JIN5EOK/DungeonShooter/issues/4
- ~~스탯 컴포넌트와 스킬 컴포넌트를 Pure C# 객체로 변경하기~~
  - https://github.com/JIN5EOK/DungeonShooter/issues/5
- ~~StageInstantiator, StageGenerator를 Static 객체가 아닌 일반 객체로 변경하기~~
  - https://github.com/JIN5EOK/DungeonShooter/issues/6 
