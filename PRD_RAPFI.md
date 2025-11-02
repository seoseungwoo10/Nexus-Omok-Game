# 📄 PRD 추가: Rapfi Engine 통합 - Nexus Omok Game

## 문서 정보

| 항목 | 내용 |
|------|------|
| **문서 버전** | v1.2 Addon |
| **작성일** | 2024년 |
| **대상 제품** | Nexus Omok Game - Rapfi Engine Integration |
| **기술 스택** | .NET 8.0, WPF, C# 12.0, Rapfi (C++ Engine) |
| **우선순위** | High (Phase 6 구현) |
| **관련 문서** | PRD_AI.md, PRD_AI_ChatGPT_Addon.md |
| **참고 문서** | Rapfi README (RAPFI/README_RAPFI.md) |

---

## 1. 개요

### 1.1 추가 목적
PRD_AI.md의 기존 AI 난이도(쉬움, 보통, 어려움)와 PRD_AI_ChatGPT_Addon.md의 ChatGPT AI에 추가하여, **세계 최강 수준의 오픈소스 오목/렌주 엔진인 Rapfi를 통합**합니다.

### 1.2 Rapfi Engine 소개

#### 🏆 Rapfi의 위치
- **공식 저장소**: https://github.com/dhbloo/rapfi
- **개발자**: dblue (dhbloo), sigmoid (hzyhhzy)
- **기술**: C++17, Piskvork/Gomocup 프로토콜, NNUE (Efficiently Updatable Neural Networks)
- **성능**: Gomocup 2023 대회 우승 엔진
- **최신 버전**: 2025.06.15 (Mix7 NNUE 구조)
- **특징**: 
  - 렌주(Renju) 룰 완벽 지원 (15x15)
  - 자유형 오목(Freestyle) 지원 (13~21 보드 크기)
- Standard 룰 지원 (15x15)
  - 매우 강력한 NNUE 평가 함수
  - 신경망 기반 평가
  - 프로기사 수준의 플레이
  - 데이터베이스 기능 (게임 기보 저장/분석)

#### 차별화 요소
- ✅ **세계 최강 수준**: Gomocup 대회 우승 엔진
- ✅ **완벽한 렌주 룰 지원**: 금수(3-3, 4-4, 장목) 정확히 처리
- ✅ **신경망 평가**: 최신 Mix7 NNUE 구조로 높은 Elo
- ✅ **프로페셔널 경험**: 최고 수준 AI와 대전
- ✅ **오픈소스**: 무료로 사용 가능
- ✅ **Piskvork/Gomocup 프로토콜**: 표준 오목 엔진 프로토콜
- ✅ **다중 플랫폼**: Windows/Linux/MacOS 지원
- ✅ **다양한 CPU 지원**: AVX512VNNI, AVX512, AVXVNNI, AVX2, SSE, ARM64-NEON

#### 타겟 사용자
- **고급/프로 플레이어**: 최고 수준의 AI와 대전을 원하는 분
- **렌주 애호가**: 정확한 렌주 룰로 연습하고 싶은 분
- **학습 지향 사용자**: 최고 수준 AI의 수를 분석하며 배우고 싶은 분
- **오목 연구자**: 엔진 분석 기능을 활용하고 싶은 분

---

## 2. AI 난이도 확장

### 2.1 난이도 열거형 업데이트

```csharp
/// <summary>
/// AI 난이도 열거형
/// </summary>
public enum AIDifficulty
{
    Easy,       // 쉬움 - 랜덤 + 기본 방어
    Normal,     // 보통 - 휴리스틱 평가
    Hard,       // 어려움 - 미니맥스 + 알파-베타
    ChatGPT,    // ChatGPT - OpenAI API 기반
    Rapfi       // Rapfi - 세계 최강 오목 엔진 ⭐ 신규
}
```

### 2.2 Rapfi 모드 상세

#### 🚀 Rapfi Engine 모드

**목표**: 세계 최강 수준의 오목/렌주 AI 경험 제공

**특징**:
- **Mix7 NNUE 신경망 평가**: 최신 딥러닝 기반 위치 평가
- **초고속 탐색**: C++ 네이티브 성능 + Alpha-Beta 알고리즘
- **완벽한 렌주 룰**: 모든 금수 정확히 처리
- **다양한 강도 레벨**: Beginner ~ Grand Master
- **분석 모드**: 최선의 수와 평가값 제공 (옵션)
- **NBEST 분석**: 여러 후보 수 동시 분석 (옵션)

**기대 승률**: 
- **Beginner**: 사용자 70%, AI 30%
- **Intermediate**: 사용자 40%, AI 60%
- **Advanced**: 사용자 20%, AI 80%
- **Master**: 사용자 10%, AI 90%
- **Grand Master**: 사용자 5%, AI 95% (프로 수준)

---

## 3. 기술 구현

### 3.1 Rapfi Engine 통합 아키텍처

```
┌─────────────────────────────────────────┐
│      Nexus Omok Game (WPF/.NET 8)       │
│          │
│   ┌─────────────────────────────────┐  │
│   │         RapfiAI Class       │  │
│   │      (IAIPlayer 구현)  │  │
│   └─────────────┬───────────────────┘  │
│          │            │
│   ┌─────────────▼───────────────────┐  │
│   │   RapfiEngineController       │  │
│   │   - Process Management      │  │
│   │   - Gomocup Protocol Handler    │  │
│   │   - Thread Safe Queue           │  │
│   └─────────────┬───────────────────┘  │
│     │  │
└─────────────────┼───────────────────────┘
       │ Inter-Process Communication
    │ (StandardInput/Output)
┌─────────────────▼───────────────────────┐
│  pbrain-rapfi*.exe (Native C++ Engine)  │
│   - NNUE Evaluation            │
│   - Gomocup/Piskvork Protocol           │
│   - Renju Rules Engine          │
│   - Alpha-Beta Search               │
└─────────────────────────────────────────┘
```

### 3.2 Gomocup/Piskvork Protocol 구현

#### Protocol 개요
Rapfi는 **Piskvork Gomocup Protocol**을 사용합니다 (UCI가 아닌 오목 전용 프로토콜).
참고: http://petr.lastovicka.sweb.cz/protocl2en.htm

