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
        +Stats : EntityStats
        // 그 외 스탯 관련 연산이 있으면 사용
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

---
