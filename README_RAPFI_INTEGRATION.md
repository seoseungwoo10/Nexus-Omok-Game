# 🚀 Rapfi Engine 통합 완료!

## ✅ 구현 완료 사항

### 핵심 기능
1. ✅ **Gomocup 프로토콜 구현** (`GomocupProtocol.cs`)
   - START, INFO, BEGIN, TURN, BOARD, END 명령 지원
   - Rapfi 엔진 전용 옵션 정의

2. ✅ **RapfiEngineController** (`RapfiEngineController.cs`)
   - 프로세스 간 통신 (Process I/O)
   - 비동기 명령 전송 및 응답 처리
   - 렌주 룰 자동 설정
   - 5단계 강도 레벨 지원

3. ✅ **RapfiAI** (`RapfiAI.cs`)
   - IAIPlayer 인터페이스 구현
   - 비동기 수 계산
   - 자동 폴백 메커니즘

4. ✅ **AI 팩토리 업데이트** (`AIPlayerFactory.cs`)
   - 비동기 AI 생성 (`CreateAsync`)
   - 자동 엔진 검색 (AVXVNNI > AVX2 > SSE)
   - RAPFI/ 및 Engines/ 폴더 지원

5. ✅ **UI 업데이트**
   - AIDifficultyWindow - Rapfi 선택 옵션 추가
   - 5단계 강도 선택 ComboBox
   - 확인 다이얼로그

6. ✅ **게임 설정** (`GameSettings.cs`)
   - Rapfi 강도 설정 추가

7. ✅ **Enum 업데이트** (`Enums.cs`)
   - AIDifficulty.Rapfi 추가
   - RapfiStrength enum 추가

---

## 📁 파일 구조

```
Nexus-Omok-Game/
├── RAPFI/ # Rapfi 엔진 폴더 (필수!)
│   ├── pbrain-rapfi_windows_avx2.exe    # AVX2 버전 (기본)
│   ├── pbrain-rapfi_windows_avxvnni.exe # AVXVNNI 버전 (권장)
│   ├── pbrain-rapfi_windows_sse.exe     # SSE 버전 (구형 CPU)
│   ├── model_renju_v2.nnue     # 렌주 룰 신경망 (필수!)
│   └── config.toml# 엔진 설정 파일
├── GomocupProtocol.cs  # ⭐ 신규
├── RapfiEngineController.cs    # ⭐ 신규
├── RapfiAI.cs      # ⭐ 신규
├── Enums.cs   # ✏️ 업데이트
├── AIPlayerFactory.cs       # ✏️ 업데이트
├── GameSettings.cs         # ✏️ 업데이트
├── MainWindow.xaml.cs   # ✏️ 업데이트
├── AIDifficultyWindow.xaml # ✏️ 업데이트
├── AIDifficultyWindow.xaml.cs           # ✏️ 업데이트
└── GameModeWindow.xaml.cs               # ✏️ 업데이트
```

---

## 🎮 사용 방법

### 1. Rapfi 엔진 다운로드

> ⚠️ **중요**: 게임을 실행하기 전에 Rapfi 엔진을 다운로드해야 합니다!

#### 자동 다운로드 (권장)
1. https://github.com/dhbloo/rapfi/releases
2. 최신 `Rapfi-engine.7z` 다운로드
3. 압축 해제
4. 다음 파일들을 `RAPFI/` 폴더에 복사:
   - `pbrain-rapfi_windows_avx2.exe`
   - `pbrain-rapfi_windows_avxvnni.exe` (선택)
   - `pbrain-rapfi_windows_sse.exe` (선택)
   - `model_renju_v2.nnue` ← **필수!**
   - `config.toml`

#### 필수 파일
```
RAPFI/
├── pbrain-rapfi_windows_avx2.exe  ← 최소 1개 이상 필요
├── model_renju_v2.nnue  ← 필수!
└── config.toml          ← 필수!
```

### 2. 게임 실행

1. Nexus Omok Game 실행
2. "AI 대전" 선택
3. "🚀 Rapfi Engine (World Class)" 선택
4. 강도 레벨 선택:
   - 🌱 초급 (Beginner) - 승률 30%
   - 📚 중급 (Intermediate) - 승률 60%
   - ⚡ 고급 (Advanced) - 승률 80%
   - 🎯 마스터 (Master) - 승률 90%
   - 👑 그랜드마스터 - 승률 95% (프로 수준)
5. "확인" 클릭
6. 게임 시작!

---

## 🔧 트러블슈팅

### 문제 1: "Rapfi engine not found" 오류

**원인**: RAPFI/ 폴더에 엔진 파일이 없음

**해결**:
```
1. RAPFI 폴더 확인
2. pbrain-rapfi_windows_avx2.exe 파일 존재 확인
3. model_renju_v2.nnue 파일 존재 확인
4. config.toml 파일 존재 확인
```