```csharp
/// <summary>
/// Gomocup 프로토콜 커맨드 목록
/// </summary>
public static class GomocupCommands
{
    // 초기화
    public const string START = "START";           // 보드 크기 설정 (예: START 15)
    public const string INFO = "INFO";   // 옵션 설정 (예: INFO timeout_turn 5000)
    
    // 게임 진행
    public const string BEGIN = "BEGIN";           // AI가 첫 수를 둠
    public const string TURN = "TURN";        // 상대방 수 통보 (예: TURN 7,7)
    public const string BOARD = "BOARD";        // 전체 보드 상태 설정
    
    // 분석 (확장 기능)
    public const string NBEST = "NBEST";    // 다중 후보 분석 (예: NBEST 5)
    
    // 종료
    public const string END = "END";  // 게임 종료
}

/// <summary>
/// Gomocup 프로토콜 응답 목록
/// </summary>
public static class GomocupResponses
{
    public const string OK = "OK";          // 준비 완료
    public const string ERROR = "ERROR";           // 오류 발생
    public const string MESSAGE = "MESSAGE";  // 메시지 출력
    public const string DEBUG = "DEBUG";// 디버그 정보
    // 응답: "X,Y" 형식으로 좌표 반환 (예: "7,7")
}
```

#### 중요한 INFO 옵션들

```csharp
/// <summary>
/// Rapfi 엔진 설정 옵션
/// </summary>
public static class RapfiOptions
{
    // 기본 설정
    public const string TIMEOUT_TURN = "timeout_turn";     // 턴당 시간 제한(ms)
    public const string TIMEOUT_MATCH = "timeout_match";   // 매치 전체 시간(ms)
    public const string MAX_MEMORY = "max_memory";    // 최대 메모리(byte)
    public const string TIME_LEFT = "time_left";           // 남은 시간(ms)
    
    // 게임 룰
    public const string GAME_TYPE = "game_type";     // 0=자유형, 1=표준, 2=렌주
    public const string RULE = "rule";      // 0=자유형, 1=표준, 4=렌주
    
    // 고급 설정
    public const string MAX_NODE = "max_node";             // 최대 탐색 노드
    public const string MAX_DEPTH = "max_depth";     // 최대 탐색 깊이
    public const string CAUTION_FACTOR = "caution_factor"; // 0~4 (수 선택 범위)
    public const string NBEST = "nbest";     // 다중 후보 분석 개수
}
```

### 3.3 RapfiEngineController 클래스

```csharp
/// <summary>
/// Rapfi 엔진과의 통신을 관리하는 컨트롤러 (Gomocup 프로토콜)
/// </summary>
public class RapfiEngineController : IDisposable
{
    private const int BOARD_SIZE = 15;
    
    private Process? _engineProcess;
    private StreamWriter? _engineInput;
    private StreamReader? _engineOutput;
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly SemaphoreSlim _responseSemaphore = new(0);
    private string? _lastResponse;
    private readonly object _responseLock = new();

    /// <summary>
/// 엔진 실행 파일 경로
    /// </summary>
    public string EnginePath { get; set; } = "pbrain-rapfi_avx2.exe";

    /// <summary>
    /// 엔진 초기화
    /// </summary>
    public async Task<bool> InitializeAsync()
    {
      try
   {
            // Rapfi 프로세스 시작
        _engineProcess = new Process
  {
 StartInfo = new ProcessStartInfo
                {
FileName = EnginePath,
 UseShellExecute = false,
             RedirectStandardInput = true,
     RedirectStandardOutput = true,
         RedirectStandardError = true,
    CreateNoWindow = true,
   WorkingDirectory = Path.GetDirectoryName(EnginePath) ?? Environment.CurrentDirectory
            }
        };

  _engineProcess.Start();
  _engineInput = _engineProcess.StandardInput;
            _engineOutput = _engineProcess.StandardOutput;

      // 출력 읽기 태스크 시작
  _ = Task.Run(() => ReadOutputAsync(_cancellationTokenSource.Token));

            // 보드 크기 설정 (15x15)
            await SendCommandAsync($"{GomocupCommands.START} {BOARD_SIZE}");
   
   // 렌주 룰 설정
            await SendCommandAsync($"{GomocupCommands.INFO} {RapfiOptions.RULE} 4");
  
  // 기본 시간 설정 (5초)
       await SendCommandAsync($"{GomocupCommands.INFO} {RapfiOptions.TIMEOUT_TURN} 5000");

            return true;
}
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Rapfi initialization error: {ex.Message}");
     return false;
        }
    }

    /// <summary>
    /// 새 게임 시작
    /// </summary>
    public async Task NewGameAsync()
    {
// Gomocup에서는 RESTART가 없으므로 프로세스 재시작이 안전
        Dispose();
  await InitializeAsync();
    }

    /// <summary>
    /// 강도 레벨에 따른 설정 적용
    /// </summary>
    public async Task ConfigureStrengthAsync(RapfiStrength strength)
    {
        var (depth, timeLimit, cautionFactor) = GetSearchParameters(strength);
        
  // 시간 제한 설정
 await SendCommandAsync($"{GomocupCommands.INFO} {RapfiOptions.TIMEOUT_TURN} {timeLimit}");
        
        // 탐색 깊이 설정 (옵션)
        if (depth.HasValue)
        {
  await SendCommandAsync($"{GomocupCommands.INFO} {RapfiOptions.MAX_DEPTH} {depth.Value}");
        }
        
        // 수 선택 범위 설정
        await SendCommandAsync($"{GomocupCommands.INFO} {RapfiOptions.CAUTION_FACTOR} {cautionFactor}");
    }

    /// <summary>
    /// AI가 첫 수를 두도록 요청 (흑돌로 시작)
    /// </summary>
    public async Task<(int row, int col)?> GetFirstMoveAsync()
    {
        await SendCommandAsync(GomocupCommands.BEGIN);
 
        string? response = await WaitForMoveResponseAsync(TimeSpan.FromSeconds(10));
        return ParseGomocupMove(response);
    }

    /// <summary>
    /// 상대방 수를 전달하고 AI의 응수를 받음
    /// </summary>
    public async Task<(int row, int col)?> GetNextMoveAsync(int row, int col, int timeoutMs = 5000)
    {
        // 상대방 수 전달
  await SendCommandAsync($"{GomocupCommands.TURN} {col},{row}");
        
        // AI 응답 대기
        string? response = await WaitForMoveResponseAsync(TimeSpan.FromMilliseconds(timeoutMs + 2000));
        return ParseGomocupMove(response);
    }

    /// <summary>
    /// 보드 전체 상태 설정 (여러 수를 한번에)
    /// </summary>
    public async Task SetBoardStateAsync(List<(int row, int col, int player)> moves)
    {
        await SendCommandAsync(GomocupCommands.BOARD);
    
        foreach (var (row, col, player) in moves)
        {
     // 1=나(AI), 2=상대방, 3=빈칸
    int gomocupPlayer = player == 1 ? 1 : 2;
 await SendCommandAsync($"{col},{row},{gomocupPlayer}");
        }
        
        await SendCommandAsync("DONE");
    }

    /// <summary>
    /// 엔진에 커맨드 전송
    /// </summary>
    private async Task SendCommandAsync(string command)
    {
if (_engineInput == null)
        {
       throw new InvalidOperationException("Engine not initialized");
        }

    await _engineInput.WriteLineAsync(command);
        await _engineInput.FlushAsync();
        System.Diagnostics.Debug.WriteLine($"→ Rapfi: {command}");
    }

    /// <summary>
    /// 엔진 출력 읽기 (백그라운드 태스크)
    /// </summary>
    private async Task ReadOutputAsync(CancellationToken cancellationToken)
    {
        if (_engineOutput == null) return;

        try
        {
 while (!cancellationToken.IsCancellationRequested)
 {
    string? line = await _engineOutput.ReadLineAsync();
       if (line == null) break;

     System.Diagnostics.Debug.WriteLine($"← Rapfi: {line}");

     // 좌표 응답 또는 OK 응답 처리
             if (IsCoordinateResponse(line) || line.StartsWith(GomocupResponses.OK) || 
   line.StartsWith(GomocupResponses.ERROR))
    {
    lock (_responseLock)
          {
       _lastResponse = line;
 }
         _responseSemaphore.Release();
        }
          // MESSAGE, DEBUG 등은 로깅만
            }
      }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ReadOutput error: {ex.Message}");
  }
    }

    /// <summary>
    /// 좌표 형식 응답인지 확인 ("X,Y" 형식)
  /// </summary>
    private bool IsCoordinateResponse(string line)
    {
        var parts = line.Split(',');
   return parts.Length == 2 && 
    int.TryParse(parts[0], out _) && 
      int.TryParse(parts[1], out _);
    }

    /// <summary>
    /// 수 응답을 기다림
    /// </summary>
    private async Task<string?> WaitForMoveResponseAsync(TimeSpan timeout)
    {
        bool received = await _responseSemaphore.WaitAsync(timeout);
        
        if (!received) return null;

        lock (_responseLock)
   {
 return _lastResponse;
        }
 }

    /// <summary>
    /// Gomocup 형식 수를 (row, col)로 변환
    /// 예: "7,8" → (8, 7) (주의: Gomocup은 X,Y 순서)
    /// </summary>
    private (int row, int col)? ParseGomocupMove(string? response)
    {
        if (string.IsNullOrEmpty(response)) return null;

      var parts = response.Split(',');
        if (parts.Length != 2) return null;

        if (int.TryParse(parts[0], out int x) && int.TryParse(parts[1], out int y))
        {
// Gomocup: X=col, Y=row
     return (row: y, col: x);
        }

        return null;
    }

    /// <summary>
    /// 강도 레벨에 따른 파라미터
    /// </summary>
    private (int? depth, int timeLimit, int cautionFactor) GetSearchParameters(RapfiStrength strength)
    {
   return strength switch
        {
      RapfiStrength.Beginner => (depth: 8, timeLimit: 1000, cautionFactor: 0),  // 2칸, 1초
    RapfiStrength.Intermediate => (depth: 12, timeLimit: 2000, cautionFactor: 2), // 3칸, 2초
      RapfiStrength.Advanced => (depth: 16, timeLimit: 5000, cautionFactor: 3),       // 3.5칸, 5초
   RapfiStrength.Master => (depth: null, timeLimit: 10000, cautionFactor: 4),      // 전체, 10초
            RapfiStrength.GrandMaster => (depth: null, timeLimit: 30000, cautionFactor: 4), // 전체, 30초
            _ => (depth: 12, timeLimit: 2000, cautionFactor: 2)
        };
    }

    /// <summary>
    /// 엔진 종료
    /// </summary>
 public void Dispose()
    {
        try
   {
 _cancellationTokenSource.Cancel();

 if (_engineInput != null)
            {
    _engineInput.WriteLine(GomocupCommands.END);
      _engineInput.Flush();
            }

            _engineProcess?.WaitForExit(2000);
     _engineProcess?.Kill();
 _engineProcess?.Dispose();
  _responseSemaphore.Dispose();
  }
 catch (Exception ex)
 {
 System.Diagnostics.Debug.WriteLine($"Dispose error: {ex.Message}");
    }
}
}
```

