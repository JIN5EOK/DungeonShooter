# 개요
> UI 관리를 위한 UIManager, UI를 상속받는 UIBase들에 대한 설계입니다

### UI 구조도
```mermaid
classDiagram
    class UIType{
        <<Enum>>
        HudUI
        PopupUI
    }
    
    class UIManager["UIManager<br>UI생성 및 게임오브젝트 계층구조 담당"]{
        +CreateUI(string key, bool isUnique) UIBase
        +RemoveUI(UIBase uiBase) UIBase
        +GetOrder(UIBase uiBase) int // 정렬순서 조회
        +SetOrder(UIBase uiBase, int order) void // 정렬순서 조절
    }
    
    class UIBase["UIBase<br>UI들의 부모 클래스"]{
        <<abstract>>
        +OnShow event Action
        +OnHide event Action
        +OnDestroy event Action
        +Show() void
        +Hide() void
        +Destroy() void
    }
    class PopupUI["PopupUI<br>일반적인 UI, 버튼등 상호작용 가능"]{
        
    }
    class HudUI["HudUI<br>스크린 영역에 표시되는 정보표시 UI"]{

    }
    
    UIManager --> UIType : UI 타입 구분
    UIManager "1"-->"0..*" UIBase
    UIBase <|-- PopupUI
    UIBase <|-- HudUI
    PopupUI <|-- 상세UI구현
    HudUI<|-- 상세UI구현
```
* `UIManager` -> UI생성 및 게임오브젝트 계층구조 담당
  * UI 오브젝트 생성 
  * `UIType`별 캔버스 및 계층구조 생성
    * `UIType`별로 캔버스를 생성한다 
    * 캔버스간 정렬 순서는 UIType에 정의된 순서를 따른다 (HudUI < PopupUI)
* `CreateUIAsync` 의 `isUnique`
  * UI를 불러올때 동일한 UI를 사용할건지 여부, 처음 불러올때 내부에 주소 : UI 형태로 캐시해두고 같은 요청이 있을때 같은 UI반환