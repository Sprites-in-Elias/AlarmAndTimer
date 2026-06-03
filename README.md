# 간단한 알람 & 타이머

기존 대형 웹 포털사이트 등에서 구색 맞추기 용으로 만들어 주던 데스크탑 용 알람 혹은 타이머 시계, 그리고 윈도우 기본 시계 앱 등이 제공하던 쓸데없이 크고 불편한 UI 와 짜증나는 사용자 경험을 유발하는 모든 요소를 제거하고  
최소한의 UI, 필수적인 기능을 집약하여 제작한 데스크톱 앱입니다.

- 최신버전 다운로드 링크 : <https://github.com/Sprites-in-Elias/AlarmAndTimer/releases/latest>

## 기존 비슷한 제품의 한계
- 보통 컴퓨터에서 타이머가 필요하면 웹브라우저에서 타이머를 검색하여 사용함
  - 검색결과 가장 유용한게 네이버에서 제공하는 엄청 옛날에 만들어진 타이머..
    - 하지만 항상 위를 제공하지 않음 <= 타이머 특성상 빈번히 들여다 봐야함
    - 다양한 볼륨이나 사운드 옵션을 제공하지 않아 들리지 않거나 너무 크게 들림
    - 알람 목록을 제공하지 않아 연속적인 타이머가 필요할 시 여러개의 창을 띄어야 함
    - 브라우저를 아무리 작게 줄여도 내가 원하는 만큼 작게 줄일 수 없어 모니터를 차지함
    - and so on..
- 윈도우 기본 제공 시계앱
  - 그냥 뭐 업무용으로 쓸 수가 없는 수준이라 보면된다.
  - 일단 뭐 UI 등이 너무 커서 작업공간을 다 차지하고 사운드 옵션도 없고 태스크 메뉴에서 숨긴다던가 하는 편의 기능도 없고 이런 안좋은것에도 불구하고 예쁘지도 않다.
- 다른 군소 앱들도 개인화된 옵션이나 최소화된 UI등을 제공하지 않는다.

## 이 프로젝트가 제공하는 이점
- 모든 요소를 최소화하여 '남은시간, 종료시간, 알람제목, 삭제, 이동'만 남겼습니다.
  - 작업공간을 최소화 하여 사용할 수 있고 창을 줄여 모니터 한군데 짱박아둘 수 있습니다.
    -또한 항상 위를 제공해서 계속 남은 시간을 확인 할 수 있음
![5](https://github.com/Sprites-in-Elias/AlarmAndTimer/blob/master/screenshot/5.png?raw=true)

- 모든 컨트롤의 시작은 오른쪽 클릭입니다. 하지만 이 설명서를 읽지 않아도 직관적으로 사용할 수 있게 디자인 하였습니다.
  - 모든 기능을 컨텍스트 메뉴에 몰아 넣어 한눈에 모든 기능을 파악할 수 있습니다.
![4](https://github.com/Sprites-in-Elias/AlarmAndTimer/blob/master/screenshot/4.png?raw=true)

- 다양한 윈도우 컨트롤을 제공합니다.
  - 항상 위
  - 트레이 아이콘
  - 테스크바에서 프로세스 숨김
  - 최소화 및 닫기

- 다양한 개인화 설정을 제공합니다
  - 현재 시간 표시 여부
  - 색상 테마 선택
  - 다양한 알람 방식
  - 앱 깜박임 및 자동 활성화 기능
  - 커스텀 사운드 적용, 볼륨, 반복여부
  - 폰트 크기
  - 다국어
![2](https://github.com/Sprites-in-Elias/AlarmAndTimer/blob/master/screenshot/2.png?raw=true)

- 자주 사용하는 타이머 및 알람 목록을 스크립트화 및 스크립트 편집기 제공
  - 스크립트를 불러와 자주 사용하는 알람 혹은 타이머를 한번에 불러올 수 있습니다.
  - 스크립트 작성을 위한 스크립트 편집기도 제작하였습니다.
![1](https://github.com/Sprites-in-Elias/AlarmAndTimer/blob/master/screenshot/1.png?raw=true)


- 일원화된 알람과 타이머 제작 방법
  - 타이머나 알람이나 결국 목적된 시간까지 걸리는 시간을 구하는 공통점에서 같은 목록에 두는게 맞다고 봄
![3](https://github.com/Sprites-in-Elias/AlarmAndTimer/blob/master/screenshot/3.png?raw=true)


[![C#](https://img.shields.io/badge/C%23-WPF-blue.svg)](https://dotnet.microsoft.com/apps/wpf)

## 설치 방법
1. zip 파일을 다운받기
   - zip 파일을 다운받아서 압축을 푼 폴더를 Program Files 폴더에 넣고 exe 파일만 바로가기로 만들어 사용하십시오
2. exe 파일을 받아서 설치하기
   - 설치시 위협이 있을 수 있다는 경고가 뜰 수 있는데 개인개발자라 그렇다 못 믿으면 어쩔 수 없는거고,, 아님 소스코드 받아서 직접 빌드ㄱㄱ

## 릴리즈 파일 바이러스 검사 결과




### 프로젝트 빌드
---

그냥 하던대로 빌드를 하면 됩니다.

```bash
cd ./AlarmAndTimer
dotnet publish -c Release -r win-x64 --self-contained true
```
이후 bin 아래 어딘가에 publish 폴더를 찾아 꺼내 쓰기  
(하지만 릴리즈 파일을 배포하는데 굳이 이런 짓을 할 필요는 없을 듯)
