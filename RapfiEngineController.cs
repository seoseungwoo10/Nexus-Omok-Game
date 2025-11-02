using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Nexus_Omok_Game
{
    /// <summary>
    /// Rapfi 엔진과의 통신을 관리하는 컨트롤러 (Gomocup 프로토콜)
    /// </summary>
    public class RapfiEngineController : IDisposable
    {
        private const int BOARD_SIZE = 15;

        private Process? _engineProcess;
        private StreamWriter? _engineInput;
        private StreamReader? _engineOutput;
        private CancellationTokenSource _cancellationTokenSource = new();
        private SemaphoreSlim _responseSemaphore = new(0);
        private string? _lastResponse;
        private readonly object _responseLock = new();

        // ⭐ 초기화 완료 플래그 추가
        private bool _isEngineReady = false;
        private SemaphoreSlim _initSemaphore = new(0);

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

                // ⭐ OK 응답 대기 (엔진 초기화 완료)
                bool ready = await _initSemaphore.WaitAsync(TimeSpan.FromSeconds(30));
                if (!ready)
                {
                    Debug.WriteLine("❌ Rapfi: 초기화 타임아웃");
                    return false;
                }

                Debug.WriteLine("✅ Rapfi: 엔진 준비 완료");

                // 렌주 룰 설정
                await SendCommandAsync($"{GomocupCommands.INFO} {RapfiOptions.RULE} 4");

                // 기본 시간 설정 (5초)
                await SendCommandAsync($"{GomocupCommands.INFO} {RapfiOptions.TIMEOUT_TURN} 5000");

                _isEngineReady = true;
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Rapfi initialization error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 새 게임 시작
        /// </summary>
        public async Task NewGameAsync()
        {
            // ⭐ 기존 리소스 정리
            Dispose();

            // ⭐ 새로운 동기화 객체 생성
            _cancellationTokenSource = new CancellationTokenSource();
            _responseSemaphore = new SemaphoreSlim(0);
            _initSemaphore = new SemaphoreSlim(0);
            _isEngineReady = false;
            _lastResponse = null;

            // ⭐ 엔진 재시작
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
            if (!_isEngineReady)
            {
                Debug.WriteLine("⚠️ Rapfi: 엔진이 준비되지 않음");
                return null;
            }

            // ⭐ 이전 응답 초기화
            lock (_responseLock)
            {
                _lastResponse = null;
            }

            // 세마포어 카운트 초기화 (대기 중인 응답 제거)
            while (_responseSemaphore.CurrentCount > 0)
            {
                await _responseSemaphore.WaitAsync(TimeSpan.FromMilliseconds(1));
            }

            await SendCommandAsync(GomocupCommands.BEGIN);

            // ⭐ 긴 타임아웃 (신경망 로딩 시간 고려)
            string? response = await WaitForMoveResponseAsync(TimeSpan.FromSeconds(30));

            if (response == null)
            {
                Debug.WriteLine("❌ Rapfi: BEGIN 응답 타임아웃");
            }

            return ParseGomocupMove(response);
        }

        /// <summary>
        /// 상대방 수를 전달하고 AI의 응수를 받음
        /// </summary>
        public async Task<(int row, int col)?> GetNextMoveAsync(int row, int col, int timeoutMs = 5000)
        {
            if (!_isEngineReady)
            {
                Debug.WriteLine("⚠️ Rapfi: 엔진이 준비되지 않음");
                return null;
            }

            // ⭐ 이전 응답 초기화
            lock (_responseLock)
            {
                _lastResponse = null;
            }

            // 세마포어 카운트 초기화
            while (_responseSemaphore.CurrentCount > 0)
            {
                await _responseSemaphore.WaitAsync(TimeSpan.FromMilliseconds(1));
            }

            // 상대방 수 전달
            await SendCommandAsync($"{GomocupCommands.TURN} {col},{row}");

            // AI 응답 대기
            string? response = await WaitForMoveResponseAsync(TimeSpan.FromMilliseconds(timeoutMs + 2000));

            if (response == null)
            {
                Debug.WriteLine($"❌ Rapfi: TURN {col},{row} 응답 타임아웃");
            }

            return ParseGomocupMove(response);
        }

        /// <summary>
        /// 보드 전체 상태 설정 (여러 수를 한번에)
        /// </summary>
        public async Task SetBoardStateAsync(List<(int row, int col, int player)> moves)
        {
            // ⭐ 이전 응답 초기화
            lock (_responseLock)
            {
                _lastResponse = null;
            }

            // 세마포어 카운트 초기화
            while (_responseSemaphore.CurrentCount > 0)
            {
                await _responseSemaphore.WaitAsync(TimeSpan.FromMilliseconds(1));
            }

            await SendCommandAsync(GomocupCommands.BOARD);

            foreach (var (row, col, player) in moves)
            {
                // ⭐ Gomocup 프로토콜: 1=AI(나), 2=상대방
                // player는 이미 Gomocup 형식이어야 함
                await SendCommandAsync($"{col},{row},{player}");
            }

            await SendCommandAsync("DONE");

            // ⭐ BOARD 후에는 OK가 아니라 좌표 응답이 옴!
            // 응답을 기다리지 않고 바로 리턴
            Debug.WriteLine($"✅ Rapfi: BOARD 설정 전송 완료");

            // 짧은 대기 (엔진이 BOARD 처리하도록)
            await Task.Delay(100);
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
            Debug.WriteLine($"→ Rapfi: {command}");
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

                    Debug.WriteLine($"← Rapfi: {line}");

                    // ⭐ OK 응답 감지 (초기화 완료)
                    if (line.StartsWith(GomocupResponses.OK))
                    {
                        _initSemaphore.Release();

                        lock (_responseLock)
                        {
                            _lastResponse = line;
                        }
                        _responseSemaphore.Release();
                    }
                    // 좌표 응답 처리
                    else if (IsCoordinateResponse(line))
                    {
                        lock (_responseLock)
                        {
                            _lastResponse = line;
                        }
                        _responseSemaphore.Release();
                    }
                    // 에러 응답 처리
                    else if (line.StartsWith(GomocupResponses.ERROR))
                    {
                        Debug.WriteLine($"⚠️ Rapfi Error: {line}");
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
                Debug.WriteLine($"ReadOutput error: {ex.Message}");
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

            if (!received)
            {
                Debug.WriteLine($"⏱️ Rapfi: 응답 대기 시간 초과 ({timeout.TotalSeconds}초)");
                return null;
            }

            lock (_responseLock)
            {
                var response = _lastResponse;
                _lastResponse = null;  // ⭐ 응답 사용 후 초기화
                return response;
            }
        }

        /// <summary>
        /// Gomocup 형식 수를 (row, col)로 변환
        /// 예: "7,8" → (8, 7) (주의: Gomocup은 X,Y 순서)
        /// </summary>
        private (int row, int col)? ParseGomocupMove(string? response)
        {
            if (string.IsNullOrEmpty(response)) return null;

            // ERROR 응답 처리
            if (response.StartsWith(GomocupResponses.ERROR))
            {
                Debug.WriteLine($"❌ Rapfi: 오류 응답 - {response}");
                return null;
            }

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
                _isEngineReady = false;

                // ⭐ CancellationTokenSource 취소 (이미 취소되었을 수 있음)
                if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
                {
                    _cancellationTokenSource.Cancel();
                }

                if (_engineInput != null)
                {
                    try
                    {
                        _engineInput.WriteLine(GomocupCommands.END);
                        _engineInput.Flush();
                    }
                    catch
                    {
                        // 이미 닫혔을 수 있음
                    }
                }

                _engineProcess?.WaitForExit(2000);
                _engineProcess?.Kill();
                _engineProcess?.Dispose();

                // ⭐ Dispose는 호출하되, NewGameAsync에서 새로 생성함
                _responseSemaphore?.Dispose();
                _initSemaphore?.Dispose();
                _cancellationTokenSource?.Dispose();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Dispose error: {ex.Message}");
            }
        }
    }
}