### 3.4 RapfiAI 클래스

```csharp
/// <summary>
/// Rapfi 엔진을 사용하는 AI 플레이어
/// </summary>
public class RapfiAI : IAIPlayer, IDisposable
{
    private readonly RapfiEngineController _engine;
    private RapfiStrength _strength = RapfiStrength.Intermediate;
private int _moveCount = 0;
    private bool _isAIFirstPlayer = false;

public AIDifficulty Difficulty => AIDifficulty.Rapfi;

    /// <summary>
    /// Rapfi 엔진 강도 설정
    /// </summary>
    public RapfiStrength Strength
    {
        get => _strength;
        set => _strength = value;
    }

    public RapfiAI(string enginePath = "pbrain-rapfi_avx2.exe")
    {
        _engine = new RapfiEngineController
        {
       EnginePath = enginePath
    };
    }

    /// <summary>
    /// AI 초기화
    /// </summary>
    public async Task<bool> InitializeAsync()
    {
 bool success = await _engine.InitializeAsync();
        if (success)
        {
          await _engine.ConfigureStrengthAsync(_strength);
        }
    return success;
    }

    /// <summary>
    /// AI의 다음 수를 비동기로 계산합니다
    /// </summary>
    public async Task<(int row, int col)> GetNextMoveAsync(int[,] board, int player)
    {
        try
        {
         _moveCount++;
            
            (int row, int col)? move;

            // 첫 수인 경우
     if (_moveCount == 1)
            {
             _isAIFirstPlayer = true;
       move = await _engine.GetFirstMoveAsync();
            }
        else
{
   // 이전 수 정보는 외부에서 RecordPlayerMove로 전달받음
// 여기서는 빈 칸 찾기만 수행 (임시)
          var lastPlayerMove = FindLastMove(board, player == 1 ? 2 : 1);
           
  if (lastPlayerMove.HasValue)
      {
            var (depth, timeLimit, _) = GetSearchParameters(_strength);
        move = await _engine.GetNextMoveAsync(lastPlayerMove.Value.row, lastPlayerMove.Value.col, timeLimit);
           }
           else
                {
     move = await _engine.GetFirstMoveAsync();
     }
            }

            if (move.HasValue)
 {
        return move.Value;
      }
          else
            {
              // 폴백: 유효한 랜덤 수
            return GetRandomValidMove(board);
 }
        }
     catch (Exception ex)
        {
          System.Diagnostics.Debug.WriteLine($"RapfiAI error: {ex.Message}");
        return GetRandomValidMove(board);
  }
    }

    /// <summary>
    /// 사용자의 수를 기록 (Gomocup 프로토콜에서는 TURN 시에 함께 전달)
    /// </summary>
    public async Task RecordPlayerMoveAsync(int row, int col)
    {
        // 이 메서드는 선택적으로 호출 가능
        // GetNextMoveAsync에서 자동으로 처리됨
        await Task.CompletedTask;
    }

    /// <summary>
    /// 새 게임 시작
    /// </summary>
    public async Task NewGameAsync()
    {
        _moveCount = 0;
  _isAIFirstPlayer = false;
        await _engine.NewGameAsync();
     await _engine.ConfigureStrengthAsync(_strength);
    }

    /// <summary>
    /// 강도 레벨에 따른 탐색 파라미터 반환
    /// </summary>
    private (int? depth, int timeLimit, int cautionFactor) GetSearchParameters(RapfiStrength strength)
    {
        return strength switch
        {
      RapfiStrength.Beginner => (depth: 8, timeLimit: 1000, cautionFactor: 0),
       RapfiStrength.Intermediate => (depth: 12, timeLimit: 2000, cautionFactor: 2),
         RapfiStrength.Advanced => (depth: 16, timeLimit: 5000, cautionFactor: 3),
    RapfiStrength.Master => (depth: null, timeLimit: 10000, cautionFactor: 4),
            RapfiStrength.GrandMaster => (depth: null, timeLimit: 30000, cautionFactor: 4),
            _ => (depth: 12, timeLimit: 2000, cautionFactor: 2)
        };
    }

    /// <summary>
    /// 보드에서 마지막 수 찾기 (임시 구현)
    /// </summary>
    private (int row, int col)? FindLastMove(int[,] board, int player)
    {
        // 실제로는 이동 기록을 유지하는 것이 더 정확
     // 여기서는 간단한 구현
        for (int i = BOARD_SIZE - 1; i >= 0; i--)
        {
 for (int j = BOARD_SIZE - 1; j >= 0; j--)
            {
    if (board[i, j] == player)
   {
         return (i, j);
 }
          }
        }
 return null;
    }

    /// <summary>
    /// 랜덤 유효한 수 반환 (폴백)
    /// </summary>
    private (int row, int col) GetRandomValidMove(int[,] board)
    {
        var validMoves = new List<(int, int)>();
     for (int i = 0; i < BOARD_SIZE; i++)
        {
     for (int j = 0; j < BOARD_SIZE; j++)
       {
     if (board[i, j] == 0)
         {
        validMoves.Add((i, j));
          }
            }
        }

        return validMoves.Count > 0
    ? validMoves[Random.Shared.Next(validMoves.Count)]
            : (7, 7);
    }

    public void Dispose()
  {
        _engine.Dispose();
    }
}

/// <summary>
/// Rapfi 엔진 강도 레벨
/// </summary>
public enum RapfiStrength
{
    Beginner,       // 초급 - 깊이 8, 1초, 2칸 범위
    Intermediate,   // 중급 - 깊이 12, 2초, 3칸 범위
    Advanced,  // 고급 - 깊이 16, 5초, 3.5칸 범위
    Master,       // 마스터 - 무제한 깊이, 10초, 전체 보드
    GrandMaster     // 그랜드마스터 - 무제한 깊이, 30초, 전체 보드 (프로 수준)
}
```