### 문제 2: 엔진 초기화 실패

**원인**: CPU가 AVX2를 지원하지 않음

**해결**:
```
1. SSE 버전 다운로드
2. pbrain-rapfi_windows_sse.exe 파일을 RAPFI 폴더에 복사
3. 게임 재시작
```

### 문제 3: AI 응답이 너무 느림

**원인**: 강도 레벨이 너무 높음

**해결**:
```
1. 낮은 강도 레벨 선택 (Beginner 또는 Intermediate)
2. 게임 재시작
```

---

## 📊 강도 레벨 상세

| 레벨 | 탐색 깊이 | 시간 제한 | 수 선택 범위 | 예상 승률 (AI) | 추천 대상 |
|------|----------|----------|------------|---------------|-----------|
| 🌱 Beginner | 8 | 1초 | 2칸 | 30% | 오목 입문자 |
| 📚 Intermediate | 12 | 2초 | 3칸 | 60% | 일반 플레이어 |
| ⚡ Advanced | 16 | 5초 | 3.5칸 | 80% | 숙련된 플레이어 |
| 🎯 Master | 무제한 | 10초 | 전체 | 90% | 고급 플레이어 |
| 👑 Grand Master | 무제한 | 30초 | 전체 | 95% | 프로 수준 연습 |

---

## 🌟 주요 기능

### Rapfi Engine의 장점
- ✅ **세계 최강 수준**: Gomocup 2023 대회 우승 엔진
- ✅ **완벽한 렌주 룰**: 금수(3-3, 4-4, 장목) 정확히 처리
- ✅ **NNUE 신경망**: 최신 Mix7 구조로 프로 수준 평가
- ✅ **5단계 강도**: 초보자부터 프로까지 모두 만족
- ✅ **무료 오픈소스**: MIT 라이선스

### 자동 기능
- ✅ CPU에 맞는 엔진 자동 선택 (AVXVNNI > AVX2 > SSE)
- ✅ 렌주 룰 자동 설정
- ✅ 오류 시 랜덤 수 폴백

---

## 🔍 기술 세부사항

### Gomocup 프로토콜
```csharp
// 초기화
START 15       // 15x15 보드
INFO rule 4       // 렌주 룰
INFO timeout_turn 5000  // 5초 제한

// 게임 진행
BEGIN     // AI가 첫 수
TURN 7,7 // 상대가 (7,7)에 둠

// 응답
7,8           // AI가 (7,8)에 둠

// 종료
END
```

### CPU 명령어 세트
| 버전 | CPU 요구사항 | 속도 |
|------|-------------|------|
| AVXVNNI | Intel 12세대+ | ⭐⭐⭐⭐⭐ |
| AVX2 | 2013년 이후 | ⭐⭐⭐⭐ |
| SSE | 구형 CPU | ⭐⭐⭐ |

---

## 📚 참고 자료

### 공식 문서
- Rapfi GitHub: https://github.com/dhbloo/rapfi
- Discord: https://discord.gg/7kEpFCGdb5
- 웹 버전: https://gomocalc.com

### 프로토콜
- Piskvork: http://petr.lastovicka.sweb.cz/protocl2en.htm
- Gomocup: https://gomocup.org/

### 관련 문서
- [PRD_RAPFI.md](PRD_RAPFI.md) - 완전한 기술 문서
- [RAPFI/README_RAPFI_EASY_GUIDE.md](RAPFI/README_RAPFI_EASY_GUIDE.md) - 엔진 사용 가이드

---

## 🎯 다음 단계 (선택사항)

### Phase 6.1: 분석 기능 (향후)
- [ ] NBEST 다중 수 분석
- [ ] 실시간 평가값 표시
- [ ] 추천 수 UI 패널

### Phase 6.2: 고급 기능 (향후)
- [ ] 게임 리뷰
- [ ] 포지션 분석
- [ ] 데이터베이스 통합

---

## 💡 팁

### 초보자
- Beginner 또는 Intermediate 레벨 시작
- AI의 수를 관찰하며 학습
- 금수 룰 익히기

### 고급자
- Advanced 또는 Master 레벨 도전
- 다양한 전략 시도
- Grand Master로 실력 테스트

---

## 🐛 알려진 이슈

- Grand Master 레벨은 매우 강력하여 일반 사용자가 이기기 거의 불가능
- 엔진 초기화에 몇 초 소요 (정상)
- 첫 수 계산 시간이 더 걸릴 수 있음 (정상)

---

## 📝 라이선스

### Rapfi Engine
- MIT License
- Copyright (c) 2023-2025 dhbloo, hzyhhzy

### Nexus Omok Game
- MIT License
- Rapfi 통합 기능 포함

---

**즐거운 오목 되세요!** 🎮⚫⚪🚀

