# 간단한 알람 & 타이머

기존 대형 웹 포털사이트 등에서 구색 맞추기 용으로 만들어 주던 데스크탑 용 알람 혹은 타이머 시계, 그리고 윈도우 기본 시계 앱 등이 제공하던 쓸데없이 크고 불편한 UI 와 짜증나는 사용자 경험을 유발하는 모든 요소를 제거하고  
최소한의 UI, 필수적인 기능을 집약하여 제작한 데스크톱 앱입니다.

- 최신버전 다운로드 링크 : <https://github.com/Sprites-in-Elias/AlarmAndTimer/releases/latest>

## 특징
- 모든 요소를 최소화하여 '남은시간, 종료시간, 알람제목, 삭제, 이동'만 남겼습니다.





[![C#](https://img.shields.io/badge/C%23-WPF-blue.svg)](https://dotnet.microsoft.com/apps/wpf)


### 프로젝트 빌드
---

그냥 하던대로 빌드를 하면 됩니다.

```bash
cd ./AlarmAndTimer
dotnet publish -c Release -r win-x64 --self-contained true
```
이후 bin 아래 어딘가에 publish 폴더를 찾아 꺼내 쓰기  
(하지만 릴리즈 파일을 배포하는데 굳이 이런 짓을 할 필요는 없을 듯)