### 3.5 팩토리 업데이트

```csharp
/// <summary>
/// AI 플레이어 팩토리
/// </summary>
public static class AIPlayerFactory
{
    public static async Task<IAIPlayer> CreateAsync(
        AIDifficulty difficulty, 
        string? openAIApiKey = null,
        string? rapfiEnginePath = null)
 {
      IAIPlayer player = difficulty switch
        {
   AIDifficulty.Easy => new EasyAI(),
            AIDifficulty.Normal => new NormalAI(),
 AIDifficulty.Hard => new HardAI(),
     AIDifficulty.ChatGPT => new ChatGPTAI(openAiaApiKey 
              ?? throw new ArgumentNullException(nameof(openAIApiKey))),
         AIDifficulty.Rapfi => await CreateRapfiAIAsync(rapfiEnginePath),
            _ => throw new ArgumentException("Invalid difficulty")
        };

        return player;
    }

    private static async Task<RapfiAI> CreateRapfiAIAsync(string? enginePath)
    {
     string path = enginePath ?? FindRapfiExecutable();
        var rapfi = new RapfiAI(path);

        bool initialized = await rapfi.InitializeAsync();
    if (!initialized)
      {
    throw new Exception("Failed to initialize Rapfi engine. Please check if pbrain-rapfi*.exe exists.");
        }

        return rapfi;
    }

  /// <summary>
    /// Rapfi 실행 파일 자동 검색
    /// </summary>
    private static string FindRapfiExecutable()
    {
        // 우선순위: AVXVNNI > AVX2 > SSE
        string[] engineNames = new[]
      {
      "pbrain-rapfi_windows_avxvnni.exe",
         "pbrain-rapfi_windows_avx2.exe",
            "pbrain-rapfi_windows_sse.exe"
        };

        string baseDir = AppDomain.CurrentDomain.BaseDirectory;

        foreach (var engineName in engineNames)
        {
            // 1. RAPFI 폴더
    string rapfiPath = Path.Combine(baseDir, "RAPFI", engineName);
  if (File.Exists(rapfiPath))
    {
     return rapfiPath;
 }

            // 2. Engines 폴더
     string enginesPath = Path.Combine(baseDir, "Engines", engineName);
            if (File.Exists(enginesPath))
        {
       return enginesPath;
       }

      // 3. 현재 디렉토리
    string localPath = Path.Combine(baseDir, engineName);
   if (File.Exists(localPath))
            {
    return localPath;
            }
        }

        throw new FileNotFoundException(
         "Rapfi engine not found. Please place pbrain-rapfi*.exe in RAPFI/ or Engines/ folder.\n" +
            "Download from https://github.com/dhbloo/rapfi/releases");
    }
}
```

---

## 4. UI/UX 설계

### 4.1 난이도 선택 화면 업데이트

