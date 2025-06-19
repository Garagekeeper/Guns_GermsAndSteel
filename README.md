[![영상 설명](https://img.youtube.com/vi/X_B_yyo6mb8/0.jpg)](https://www.youtube.com/watch?v=X_B_yyo6mb8)
>이미지 클릭시 데모 영상으로 이동합니다.
# 🎮 프로젝트 소개

**Guns, Germs & Steel**는 2D 로그라이크 게임인 **The Binding of Isaac**의 모작 프로젝트 입니다.

게임 플레이 경험을 바탕으로 Unity 환경에서 재구성 하였습니다.

---

## ✨ 주요 특징

-  **시드 기반의 맵 생성**: 유사 난수 생성기 + BFS를 통해서 시드 기반으로 맵을 생성합니다.  
-  **FSM을 이용한 몬스터 AI 관리**: 상태를 기반으로 몬스터의 행동 구현
-  **맵 에디팅**:  Tilemap과 Prefab을 이용해 게임에 사용될 방의 레벨을 디자인
-  **간편한 시스템 디자인**:  Unity 외부(CSV) 파일을 통한 시스템 디자인
    - 아이템, 아이템 풀, 아이템 배열, 몬스터/보스,  방의 Pool 을 게임 외부에서 손쉽게 수정 가능
---

## 🛠️ 사용 기술 스택

| 항목       | 내용                     |
|------------|--------------------------|
| 게임 엔진   | Unity (2022 이상)        |
| 프로그래밍 | C#                       |
| 버전 관리   | Git, GitHub              |
| SFX, GFX   | 게임사에서 제공하는 추출툴을 사용해 원작의 리소스를 사용 |

---

## 📁 프로젝트 구조
```
Guns_GermsAndSteel/
├── Assets/ 
│  ├── @Resources/ #사용될 리소스
│  │  ├── Animation
│  │  ├── Data
│  │  ├── Font
│  │  ├── Prefabs
│  │  ├── Sounds
│  │  ├── Sprites
│  │  ├── Tiles
│  │  └── UI
│  ├── @Scenes/  # 유니티 씬 파일
│  ├── @Scripts/ # 게임 로직 관련 스크립트
├── ProjectSettings/
├── Release/
└── README.md
```
## 🚀 실행 방법

1. 프로젝트를 Unity에서 열고 싶으면 Unity 2022 이상 버전으로 프로젝트를 엽니다
2. Assets//@Scenes/Title.unity 씬을 실행합니다
- Release 디렉토리만 다운받아서 게임을 실행할 수 있습니다.
