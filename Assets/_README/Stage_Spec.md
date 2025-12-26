# 스펙

## 방
* `Stage`는 여러가지 `Room`이 모여서 만들어진다
    * `Room`간의 연결은 방을 노드, 문을 엣지로 사용하는 그래프를 사용해 표현한다
* `Room`에 진입
    * `Room` 내에 잠금 조건이 존재한다면 모든 문이 닫힌다
        * 문이 잠겼다면 카메라 범위를 방 안으로 제한한다, (카메라가 방 밖으로 빠져나가지 않도록 한다), 만약 카메라보다 방이 작거나 같다면 카메라의 위치는 방의 중앙 지점으로 고정된다
* `Room` 클리어
    * 잠금 조건을 모두 해결하였다면 (적 모두 제거, 스위치 누르기 등) 방을 클리어 한 것으로 처리하고 방문이 열린다

### 방 생성방식
* `RoomData`를 기반으로 `Room`을 생성한다
* `RoomData`는 일종의 설계도 클래스로 직렬화하여 저장하고 게임 중 역직렬화 해 방 생성이 가능하도록 한다, 에디터에서 타일맵과 오브젝트를 배치한 후 이를 읽어들여 저장한다

## 다이어그램

### 방 진입 -> 클리어 흐름도
```mermaid
graph TD;
    Start((방 진입))
    Start --> A{방에 몬스터 같은 잠김 조건이 있는가?}
    A --> |Yes| B[카메라를 방 내부로 고정 + 문 잠김]
    A --> |No| E
    B --> C{잠김 조건이 해소되었나?}
    C --> |No| C
    C --> |Yes| D[카메라 고정 해제]
    D --> E[문 열림]
```

### 클래스 다이어그램

스테이지와 방
```mermaid
classDiagram
    class Stage {
        +rooms : Dictionary< int, Room > // int == id
        +AddRoom(RoomData roomData, Vector2 position) 
        void
        +ConnectRooms(int, int, Direction) void
        +GetRoom(int) Room
    }
    class Room {
        +id : int
        +roomData readonly RoomData 
        +position : Vector2Int
        +isCleared : bool
        +connections : Dictionary< Direction, int > 
        // 방들간 연결 표현, Direction은 문이 위치하는 방향
    }
    class RoomData {
        -assetAddresses List< string >
        // 타일 팔레트, 게임 오브젝트등 동적 로드에 사용되는 주소들
        +tiles : List< TileLayerData >
        +objects : List< ObjectData >
    }

    class TileLayerData{
        +int index
        // index == RoomData의 어드레스 컬렉션상의 인덱스
        // 타일 팔레트 어드레스에 해당
        // 문자열 주소의 중복을 최소화 해서 용량 아끼기 위함
        +palettePosition Vector2Int
        // 팔레트상의 위치
        +position Vector2Int
        // 방 생성시 배치될 위치
    }

    class ObjectData{
        +int index
        // index == RoomData의 어드레스 컬렉션상의 인덱스
        // 오브젝트 어드레스에 해당
        // 문자열 주소의 중복을 최소화 해서 용량 아끼기 위함
        +position Vector2
        +rotation Quaternion
        // 방 생성시 배치될 위치와 회전값
    }

    Stage "1" -- "1..*" Room
    Room --> RoomData
    RoomData "1"-->"1..*" TileLayerData
    RoomData "1"-->"1..*" ObjectData
```