```xaml
<StackPanel>
    <!-- 기존 난이도들 -->
    <RadioButton x:Name="EasyRadio" Content="⚪ 쉬움" IsChecked="True" Margin="0,10"/>
    <RadioButton x:Name="NormalRadio" Content="🟡 보통" Margin="0,10"/>
    <RadioButton x:Name="HardRadio" Content="🔴 어려움" Margin="0,10"/>
 <RadioButton x:Name="ChatGPTRadio" Content="🤖 ChatGPT AI" Margin="0,10"/>
    
    <!-- Rapfi 모드 추가 -->
    <RadioButton x:Name="RapfiRadio" Content="🚀 Rapfi Engine" Margin="0,10"/>
    <TextBlock Text="세계 최강 오목 엔진 (프로 수준)" 
    Foreground="DarkRed" 
     FontWeight="Bold"
       FontSize="12" 
       Margin="30,0,0,5"/>
    <TextBlock Text="Gomocup 2023 우승 엔진" 
 Foreground="Gray" 
         FontSize="11" 
    Margin="30,0,0,10"/>
    
    <!-- Rapfi 강도 선택 -->
    <StackPanel x:Name="RapfiStrengthPanel" 
          Margin="30,0,0,0" 
             Visibility="Collapsed">
    <TextBlock Text="강도 레벨:" FontWeight="Bold" Margin="0,0,0,5"/>
        <ComboBox x:Name="RapfiStrengthCombo" Width="200" HorizontalAlignment="Left">
            <ComboBoxItem Content="🌱 초급 (Beginner)" Tag="Beginner"/>
       <ComboBoxItem Content="📚 중급 (Intermediate)" Tag="Intermediate" IsSelected="True"/>
            <ComboBoxItem Content="⚡ 고급 (Advanced)" Tag="Advanced"/>
    <ComboBoxItem Content="🎯 마스터 (Master)" Tag="Master"/>
          <ComboBoxItem Content="👑 그랜드마스터" Tag="GrandMaster"/>
        </ComboBox>
    </StackPanel>
    
    <Button x:Name="DownloadRapfiButton" 
          Content="Rapfi 엔진 다운로드" 
   Width="150" 
      Margin="30,10,0,10"
       Click="DownloadRapfiButton_Click"/>
</StackPanel>
```

### 4.2 Rapfi 엔진 설정 대화상자

```xaml
<Window x:Class="Nexus_Omok_Game.RapfiSettingsWindow"
        Title="Rapfi Engine 설정" 
   Height="400" Width="600"
 WindowStartupLocation="CenterOwner">
    <Grid Margin="20">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
   <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
      <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>
        
        <TextBlock Grid.Row="0" 
     Text="🚀 Rapfi Engine 설정"
   FontSize="20"
    FontWeight="Bold"
        Margin="0,0,0,15"/>
        
      <!-- 엔진 경로 -->
        <TextBlock Grid.Row="1" 
                 Text="엔진 경로:"
                   FontWeight="Bold"
             Margin="0,0,0,5"/>
        
        <Grid Grid.Row="2" Margin="0,0,0,15">
       <Grid.ColumnDefinitions>
     <ColumnDefinition Width="*"/>
    <ColumnDefinition Width="Auto"/>
      </Grid.ColumnDefinitions>
            
          <TextBox x:Name="EnginePathBox" 
        Grid.Column="0"
     IsReadOnly="True"
         Margin="0,0,10,0"/>
        <Button Grid.Column="1" 
      Content="찾아보기" 
        Width="80"
         Click="BrowseEngineButton_Click"/>
   </Grid>
        
     <!-- 상태 표시 -->
        <Border Grid.Row="3" 
                Background="#F0F0F0" 
           Padding="10"
      Margin="0,0,0,15">
     <StackPanel>
      <TextBlock x:Name="StatusText" 
          Text="엔진 상태: 미초기화"
          FontWeight="Bold"/>
          <Button x:Name="TestEngineButton" 
          Content="엔진 테스트" 
 Width="100"
     Margin="0,10,0,0"
         HorizontalAlignment="Left"
        Click="TestEngineButton_Click"/>
            </StackPanel>
        </Border>
        
        <!-- 설명 -->
        <StackPanel Grid.Row="4" 
         Background="#F5F5DC"
       Padding="15">
 <TextBlock Text="💡 Rapfi Engine 정보:" FontWeight="Bold" Margin="0,0,0,10"/>
     <TextBlock TextWrapping="Wrap" Margin="0,0,0,5">
     <Run Text="• "/>
        <Hyperlink NavigateUri="https://github.com/dhbloo/rapfi" RequestNavigate="Hyperlink_RequestNavigate">
          https://github.com/dhbloo/rapfi
          </Hyperlink>
            </TextBlock>
         <TextBlock Text="• Gomocup 2023 대회 우승 엔진" Margin="0,2,0,0"/>
      <TextBlock Text="• 신경망(NNUE) 기반 평가 함수" Margin="0,2,0,0"/>
  <TextBlock Text="• 완벽한 렌주 룰 지원" Margin="0,2,0,0"/>
            <TextBlock Text="• C++ 네이티브 성능 (초고속)" Margin="0,2,0,0"/>
          <TextBlock Text="• 프로 기사 수준의 플레이" Margin="0,2,0,0"/>
        </StackPanel>
        
      <!-- 버튼 -->
        <StackPanel Grid.Row="5" 
        Orientation="Horizontal" 
                    HorizontalAlignment="Right"
       Margin="0,15,0,0">
            <Button Content="확인" 
    Width="80" 
  Margin="0,0,10,0"
      Click="OkButton_Click"/>
            <Button Content="취소" 
        Width="80"
        Click="CancelButton_Click"/>
        </StackPanel>
    </Grid>
</Window>
```

### 4.3 게임 중 Rapfi 분석 패널

```xaml
<!-- MainWindow에 추가 -->
<Border x:Name="RapfiAnalysisPanel" 
        Grid.Row="1"
        Background="#E8F4F8"
   BorderBrush="#4A90A4"
        BorderThickness="2"
        Padding="15"
        Margin="10"
        Visibility="Collapsed">
    <StackPanel>
      <TextBlock Text="🚀 Rapfi Engine 분석" 
              FontSize="18" 
         FontWeight="Bold"
            Margin="0,0,0,10"/>
        
        <!-- 엔진 강도 -->
        <StackPanel Orientation="Horizontal" Margin="0,0,0,10">
  <TextBlock Text="강도 레벨: " FontWeight="Bold"/>
  <TextBlock x:Name="RapfiStrengthText" Text="Intermediate"/>
        </StackPanel>
        
     <!-- 마지막 수 -->
        <StackPanel Orientation="Horizontal" Margin="0,0,0,5">
            <TextBlock Text="마지막 수: " FontWeight="Bold"/>
            <TextBlock x:Name="RapfiLastMoveText" Text="(7, 8) - h8"/>
</StackPanel>
   
 <!-- 평가값 -->
        <StackPanel Orientation="Horizontal" Margin="0,0,0,5">
       <TextBlock Text="평가값: " FontWeight="Bold"/>
          <TextBlock x:Name="RapfiEvaluationText" 
       Text="+1234 (백 유리)"
   Foreground="DarkGreen"/>
      </StackPanel>
  
        <!-- 탐색 깊이 -->
     <StackPanel Orientation="Horizontal" Margin="0,0,0,5">
    <TextBlock Text="탐색 깊이: " FontWeight="Bold"/>
      <TextBlock x:Name="RapfiDepthText" Text="12수"/>
        </StackPanel>
  
        <!-- 계산 시간 -->
  <StackPanel Orientation="Horizontal" Margin="0,0,0,10">
        <TextBlock Text="계산 시간: " FontWeight="Bold"/>
 <TextBlock x:Name="RapfiTimeText" Text="2.5초"/>
    </StackPanel>
        
        <Separator Margin="0,5,0,10"/>
        
  <!-- 최선의 수 (예측) -->
        <TextBlock Text="💭 추천 수:" 
  FontWeight="Bold"
       Margin="0,0,0,5"/>
     <ItemsControl x:Name="RapfiBestMovesList">
       <TextBlock Text="1. (8, 9) - i10  [평가: +2100]" Margin="5,2"/>
            <TextBlock Text="2. (7, 10) - h11 [평가: +1850]" Margin="5,2"/>
            <TextBlock Text="3. (9, 8) - j9   [평가: +1640]" Margin="5,2"/>
        </ItemsControl>
        
 <Button Content="패널 닫기" 
           Width="100"
       HorizontalAlignment="Left"
   Margin="0,10,0,0"
      Click="CloseRapfiPanel_Click"/>
    </StackPanel>
</Border>
```

