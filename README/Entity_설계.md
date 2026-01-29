# 개요
플레이어, 적, 보물상자, NPC 등 게임 내의 Entity 개체들에 대한 설계 문서입니다.

---
## 컴포넌트 위주 설계
* 괸리자 역할을 담당하는 컴포넌트 + 다수의 기능 컴포넌트로 구성
* 기능 추가는 단일 책임 원칙을 베이스로 하는 여러 기능 컴포넌트 추가를 통한 확장 구조를 통해 수행
	* 예: 이동 컴포넌트는 이동만 담당하기

## EntityBase - Entity의 중추 기반 클래스
```mermaid
classDiagram
    EntityBase <|-- Player
    EntityBase <|-- Enemy
```
* `EntityBase`를 상속받은 `Player`같은 스크립트는 작동 로직은 최소화하고 주로 아래 역할을 담당한다
  * 객체 초기화
  * 객체 중앙관리

## EntityComponent - Entity의 기능 담당 파트들

### Entity 컴포넌트 예시

```mermaid
classDiagram
    class MovementComponent["MovementComponent : Monobehaviour<br>캐릭터 이동 기능 담당"]{
        +MoveSpeed : float
        +MoveDirection : Vector2
        +Move() void
    }
    class DashComponent["MovementComponent : Monobehaviour<br>캐릭터 대시 기능 담당"]{
        +IsDashing : bool
        +Dash() bool // 대시 성공여부 반환
        +CancelDash() void // 대시중이라면 취소
    }
    class EntityStatsComponent["EntityStatsComponent : Monobehaviour<br>캐릭터 스탯 기능 담당"]{
        // 스탯 데이터와 관련 연산
    }
```

```mermaid
classDiagram
    class HealthComponent["HealthComponent : Monobehaviour<br>캐릭터 체력 기능 담당"]{
        +TakeDamage() void
        +Heal() void
        +Die() void
    }
    class InteractComponent["InteractComponent : Monobehaviour<br>플레이어와 상호작용 가능 물체 상호작용 담당"]{
        +TryInteract() void 
        // () 후보에 있는 IInteractable 객체와 상호작용
        -RegisterInteractable() void
        -UnRegisterInteractable() void
        // () 범위내 객체를 후보로 등록/해제하거나 외부등록
    }
```

```mermaid
classDiagram
    class BehaviourTreeComponent["BehaviourTreeComponent : Monobehaviour<br>적 AI등 행동트리 담당"]{
        +SetBehaviourTree(BehaviourTreeNode bTNode) 
    }
```

### 컴포넌트간 참조는 자유롭게
```mermaid
classDiagram
    class EntityStatsComponent{
    }

    HealthComponent --> EntityStatsComponent : 최대 체력 참고
    MovementComponent --> EntityStatsComponent : 이동속도 참고
```
* `EntityComponent`는 모두 동일한 게임오브젝트에 붙이는걸 전제함
* 따라서 컴포넌트간 참조가 필요하다면 gameObject.GetComponent를 통해 참조
* 특정 컴포넌트가 존재하지 않을 가능성이 존재하므로 다른 컴포넌트를 참조할때 예외처리 반드시 수행

## Entity 스탯 시스템 구조

### 스탯 시스템 구조도
```mermaid
classDiagram
    class StatType["StatType<br>스탯 타입 Enum"]{
        Hp,
        Attack,
        Defense,
        MoveSpeed
    }
    class StatModifierType["StatModifierType<br>스탯 변경 데이터 타입"]{
        Constant // 캐릭터 기본수치
        Add // 더하기
        Multiply // 곱하기
    }
    
    class EntityStat["EntityStat<br>개별 스탯, 장비와 아이템에 의한 최종 스탯 계산"]{
        +GetValue() int // 최종 수치
        +GetOriginValue() int // 원본 수치 
        +AddModifier(string key, StatModifierType modiType, int value) void
        // AddModifier의 key는 해쉬코드 등 스탯 변경 요청의 발원지를 구분할 수 있는 고유값을 사용
        +RemoveModifier(string key) void
    }

    StatModifierType <-- EntityStat
    
    StatType "0..*"<--"1" EntityStatsComponent
    
    EntityStat "0..*"<--"1" EntityStatsComponent
    class EntityStatsComponent["EntityStatsComponent:Monobehaviour<br>Entity의 스탯 컴포넌트"]{
        +Stats : Dictionary~StatType, EntityStat~
        -StatsTableEntry : EntityStatsTableEntry
        // EntityStatsTableEntry의 수치는 AddModifier->Constant 타입으로 반영
        +GetStat(StatType type) int
    }
    
    class EntityStatsTableEntry["EntityStatsTableEntry<br>스탯 테이블 데이터"]{
        +Id : int
        +MaxHp : int
        +Attack : int
        +Defense : int
        +MoveSpeed : int
    }

    class PlayerConfigTableEntry["PlayerConfigTableEntry<br>플레이어 테이블 데이터"]{
        +Id int
        +Name string
        +Description string
        +GameObjectKey string // 게임오브젝트 주소
        +StartWeaponId int // 시작 무기 TableEntry Id
        +Skill1Id int // 1번 스킬 TableEntry Id
        +Skill2Id int // 2번 스킬 TableEntry Id
        +StatsId int // 스텟 EntityStatsTableEntry.Id
    }
    
    class EnemyConfigTableEntry["EnemyConfigTableEntry<br>적 테이블 데이터"]{
        +Id int
        +Name string
        +GameObjectKey string // 적 프리팹 주소
        +AIType string // 행동 타입
        +StatsId int // 스텟 EntityStatsTableEntry.Id
    }

    EntityStatsComponent --> EntityStatsTableEntry  : 테이블 데이터에 기반하여 기본 스탯 설정
    EntityStatsTableEntry <.. PlayerConfigTableEntry : Table ID로 간접 참조
    EntityStatsTableEntry <.. EnemyConfigTableEntry : Table ID로 간접 참조
```


- 스탯 클래스 구분
    - `EntityStatsTableEntry` : 데이터 테이블로 작성하는 개체별 '기본 스탯'
    - `EntityStatsComponent` : Entity마다 개별로 존재하는 스탯 컴포넌트

- Entity 스탯 초기화
    1. `PlayerConfigTableEntry`, `EnemyConfigTableEntry`에서 스탯 ID를 가져온다
    2. `ITableRepository`에서 ID로 조회해 `EntityStatsTableEntry`를 가져온다
    3. 게임오브젝트에 `StatsComponent`를 AddComponent 하고 가져온 `EntityStatsTableEntry`을 삽입, `StatType.Constant` 타입으로 StatModifier을 추가
- Entity 스탯 반영 방법
    - 스택 구조로 최종 스탯값을 결정한다
    - 아이템, 스킬 등에 의해 각 `EtntiyStat`들에 StatModifer들이 추가된다 
    - Constant, Add, Multiply
      - Constant -> 캐릭터의 기본 스텟 값
      - Add -> 수치 더하기
      - Multiply -> 수치 곱하기
        - 100% 단위를 기준으로 한다, 예: 50% -> 0.5배, 200% -> 2배
      - 값 연산시엔 기본값 -> 더하기 -> 곱하기 순으로 연산한다
      - 최종 값들은 한번 연산한후 캐싱해두고 사용, Add,Remove등으로 값 변동이 일어나게 되면 다시 계산한다
        - 더티 플래그 패턴을 사용한다