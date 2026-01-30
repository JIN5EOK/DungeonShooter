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
* Entity의 Root 게임오브젝트에 부착하여 사용한다
### 현재 작성된 Entity 컴포넌트 종류

```mermaid
classDiagram
    class MovementComponent["MovementComponent<br>캐릭터 이동 기능 담당"]{
        +MoveSpeed : float
        +Direction : Vector2
    }
        
    class DashComponent["DashComponent<br>캐릭터 대시 기능 담당"]{
        +StartDash() void
    }
    class EntityStatsComponent["EntityStatsComponent<br>캐릭터 스탯 기능 담당"]{
        +GetValue(StatType statType) int
        +AddModifier(...) void 
        +RemoveModifier(...) void
        // Modifier => 아이템 등에 의한 스탯 변화 추가
    }
```

```mermaid
classDiagram
    class SkillComponent["SkillComponent<br>캐릭터 고유 스킬 담당"]{
        +UseSkill(int skillId, EntityBase target) UniTask~bool~
        +RegistSkill(int skillId) UniTask~bool~
        +UnregistSkill(int skillId) UniTask~bool~
    }
    class HealthComponent["HealthComponent<br>캐릭터 체력 기능 담당"]{ 
        +Hp : int
        +TakeDamage() void
        +Heal() void
        +Kill() void
    }
    class InteractComponent["InteractComponent<br>플레이어의 상호작용 기능 담당"]{ 
        +TryInteract() void
    }
```

### 작성 예정 컴포넌트

```mermaid
classDiagram
    class BehaviourTreeComponent["BehaviourTreeComponent<br>적 AI등 행동트리 담당"]{ }
    class EntityVisualComponent["EntityVisualComponent<br>애니메이션, 스프라이트 등 비주얼 담당"]
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
        Constant // StatTable의 기본수치
        Add // 더하기
        Multiply // 곱하기
    }
    
    class EntityStat["EntityStat<br>개별 스탯, 장비와 아이템에 의한 최종 스탯 계산"]{
        +GetValue() int // 최종 수치
        +GetOriginValue() int // 원본 수치 (Constant 값만)
        +AddModifier(object key, StatModifierType modiType, int value) void
        // Modifier key는 스탯 요청 발원지 오브젝트 사용
        +RemoveModifier(object key) void
    }

    class EntityStatsComponent["EntityStatsComponent:Monobehaviour<br>Entity의 스탯 컴포넌트"]{
        +Stats : Dictionary~StatType, EntityStat~
        -StatsTableEntry : EntityStatsTableEntry
        // EntityStatsTableEntry의 수치는 AddModifier->Constant 타입으로 반영
        +ApplyStatBonus(object key, StatBonus statBonus) void// 스탯 보너스 적용
        +RemoveStatBonus(object key) void// 스탯 보너스 제거
        +GetStat(StatType type) int // 최종 스탯 수치
        +GetStatOrigin(StatType type) int // 캐릭터 고유 스탯 수치
    }
    
    class StatBonus["StatBonus<br>아이템등의 스탯 보너스 수치"]{
        +HpAdd : int // 체력증가 수치
        +HpMultiply : int // 체력증가 곱비율
        ... // 그외 스탯 보너스 정보
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
    
    EntityStatsComponent ..> StatBonus : 아이템등의 스탯 보너스 수치
    StatModifierType "0..*"<--"1" EntityStat
    StatType "0..*"<--"1" EntityStatsComponent
    EntityStat "0..*"<--"1" EntityStatsComponent
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
    - 합산 구조로 최종 스탯값을 결정한다
    - 기본 스탯 정보, 아이템, 스킬 등에 의해 각 `EntityStat`들에 StatModifer들이 추가된다
    - 스탯 보너스 구조
      - `Constant` -> `EntityStatsTableEntry`에 의해 적용되는 기본 수치 
      - `Add`,`Multiply` -> 아이템등의 `StatBonus`에 의해 적용되는 더하기, 곱셈 보정
          - 100% 단위를 기준으로 한다, 예: 50% -> 0.5배, 200% -> 2배
      - `Constant` -> `Add` -> `Multiply` 순으로 스탯에 반영하여 최종 스탯을 계산해낸다
      - 최종 값들은 한번 연산한후 캐싱해두고 사용, Add,Remove등으로 값 변동이 일어나게 되면 다시 계산한다
        - 더티 플래그 패턴을 사용한다