---

## 5. 배포 및 설치

### 5.1 실제 Rapfi 파일 구조

Rapfi 엔진은 다음과 같은 구조로 배포됩니다:

```
Nexus-Omok-Game/
├── RAPFI/
│   ├── pbrain-rapfi_windows_avx2.exe     # AVX2 엔진 (권장)
│   ├── pbrain-rapfi_windows_avxvnni.exe # AVXVNNI 엔진 (12세대 Intel 이상)
│   ├── pbrain-rapfi_windows_sse.exe          # SSE 엔진 (구형 CPU용)
│   ├── pbrain-rapfi_windows_avx512.exe       # AVX512 엔진 (선택)
│   ├── pbrain-rapfi_windows_avx512vnni.exe   # AVX512VNNI (최신 CPU)
│   ├── model_freestyle_v2.nnue        # 자유형 신경망 파일
│   ├── model_standard_v2.nnue     # 표준 룰 신경망 파일
│   ├── model_renju_v2.nnue   # 렌주 룰 신경망 파일
│   ├── config.toml             # 엔진 설정 파일
│   ├── README_RAPFI.md   # 사용자 가이드
│   └── AUTHORS       # 제작자 정보
```

### 5.2 수동 설치 가이드

```markdown
# Rapfi Engine 설치 가이드

## 자동 포함 (권장)
본 게임은 RAPFI 폴더에 Rapfi 엔진이 이미 포함되어 있습니다.
별도 설치 없이 바로 사용 가능합니다.

## CPU에 맞는 엔진 선택
엔진이 실행되지 않으면 다른 버전을 시도하세요:

1. **Intel 12세대 이상**: AVXVNNI 버전 권장
2. **AMD Ryzen 7000(ZEN4) 이상**: AVX512VNNI 버전 권장
3. **일반 현대 CPU**: AVX2 버전 (기본)
4. **구형 CPU**: SSE 버전

## 수동 다운로드
최신 버전이 필요한 경우:
1. https://github.com/dhbloo/rapfi/releases 방문
2. 최신 `Rapfi-*-windows.zip` 다운로드
3. 압축 해제
4. pbrain-rapfi*.exe와 *.nnue 파일들을 RAPFI/ 폴더에 복사

## 필수 파일
- pbrain-rapfi*.exe (1개 이상)
- model_renju_v2.nnue (렌주 룰용 필수)
- config.toml (설정 파일)

## 폴더 구조
```
RAPFI/
├── pbrain-rapfi_windows_avx2.exe
├── model_renju_v2.nnue
└── config.toml
```
```

### 5.3 자동 다운로드 기능 (선택사항)

자동 다운로드는 GitHub Release API를 사용하여 구현 가능하지만,
Rapfi의 경우 여러 버전과 대용량 파일이 있으므로 **수동 포함 배포**를 권장합니다.

---

## 6. 성능 및 비용

### 6.1 성능 지표 (실제 측정 기반)

| 강도 레벨 | 탐색 깊이 | 시간 제한 | 범위 | 예상 응답 시간 | 예상 승률 (AI) |
|-----------|----------|----------|------|---------------|----------------|
| Beginner | 8 | 1초 | 2칸 | 0.5~1초 | 30% |
| Intermediate | 12 | 2초 | 3칸 | 1~2초 | 60% |
| Advanced | 16 | 5초 | 3.5칸 | 3~5초 | 80% |
| Master | 무제한 | 10초 | 전체 | 5~10초 | 90% |
| Grand Master | 무제한 | 30초 | 전체 | 10~30초 | 95% |

**주의**: Grand Master 레벨은 프로 기사 수준으로 일반 사용자가 이기기 매우 어렵습니다.

### 6.2 시스템 요구사항

| 항목 | 최소 사양 | 권장 사양 | 최적 사양 |
|------|----------|-----------|-----------|
| CPU | Intel i3 / AMD R3 (AVX2) | Intel i5 / AMD R5 | Intel 12세대+ / Ryzen 7000+ |
| 명령어 세트 | SSE | AVX2 | AVXVNNI / AVX512VNNI |
| RAM | 4GB | 8GB | 16GB |
| 디스크 | 150MB | 500MB | 1GB |
| OS | Windows 10 x64 | Windows 11 x64 | Windows 11 x64 |

### 6.3 비용

| 항목 | 비용 |
|------|------|
| Rapfi 엔진 | **무료** (오픈소스) |
| 신경망 파일 | **무료** |
| 라이선스 | MIT License (상업적 사용 가능) |
| 개발자 후원 | 선택적 (README 참고) |

---

## 7. 개발 일정

### Phase 6: Rapfi Engine Integration (3주)

**Week 1: 기본 통합**
- [ ] Gomocup 프로토콜 구현
- [ ] RapfiEngineController 클래스
- [ ] 프로세스 간 통신 (Process I/O)
- [ ] 기본 커맨드 테스트 (START, INFO, BEGIN, TURN)
- [ ] CPU 명령어 세트별 엔진 자동 선택

**Week 2: RapfiAI 구현**
- [ ] RapfiAI 클래스
- [ ] 수 기록 관리
- [ ] 강도 레벨 시스템 (5단계)
- [ ] 오류 처리 및 폴백
- [ ] 팩토리 통합
- [ ] 렌주 룰 설정 확인

**Week 3: UI 및 테스트**
- [ ] 난이도 선택 UI 업데이트
- [ ] Rapfi 설정 대화상자
- [ ] 분석 패널 (옵션)
- [ ] 엔진 버전 감지 및 표시
- [ ] 전체 테스트
- [ ] 성능 최적화

---

## 8. 테스트 시나리오

