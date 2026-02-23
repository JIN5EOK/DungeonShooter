# 개요
> 플레이어, 적 등 게임 내의 Entity 개체들에 대한 설계 문서입니다.

---
## 컴포넌트 조합 기반 설계
* 모든 Entity들의 공통 인터페이스 + 기능 역할을 담당하는 `EntityBase` 컴포넌트와 다수의 기능 컴포넌트로 구성
* 기능 추가는 단일 책임 원칙에 기반한 단일 기능 컴포넌트 추가를 통한 확장 구조를 통해 수행
	* 예시 
      * 캐릭터 이동을 담당하는 `MovementComponent`
      * 체력 연산을 담당하는 `HelathComponent`
      * AI를 담당하는 `AIComponent`
* Entity들의 프리팹 자체에는 기능 컴포넌트를 달아두지 않음, 런타임 조합이나 로직에 따라 어떤 프리팹이라도 플레이어, 적으로 만들 수 있도록 함
  * Player / Enemy에 필요한 컴포넌트와 초기화, 레이어,태그 설정은 `PlayerFactory`/`EnemyFactory`에서 캐릭터를 생성하며 수행
* 각 컴포넌트간 참조관계는 `EntityLifeTimeScope`를 통한 의존성 주입을 통해 해결한다

## 스탯, 스킬 등 데이터 관리, 주입구조
```mermaid
classDiagram
	class EntityBase{
		+EntityContext : IEntityContext
		+SetContext(EntityContext context)
	}
	
	class EntityContext{
		+InputContext : EntityInputContext 
		// 이동, 공격 등 행동 결정에 필요한 입력정보, 플레이어의 경우 키입력으로 조작
		// 적의 경우 인공지능 컴포넌트로 조작
		+Stat : IEntityStats // 최대 체력 등 스탯 수치 관련 클래스
		+Status : IEntityStatus // 현재 체력 등 현재 상태 수치
		+Skill : IEntitySkills // 지닌 스킬 관련 클래스
		// +Inventory : IInventory // 인벤토리 관련 클래스
	}
	
	EntityContext <-- EntityBase : 보유
	EntityBase <-- PlayerFactory : 생성시 EntityContext 주입
	EntityBase <-- EnemyFactory : 생성시 EntityContext 주입
```
- 플레이어, 적들의 스탯과 스킬 관리 로직이 다르므로 팩토리에서 직접 컨텍스트를 주입한다

### 인벤토리 동작 구조
``` mermaid
classDiagram
	class Inventory["Inventory<br> 아이템 장착, 조건판단, 아이템 장착시 스탯 반영등 기능적인 부분 담당"]{
		-status : IEntityStatus
		-skill : IEntitySkills
		+Items : IReadOnlyCollection<Item>
		+bool AddItem(Item item)
		+void RemoveItem(Item item)
		+bool EquipItem(Item item)
	}
	
	class InventoryModel["InventoryModel<br> 아이템,추가,삭제 등 데이터 컬렉션의 기능 담당"]{
		+Items : IReadOnlyCollection<Item>
		+bool AddItem(Item item)
		+void RemoveItem(Item item)
	}
	IInventory <|.. Inventory
	Inventory --> InventoryModel : 인벤토리 모델에 반영
	
```
### 관련 문서
- [스탯시스템_설계.md](%EC%8A%A4%ED%83%AF%EC%8B%9C%EC%8A%A4%ED%85%9C_%EC%84%A4%EA%B3%84.md)
- [스킬시스템_설계.md](%EC%8A%A4%ED%82%AC%EC%8B%9C%EC%8A%A4%ED%85%9C_%EC%84%A4%EA%B3%84.md)

---
