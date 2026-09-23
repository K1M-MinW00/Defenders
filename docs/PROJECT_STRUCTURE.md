# 프로젝트 폴더 구조

Unity가 관리하는 `.meta` 파일은 원본 파일과 함께 이동하며, GUID 기반 Scene/Prefab 참조를 유지한다.

## Assets

- `Animations`: 애니메이션 클립과 컨트롤러
- `Art`: Sprite, Tile, Material 등 시각 리소스
  - `Environment`: 환경 및 타일 리소스
  - `ThirdParty`: 외부 에셋 원본
- `Audio/Clips`: BGM과 SFX 원본 음원
- `GameData`: Scene, Prefab 또는 다른 SO가 직접 참조하는 ScriptableObject 인스턴스
- `Resources/GameData`: `Resources.Load`로 경로 로드하는 최소 런타임 데이터
  - `Configs`: 전역 설정
  - `Catalogs`: 아이콘과 진급 진행도 등 공용 카탈로그
  - `Items`, `Units`, `Stages`: 런타임 조회 대상 데이터
- `Prefabs`: 재사용 GameObject 프리팹
- `Scenes`: Unity Scene
- `Scripts`: C# 소스

`Resources`에는 경로 로드가 반드시 필요한 에셋만 둔다. Sprite나 Tile처럼 Scene/Prefab/SO에서 GUID로 직접 참조되는 에셋은 `Art`에 둔다. 새 데이터는 가능한 한 직접 참조하고, 전역 부트스트랩이나 키 기반 카탈로그가 필요한 경우에만 `Resources/GameData`에 추가한다.

## Scripts

- `Core`: 특정 게임 기능에 의존하지 않는 기반 기능(오디오, 부트스트랩, 풀링, 씬 전환, 설정)
- `Data`: DTO, enum, ScriptableObject 타입 정의. 에셋 인스턴스는 두지 않는다.
- `Features`: 화면 또는 게임 기능 단위 구현
  - 각 기능 안에서 필요에 따라 `Controllers`, `Models`, `Runtime`, `Services`, `UI`로 구분한다.
- `Services`: 여러 화면/기능에서 공유되는 애플리케이션 서비스와 저장소
- `UI`: 기능에 종속되지 않는 공용 UI와 레이아웃 도구
- `Editor`: 빌드에 포함되지 않는 에디터 전용 코드

폴더 이름이 클래스 계층을 강제하지는 않는다. Unity 직렬화 호환성을 위해 기존 타입의 네임스페이스는 별도 마이그레이션 없이 변경하지 않는다.

## 외부 패키지

`Firebase`, `GoogleMobileAds`, `ExternalDependencyManager`, `Plugins` 등 공급자가 관리하는 코드는 프로젝트 코드 구조에 맞춰 이동하거나 수정하지 않는다.