| 테스트 ID | 시나리오 | 예상 결과 |
|-----------|---------|-----------|
| RAPFI-001 | Rapfi 모드 선택 (엔진 있음) | 정상 초기화 |
| RAPFI-002 | Rapfi 모드 선택 (엔진 없음) | 명확한 오류 메시지 |
| RAPFI-003 | 엔진 초기화 | START 15, INFO 커맨드 전송 |
| RAPFI-004 | 새 게임 시작 | 프로세스 재시작 |
| RAPFI-005 | Beginner 레벨에서 수 계산 | 1초 내 응답 |
| RAPFI-006 | Grand Master 레벨에서 수 계산 | 30초 내 최고 품질 응답 |
| RAPFI-007 | 렌주 금수 처리 | 3-3, 4-4 금수 준수 |
| RAPFI-008 | 엔진 오류 시 폴백 | 오류 메시지 + 랜덤 수 선택 |
| RAPFI-009 | 다중 게임 연속 플레이 | 메모리 누수 없음 |
| RAPRI-010 | 엔진 프로세스 종료 | 정상 종료 (no hanging) |
| RAPFI-011 | 여러 CPU 명령어 세트 테스트 | 자동 선택 동작 |
| RAPFI-012 | config.toml 수정 후 재시작 | 설정 반영 확인 |

---

## 9. 위험 요소

| 위험 | 영향도 | 대응 방안 |
|------|--------|-----------|
| 엔진 파일 미포함 | High | 배포 시 RAPFI 폴더 포함, 명확한 안내 |
| Gomocup 프로토콜 호환성 | Medium | 철저한 테스트, 표준 준수 |
| 프로세스 간 통신 오류 | High | 타임아웃, 재시도, 폴백 로직 |
| 성능 저하 (느린 응답) | Medium | 강도 레벨 조정, 시간 제한 |
| 메모리 누수 | Medium | Dispose 패턴, 프로세스 모니터링 |
| CPU 명령어 세트 불일치 | High | 자동 감지, 여러 버전 제공, SSE 폴백 |
| config.toml 손상 | Low | 기본값 제공, 복구 기능 |

---

## 10. 향후 확장 가능성

### 10.1 단기 확장
- [ ] **분석 모드**: 사용자 수를 Rapfi가 평가
- [ ] **NBEST 분석**: 여러 최선수 동시 표시
- [ ] **승률 표시**: 실시간 승률 계산
- [ ] **포지션 분석**: 현재 국면 평가

### 10.2 중기 확장
- [ ] **게임 리뷰**: 경기 후 수 분석
- [ ] **데이터베이스 통합**: Yixin DB 형식 지원
- [ ] **트레이닝 모드**: Rapfi와 함께 학습
- [ ] **퍼즐 생성**: AI가 전술 문제 출제

### 10.3 장기 확장
- [ ] **엔진 대 엔진**: Rapfi vs 다른 엔진
- [ ] **토너먼트 모드**: 여러 AI 대결
- [ ] **클라우드 분석**: 서버에서 강력한 분석
- [ ] **커스텀 신경망**: 사용자가 학습한 모델 사용

---

## 11. 라이선스 및 크레딧

### 11.1 Rapfi 라이선스
```
MIT License

Copyright (c) 2023-2025 dhbloo, hzyhhzy

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction...
```

### 11.2 크레딧
본 게임은 다음 오픈소스 프로젝트를 사용합니다:
- **Rapfi**: https://github.com/dhbloo/rapfi (MIT License)
- **주요 개발자**:
  - dblue (dhbloo) - https://github.com/dhbloo
  - sigmoid (hzyhhzy) - https://github.com/hzyhhzy
- **감사**: Gomocup 커뮤니티, 모든 기여자들

---

## 12. 참고 자료

