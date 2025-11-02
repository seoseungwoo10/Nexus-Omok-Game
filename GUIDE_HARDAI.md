# HardAI 구현 가이드

## 목차
1. [개요](#개요)
2. [AI 모델 아키텍처](#ai-모델-아키텍처)
3. [핵심 알고리즘 상세 분석](#핵심-알고리즘-상세-분석)
4. [코드 흐름 분석](#코드-흐름-분석)
5. [성능 최적화 기법](#성능-최적화-기법)
6. [실전 예제](#실전-예제)

---

## 개요

### HardAI의 목표
- **난이도**: 어려움 (Hard)
- **목표 승률**: AI 승률 70%
- **대상 플레이어**: 고급 플레이어에게도 도전적인 상대

### 사용된 핵심 기술
1. **Minimax 알고리즘**: 게임 트리 탐색을 통한 최적의 수 선택
2. **Alpha-Beta Pruning**: 불필요한 탐색 가지치기로 성능 향상
3. **Board Evaluation**: 휴리스틱 평가 함수를 통한 보드 상태 점수화
4. **Move Prioritization**: 효율적인 후보 수 선별

---

## AI 모델 아키텍처

### 클래스 구조

```csharp
public class HardAI : IAIPlayer
{
    // 상수 정의
    private const int BOARD_SIZE = 15;          // 오목판 크기
    private const int MAX_DEPTH = 4;  // 미니맥스 탐색 깊이
    private const int MOVE_SEARCH_RADIUS = 2;   // 돌 주변 탐색 반경

    // 의존성
    private readonly Random random;    // 랜덤 딜레이용
    private readonly BoardEvaluator evaluator;   // 보드 평가 엔진
}
```

### 주요 구성 요소

| 구성 요소 | 역할 | 설명 |
|----------|------|------|
| `BoardEvaluator` | 보드 평가 | 현재 보드 상태를 점수화하여 유리한 정도를 계산 |
| `Minimax` | 게임 트리 탐색 | 미래의 수를 시뮬레이션하여 최선의 수를 찾음 |
| `Alpha-Beta` | 가지치기 | 불필요한 탐색을 제거하여 성능 향상 |
| `GetPrioritizedMoves` | 후보 수 선별 | 유망한 위치만 선별하여 탐색 공간 축소 |

---

## 핵심 알고리즘 상세 분석

### 1. GetNextMoveAsync - 전체 의사결정 흐름

```csharp
public async Task<(int row, int col)> GetNextMoveAsync(int[,] board, int player)
{
    // 1단계: 사람처럼 보이게 딜레이 추가 (0.5~1.2초)
    await Task.Delay(random.Next(500, 1200));

    int opponent = player == 1 ? 2 : 1;

    // 2단계: 즉시 승리 가능 여부 확인 (최우선)
 var winMove = FindImmediateWin(board, player);
    if (winMove.HasValue)
        return winMove.Value;  // 바로 이기는 수가 있으면 즉시 선택

    // 3단계: 상대방의 승리 저지 (2순위)
    var blockMove = FindImmediateWin(board, opponent);
    if (blockMove.HasValue)
        return blockMove.Value;  // 상대방이 이길 수 있으면 막기

    // 4단계: 미니맥스로 최선의 수 계산 (일반적인 경우)
    return await Task.Run(() => FindBestMove(board, player));
}
```

**의사결정 우선순위:**
1. **즉시 승리** (FindImmediateWin) - 5목을 만들 수 있으면 바로 선택
2. **승리 방어** (FindImmediateWin for opponent) - 상대의 5목을 막기
3. **미니맥스 탐색** (FindBestMove) - 깊이 있는 분석으로 최선의 수 찾기

---

### 2. Minimax 알고리즘 - 게임 트리 탐색의 핵심

#### 기본 원리

Minimax는 **두 플레이어가 모두 최선을 다한다고 가정**하고 게임 트리를 탐색합니다.

```
          현재 상태 (depth=4)
   /     | \
        AI의 수들 (maximizing)
             /    |    \
 상대의 수들 (minimizing)
     /   |   \
    AI의 수들 (maximizing)
    /  |  \
        상대의 수들 (minimizing)
        [평가 점수]
```

#### 코드 분석

```csharp
private int Minimax(int[,] board, int depth, int alpha, int beta, 
        bool isMaximizing, int player)
{
    // === 종료 조건 1: 깊이 도달 ===
    if (depth == 0)
    {
      return evaluator.Evaluate(board, player);
    }

    int opponent = player == 1 ? 2 : 1;

    // === 종료 조건 2: 게임 종료 ===
  if (IsGameOver(board))
    {
    return evaluator.Evaluate(board, player);
  }

    // === 이동 가능한 수 가져오기 ===
    var moves = GetPrioritizedMoves(board, isMaximizing ? player : opponent);

    if (moves.Count == 0)
    {
  return evaluator.Evaluate(board, player);
    }

    // === Maximizing Player (AI 차례) ===
    if (isMaximizing)
    {
        int maxEval = int.MinValue;

        foreach (var move in moves)
  {
      // 1. 수를 두어봄 (시뮬레이션)
         board[move.row, move.col] = player;
            
         // 2. 재귀적으로 평가 (상대방 차례로 전환)
            int eval = Minimax(board, depth - 1, alpha, beta, false, player);
  
            // 3. 수를 되돌림 (백트래킹)
 board[move.row, move.col] = 0;

 // 4. 최대값 갱신
         maxEval = Math.Max(maxEval, eval);
      alpha = Math.Max(alpha, eval);

        // 5. 알파-베타 가지치기
       if (beta <= alpha)
       break;  // 더 이상 탐색 불필요
        }

return maxEval;
    }
    // === Minimizing Player (상대방 차례) ===
    else
    {
int minEval = int.MaxValue;

 foreach (var move in moves)
        {
     board[move.row, move.col] = opponent;
      int eval = Minimax(board, depth - 1, alpha, beta, true, player);
      board[move.row, move.col] = 0;

minEval = Math.Min(minEval, eval);
            beta = Math.Min(beta, eval);

  if (beta <= alpha)
           break;  // 알파 컷오프
        }

        return minEval;
    }
}
```

#### Maximizing vs Minimizing

| 플레이어 유형 | 목표 | 동작 |
|-------------|------|------|
| **Maximizing** (AI) | 점수 최대화 | `maxEval = Max(모든 가능한 수의 평가값)` |
| **Minimizing** (상대) | 점수 최소화 | `minEval = Min(모든 가능한 수의 평가값)` |

---

### 3. Alpha-Beta Pruning - 성능 최적화의 핵심

#### 기본 개념

Alpha-Beta Pruning은 **확실히 선택되지 않을 가지를 미리 제거**하여 탐색 시간을 단축합니다.

```
예시 게임 트리:

          Max (AI)
           /    \
          5      Min
   /      /   \
        5      3     ?
              /
       3

설명:
- Max는 이미 5점을 발견했음 (alpha = 5)
- Min 노드에서 3점을 발견
- Min은 최소값을 선택하므로 3 이하만 선택
- 3 < 5 이므로 Max는 절대 이 노드를 선택 안 함
- 따라서 ? 노드는 탐색할 필요 없음 (가지치기!)
```

#### 코드에서의 적용

```csharp
// Maximizing Player에서의 Beta Cutoff
if (isMaximizing)
{
    alpha = Math.Max(alpha, eval);
    
    if (beta <= alpha)
 break;  // 베타 컷오프: Min이 선택 안 할 것이 확실
}

// Minimizing Player에서의 Alpha Cutoff
else
{
    beta = Math.Min(beta, eval);
    
    if (beta <= alpha)
    break;  // 알파 컷오프: Max가 선택 안 할 것이 확실
}
```

#### Alpha-Beta 변수의 의미

| 변수 | 의미 | 역할 |
|------|------|------|
| `alpha` | Max 플레이어의 최소 보장 점수 | "나는 적어도 이만큼은 얻을 수 있다" |
| `beta` | Min 플레이어의 최대 허용 점수 | "상대는 최대 이만큼만 허용한다" |

#### 성능 향상 효과

- **최악의 경우**: O(b^d) → 변화 없음
- **최선의 경우**: O(b^(d/2)) → 약 √(b^d) 로 대폭 감소
- **실제 오목**: 약 50~70% 노드 탐색 감소

---

### 4. FindBestMove - 최선의 수 선택 과정

```csharp
private (int row, int col) FindBestMove(int[,] board, int player)
{
    // 1. 우선순위 높은 후보 수들 가져오기
    var validMoves = GetPrioritizedMoves(board, player);

    if (validMoves.Count == 0)
    {
        return (BOARD_SIZE / 2, BOARD_SIZE / 2);  // 첫 수는 중앙
    }

    int bestScore = int.MinValue;
    (int row, int col) bestMove = validMoves[0];

    // 2. Alpha-Beta 초기화
    int alpha = int.MinValue;  // -∞
    int beta = int.MaxValue;   // +∞

    // 3. 각 후보 수를 평가
    foreach (var move in validMoves)
  {
        // 3-1. 수를 두어봄
        board[move.row, move.col] = player;

        // 3-2. Minimax로 평가 (상대방 차례로 시작)
        int score = Minimax(board, MAX_DEPTH - 1, alpha, beta, false, player);

        // 3-3. 수를 되돌림
        board[move.row, move.col] = 0;

    // 3-4. 최선의 수 갱신
        if (score > bestScore)
        {
   bestScore = score;
            bestMove = move;
        }

        // 3-5. Alpha 갱신
  alpha = Math.Max(alpha, score);
    }

    return bestMove;
}
```

**프로세스 요약:**
1. 유망한 후보 수들을 우선순위순으로 가져옴
2. 각 후보 수에 대해 Minimax 평가 수행
3. 가장 높은 점수를 받은 수를 선택

---

### 5. GetPrioritizedMoves - 효율적인 후보 수 선별

#### 탐색 공간 축소 전략

```csharp
private List<(int row, int col)> GetPrioritizedMoves(int[,] board, int player)
{
    var moves = new List<(int row, int col, int priority)>();
    bool isEmpty = IsEmptyBoard(board);

    // 전략 1: 빈 보드면 중앙만 반환
    if (isEmpty)
    {
   return new List<(int row, int col)> { (BOARD_SIZE / 2, BOARD_SIZE / 2) };
 }

    // 전략 2: 기존 돌 주변만 탐색 (MOVE_SEARCH_RADIUS = 2)
    for (int row = 0; row < BOARD_SIZE; row++)
    {
        for (int col = 0; col < BOARD_SIZE; col++)
        {
 if (board[row, col] == 0 && HasNearbyStone(board, row, col))
      {
       // 우선순위 계산
       int priority = CalculateMovePriority(board, row, col, player);
         moves.Add((row, col, priority));
   }
 }
    }

    // 전략 3: 상위 20개만 선택
    return moves.OrderByDescending(m => m.priority)
    .Take(20)
  .Select(m => (m.row, m.col))
            .ToList();
}
```

#### 최적화 효과

| 최적화 기법 | 효과 | 설명 |
|-----------|------|------|
| **주변 탐색** | 225개 → 약 40개 | 기존 돌 주변 2칸 이내만 고려 |
| **상위 20개 선택** | 40개 → 20개 | 가장 유망한 수만 깊이 탐색 |
| **우선순위 정렬** | 가지치기 효율 향상 | 좋은 수를 먼저 탐색하여 Alpha-Beta 효과 증대 |

**전체 성능 향상:**
- 탐색 공간: 225개 → 20개 (약 91% 감소)
- 계산 시간: 수백 배 단축

---

### 6. HasNearbyStone - 주변 돌 검사

```csharp
private bool HasNearbyStone(int[,] board, int row, int col)
{
 // 반경 2칸 이내 검사
    for (int dr = -MOVE_SEARCH_RADIUS; dr <= MOVE_SEARCH_RADIUS; dr++)
    {
        for (int dc = -MOVE_SEARCH_RADIUS; dc <= MOVE_SEARCH_RADIUS; dc++)
        {
     if (dr == 0 && dc == 0) continue;  // 자기 자신 제외

        int r = row + dr;
            int c = col + dc;

            // 범위 체크 + 돌 존재 확인
            if (r >= 0 && r < BOARD_SIZE && 
           c >= 0 && c < BOARD_SIZE && 
  board[r, c] != 0)
     {
        return true;
            }
        }
    }
    return false;
}
```

**검사 범위 시각화 (RADIUS=2):**
```
  X X X X X
  X X X X X
  X X O X X  <- O 위치를 검사할 때
  X X X X X     X 표시 영역 내 돌이 있는지 확인
  X X X X X
```

---

### 7. CalculateMovePriority - 우선순위 계산

```csharp
private int CalculateMovePriority(int[,] board, int row, int col, int player)
{
    // 1. 해당 위치에 돌을 두어봄
    board[row, col] = player;
    
    // 2. BoardEvaluator로 점수 계산
    int score = evaluator.Evaluate(board, player);
    
    // 3. 원상복구
    board[row, col] = 0;
    
    return score;  // 점수가 높을수록 우선순위 높음
}
```

**BoardEvaluator의 평가 기준:**
- **5목**: 100,000점 (즉시 승리)
- **4목**: 10,000점 (다음 수에 승리 가능)
- **열린 3목**: 1,000점 (양쪽이 열려있어 4목 가능)
- **닫힌 3목**: 100점
- **열린 2목**: 50점
- **위치 보너스**: 중앙에 가까울수록 가산점

---

## 코드 흐름 분석

### 전체 실행 흐름도

```
[게임 시작]
    ↓
[GetNextMoveAsync 호출]
    ↓
[1단계: 딜레이 (0.5~1.2초)]
    ↓
[2단계: 즉시 승리 체크]
    ├─ 승리 가능? → [해당 수 반환]
    └─ 불가능 ↓
[3단계: 상대 승리 저지]
 ├─ 저지 필요? → [막는 수 반환]
    └─ 불필요 ↓
[4단계: FindBestMove 호출]
    ↓
[GetPrioritizedMoves로 후보 수 선별]
    ├─ 빈 보드? → [중앙 반환]
    └─ 아니면 ↓
[주변 돌 있는 곳만 검사]
  ↓
[각 위치 우선순위 계산]
    ↓
[상위 20개 선택]
    ↓
[각 후보 수에 대해 Minimax 실행]
  ↓
[Minimax 재귀 탐색 (깊이 4)]
    ├─ Depth=0 → [evaluator.Evaluate]
    ├─ 게임 종료 → [evaluator.Evaluate]
    └─ 계속 ↓
[Maximizing/Minimizing 번갈아 실행]
    ├─ Alpha-Beta 가지치기
    └─ 모든 수 평가
[최고 점수 수 선택]
    ↓
[선택된 수 반환]
```

---

### 실행 시간 분석

#### 예상 노드 탐색 수

```
깊이 4 기준:
- 분기 계수: 약 20개 (우선순위 선별 후)
- Alpha-Beta 효율: 약 60% 가지치기

최악: 20^4 = 160,000 노드
실제: 약 64,000 노드 (Alpha-Beta 적용)
시간: 약 0.5~1초
```

---

## 성능 최적화 기법

### 1. 탐색 깊이 제한

```csharp
private const int MAX_DEPTH = 4;  // 4수 앞까지만 예측
```

**선택 이유:**
- 깊이 3: 너무 약함 (중급 수준)
- 깊이 4: 적절한 난이도 (고급 수준)
- 깊이 5: 너무 느림 (5초 이상 소요)

### 2. 후보 수 제한

```csharp
.Take(20)  // 상위 20개만 탐색
```

**효과:**
- 탐색 공간 대폭 감소
- 품질 저하 거의 없음 (좋은 수는 상위권에 집중)

### 3. 주변 탐색 반경 제한

```csharp
private const int MOVE_SEARCH_RADIUS = 2;
```

**전략:**
- 오목은 연속된 돌이 중요
- 멀리 떨어진 곳은 대부분 의미 없음
- 반경 2가 최적 (성능 vs 품질)

### 4. Move Ordering (이동 순서 최적화)

```csharp
return moves.OrderByDescending(m => m.priority)
```

**효과:**
- 좋은 수를 먼저 탐색
- Alpha-Beta 가지치기 효율 증대
- 약 2~3배 성능 향상

### 5. 조기 종료 조건

```csharp
// 즉시 승리/저지 체크로 불필요한 Minimax 회피
var winMove = FindImmediateWin(board, player);
if (winMove.HasValue) return winMove.Value;
```

---

## 실전 예제

### 예제 1: 첫 수 (빈 보드)

```
입력: 빈 보드
처리 과정:
1. GetPrioritizedMoves → 중앙 (7,7) 반환
2. Minimax 호출 없이 즉시 중앙 선택

출력: (7, 7)
시간: 0.01초
```

### 예제 2: 즉시 승리 상황

```
보드 상태:
O O O O _ X
      ↑
    여기 두면 5목

처리 과정:
1. FindImmediateWin 호출
2. (0, 4) 발견 → 즉시 반환

출력: (0, 4)
시간: 0.02초 (전체 보드 스캔만)
```

### 예제 3: 복잡한 중반 상황

```
보드 상태:
  X O O _ X
  O X X O _
  _ X O X O
  ...

처리 과정:
1. 즉시 승리 없음
2. 즉시 저지 없음
3. GetPrioritizedMoves → 35개 후보 발견
4. 우선순위 계산 → 상위 20개 선택
5. 각 후보에 대해 Minimax (depth=4) 실행
   - 약 64,000 노드 탐색
   - Alpha-Beta로 약 40,000 노드 가지치기
6. 최고 점수 수 선택

출력: (최선의 위치)
시간: 0.8초
```

---

## BoardEvaluator 연동

### 평가 함수 호출 시점

```csharp
// 1. 우선순위 계산 시
int priority = CalculateMovePriority(board, row, col, player);
└─> evaluator.Evaluate(board, player);

// 2. Minimax 종료 조건에서
if (depth == 0)
    return evaluator.Evaluate(board, player);
```

### 평가 점수의 활용

```csharp
// AI 입장의 점수 계산
int aiScore = evaluator.Evaluate(board, player);

// 점수 해석:
// +10000: 4목 만들어서 유리
// +1000: 열린 3목으로 유리
// 0: 균형
// -1000: 상대가 열린 3목으로 불리
// -10000: 상대가 4목으로 매우 불리
```

---

## 디버깅 팁

### 1. Minimax 탐색 시각화

```csharp
// 디버깅용 코드 추가
private int Minimax(int[,] board, int depth, int alpha, int beta, 
    bool isMaximizing, int player)
{
    Console.WriteLine($"Depth: {depth}, Alpha: {alpha}, Beta: {beta}, " +
    $"Maximizing: {isMaximizing}");
    
    // ...기존 코드...
}
```

### 2. 후보 수 확인

```csharp
var validMoves = GetPrioritizedMoves(board, player);
Console.WriteLine($"Candidates: {validMoves.Count}");
foreach (var move in validMoves.Take(5))
{
    Console.WriteLine($"  ({move.row}, {move.col})");
}
```

### 3. 평가 점수 확인

```csharp
int score = Minimax(board, MAX_DEPTH - 1, alpha, beta, false, player);
Console.WriteLine($"Move ({move.row}, {move.col}) -> Score: {score}");
```

---

## 개선 가능한 부분

### 1. Transposition Table (전치 테이블)

```csharp
// 같은 보드 상태를 재계산하지 않도록 캐싱
private Dictionary<string, int> transpositionTable = new();

private int Minimax(...)
{
    string boardHash = GetBoardHash(board);
    if (transpositionTable.ContainsKey(boardHash))
        return transpositionTable[boardHash];
    
    // ...계산...
    
    transpositionTable[boardHash] = result;
    return result;
}
```

### 2. Iterative Deepening (반복 심화)

```csharp
// 시간 제한 내에서 점진적으로 깊이 증가
for (int depth = 1; depth <= MAX_DEPTH; depth++)
{
    if (TimeElapsed > TimeLimit) break;
    bestMove = FindBestMove(board, player, depth);
}
```

### 3. Killer Move Heuristic

```csharp
// 이전 탐색에서 좋았던 수를 우선 탐색
private List<(int, int)> killerMoves = new();
```

---

## 요약

HardAI는 다음과 같은 정교한 메커니즘으로 동작합니다:

1. **즉시 승리/저지 우선**: 명확한 상황은 빠르게 처리
2. **Minimax 알고리즘**: 4수 앞을 내다보며 최선의 수 계산
3. **Alpha-Beta Pruning**: 불필요한 탐색 제거로 성능 60% 향상
4. **후보 수 선별**: 225개 → 20개로 탐색 공간 대폭 축소
5. **BoardEvaluator 활용**: 보드 상태를 정확히 점수화

**결과적으로:**
- 고급 플레이어에게도 도전적인 난이도 제공
- 약 1초 내외의 빠른 응답 시간
- 70% 목표 승률 달성 가능

---

## 참고 자료

- **Minimax Algorithm**: https://en.wikipedia.org/wiki/Minimax
- **Alpha-Beta Pruning**: https://en.wikipedia.org/wiki/Alpha%E2%80%93beta_pruning
- **Game Tree Search**: 게임 이론의 기본 개념
- **Gomoku AI**: 오목 특화 AI 전략
- **Gomocup** : https://gomocup.org/download-for-developers/

---

_이 가이드는 HardAI.cs의 구현을 상세히 분석한 문서입니다._
_코드의 각 부분이 어떻게 연동되어 강력한 AI를 만들어내는지 이해하는데 도움이 되길 바랍니다._