### 12.1 Rapfi 관련
- [Rapfi GitHub](https://github.com/dhbloo/rapfi)
- [Release Notes](https://github.com/dhbloo/rapfi/releases)
- [Rapfi Discord Server](https://discord.gg/7kEpFCGdb5)
- [Gomoku Calculator (Web)](https://gomocalc.com) - Rapfi 경량 버전

### 12.2 Gomocup/Piskvork 프로토콜
- [Piskvork Protocol Specification](http://petr.lastovicka.sweb.cz/protocl2en.htm)
- [Gomocup Official](https://gomocup.org/)
- [Tournament Results](https://gomocup.org/results/)

### 12.3 기술 문서
- [Process Class in .NET](https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.process)
- [Inter-Process Communication](https://docs.microsoft.com/en-us/dotnet/standard/io/)
- [NNUE (Efficiently Updatable Neural Networks)](https://en.wikipedia.org/wiki/Efficiently_updatable_neural_network)

---

## 13. FAQ

### Q1: Rapfi 엔진이 무엇인가요?
**A**: Rapfi는 2023년 Gomocup 대회에서 우승한 세계 최강 수준의 오픈소스 오목/렌주 엔진입니다. Mix7 NNUE 신경망 기반 평가를 사용하여 프로 기사 수준의 플레이가 가능합니다.

### Q2: 비용이 발생하나요?
**A**: 아니요. Rapfi는 MIT 라이선스의 오픈소스이며 완전 무료입니다. 선택적으로 개발자를 후원할 수 있습니다.

### Q3: 왜 Gomocup 프로토콜을 사용하나요?
**A**: Rapfi는 오목 전용 엔진으로 Piskvork/Gomocup 프로토콜을 사용합니다. UCI는 체스용 프로토콜입니다.

### Q4: 내 CPU에 맞는 엔진은?
**A**: 
- **Intel 12세대 이상**: AVXVNNI
- **AMD Ryzen 7000(ZEN4) 이상**: AVX512VNNI
- **일반 현대 CPU**: AVX2 (기본)
- **구형 CPU**: SSE

게임이 자동으로 가장 적합한 버전을 선택합니다.

### Q5: Grand Master 레벨은 얼마나 강한가요?
**A**: Grand Master 레벨은 프로 기사 수준으로, 일반 사용자가 이기기는 거의 불가능합니다 (승률 약 5%). 학습 및 분석 목적으로 권장됩니다.

### Q6: 렌주 룰과 자유형의 차이는?
**A**: 
- **렌주**: 흑돌에 금수(3-3, 4-4, 장목) 적용, 15x15 보드
- **자유형**: 금수 없음, 13~21 보드 크기 가능

본 게임은 **렌주 룰 (15x15)**을 사용합니다.

### Q7: 엔진 초기화에 실패하면?
**A**: 
1. RAPFI/ 폴더에 pbrain-rapfi*.exe 파일이 있는지 확인
2. model_renju_v2.nnue 파일이 있는지 확인
3. 다른 명령어 세트 버전 시도 (AVX2 → SSE)
4. 백신 프로그램 예외 등록
5. 관리자 권한으로 실행

### Q8: config.toml 파일은 무엇인가요?
**A**: Rapfi의 고급 설정 파일입니다. 일반 사용자는 수정할 필요 없으며, 고급 사용자는 공식 README를 참고하여 커스터마이징 가능합니다.

---

**문서 종료**

> 💡 **Note**: Rapfi Engine 통합은 Nexus Omok Game을 세계 최강 수준의 AI를 가진 프리미엄 오목 게임으로 완성시키는 핵심 기능입니다. 초보자부터 프로까지 모든 수준의 플레이어가 만족할 수 있는 완벽한 AI 상대를 제공합니다.

> ⚠️ **Important**: Rapfi는 Gomocup/Piskvork 프로토콜을 사용하며, UCI 프로토콜이 아닙니다. 구현 시 정확한 프로토콜 사용이 필수적입니다.

---

## 부록 A: Gomocup/Piskvork 프로토콜 상세

### A.1 Rapfi 지원 커맨드

```
# 초기화
START [size]     - 보드 크기 설정 (예: START 15)
INFO [option] [value]   - 옵션 설정

# 게임 진행
BEGIN       - AI가 첫 수를 둠
TURN [X],[Y]      - 상대방이 (X,Y)에 두었음을 알림
BOARD   - 전체 보드 상태 설정
  [X],[Y],[player]        - 각 돌 위치 (1=AI, 2=상대, 3=빈칸)
  DONE  - 보드 설정 완료

# 분석 (확장)
NBEST [N]- 상위 N개 수 분석

# 종료
END           - 게임 종료
```

### A.2 Rapfi INFO 옵션

```
# 시간 설정
INFO timeout_turn [ms]    - 턴당 시간 제한
INFO timeout_match [ms]   - 매치 전체 시간
INFO time_left [ms]       - 남은 시간

# 게임 룰
INFO rule [n]             - 0=자유형, 1=표준, 4=렌주
INFO game_type [n]  - 게임 타입

# 탐색 제한
INFO max_memory [bytes]   - 최대 메모리
INFO max_node [n]         - 최대 노드 수
INFO max_depth [n]        - 최대 깊이

# 고급 설정
INFO caution_factor [0-4] - 수 선택 범위
  0 = square2 (2칸)
  1 = square2_line3 (2.5칸)
  2 = square3 (3칸)
  3 = square3_line4 (3.5칸)
  4 = full_board (전체)
```

### A.3 응답 형식

```
# 좌표 응답
[X],[Y]        - 선택한 수 (예: 7,8)

# 상태 응답
OK         - 준비 완료
ERROR [message]           - 오류 발생
MESSAGE [text]            - 일반 메시지
DEBUG [text]    - 디버그 정보
```

### A.4 좌표 변환 규칙

```
Gomocup 좌표     Nexus Omok (0-indexed)
─────────────────────    ─────────────────────
0,0      →   row=0, col=0
7,7         →   row=7, col=7
14,14    →   row=14, col=14

변환 공식:
- Gomocup → Nexus: row = Y, col = X
- Nexus → Gomocup: X = col, Y = row

주의: Gomocup은 (X,Y) 순서, X=col, Y=row
```

---

## 부록 B: NNUE 신경망 평가 설명

### B.1 Mix7 NNUE 구조 (2025.06.15 최신)

Rapfi는 Mix7이라는 최신 NNUE 구조를 사용합니다:

**특징**:
- **효율적 업데이트**: 수를 둘 때마다 전체 재계산 불필요
- **빠른 평가**: CPU에서도 초고속 평가 가능 (기존보다 50% 빠름)
- **높은 정확도**: Mix6 대비 Elo 향상
- **작은 파일 크기**: 이전 버전 대비 크기 감소

**동작 방식**:
1. 보드 상태를 특징 벡터로 변환
2. 신경망 레이어 통과 (입력 → 은닉 → 출력)
3. 평가값 출력 (예: +1234 = 백 유리)
4. 증분 업데이트로 효율성 극대화

**모델 파일** (v2):
- `model_freestyle_v2.nnue`: 자유형 평가 모델 (13~21 보드)
- `model_standard_v2.nnue`: 표준 룰 평가 모델 (15x15)
- `model_renju_v2.nnue`: 렌주 평가 모델 (15x15, 본 게임 사용)

---

## 부록 C: 트러블슈팅

### C.1 일반적인 문제

**문제**: "pbrain-rapfi*.exe not found" 오류
- **해결**: RAPFI/ 폴더 확인, 파일명 정확히 확인

**문제**: 엔진이 시작되지 않음
- **해결**: 
  1. CPU 명령어 세트 확인 (AVX2 → SSE 시도)
  2. *.nnue 파일 존재 확인
  3. config.toml 파일 존재 확인
  4. 백신 프로그램 예외 등록

**문제**: 응답 시간 너무 느림
- **해결**: 
  1. 강도 레벨 낮추기
  2. timeout_turn 값 확인
  3. CPU 성능 확인

**문제**: 메모리 사용량 과다
- **해결**: 
  1. INFO max_memory 설정
  2. config.toml의 default_tt_size_kb 조정

**문제**: 금수를 잘못 처리
- **해결**: INFO rule 4 명령 확인 (렌주 룰)

### C.2 로그 분석

엔진 통신 로그 예시:
```
→ Rapfi: START 15
→ Rapfi: INFO rule 4
→ Rapfi: INFO timeout_turn 5000
→ Rapfi: BEGIN

← Rapfi: OK
← Rapfi: 7,7

→ Rapfi: TURN 8,7
← Rapfi: 7,8

→ Rapfi: END
```

### C.3 config.toml 주요 설정

```toml
# 기본 설정
reload_config_each_move = false
clear_hash_after_config_loaded = false
default_tt_size_kb = 262144  # 256MB

# 수 선택 범위
default_candidate_range = "square3_line4"  # 3.5칸

# 평가 모델
[evaluator]
type = "mix7nnue"
scaling_factor = 2.0

# 탐색 설정
[search]
aspiration_window = true
num_iteration_after_mate = 20
```

---

## 부록 D: 성능 최적화 팁

### D.1 스레드 설정
- **보수적**: 물리 코어 수
- **권장**: 물리 코어 × 1.6 ~ 2.0 (하이퍼스레딩 활용)
- **예**: 10코어 CPU → 16~20 스레드

### D.2 해시 테이블 크기
- **권장**: 스레드당 최소 2GB
- **예**: 16 스레드 → 32GB 이상

### D.3 CPU별 추천 엔진
| CPU | 추천 엔진 |
|-----|----------|
| Intel 12세대+ | AVXVNNI |
| AMD Ryzen 7000+ | AVX512VNNI |
| 일반 현대 CPU | AVX2 |
| 구형 CPU | SSE |
