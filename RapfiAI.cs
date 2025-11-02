using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Nexus_Omok_Game
{
    /// <summary>
    /// Rapfi 엔진을 사용하는 AI 플레이어
    /// </summary>
    public class RapfiAI : IAIPlayer, IDisposable
    {
        private const int BOARD_SIZE = 15;

        private readonly RapfiEngineController _engine;
        private RapfiStrength _strength = RapfiStrength.Intermediate;
        private int _moveCount = 0;
        private bool _isAIFirstPlayer = false;

        // 이동 기록 추가
        private readonly List<(int row, int col, int player)> _moveHistory = new();
        private (int row, int col)? _lastPlayerMove = null;

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

                (int row, int col)? move = null;

                // ⭐ 5수마다 BOARD로 전체 동기화
                //    일단 주석처리 BOARD 후 영뚱한 수를 두는 문제 발생
                //if (_moveCount > 1 && _moveCount % 5 == 0)
                //{
                //    await SyncBoardStateAsync(board);
                //}

                // ⭐ AI가 첫 수인 경우 (플레이어 수가 기록되지 않음)
                if (_moveHistory.Count == 0)
                {
                    _isAIFirstPlayer = true;
                    Debug.WriteLine($"🎯 Rapfi: AI가 첫 수 (흑돌)");
                    move = await _engine.GetFirstMoveAsync();
                }
                else
                {
                    // 마지막 플레이어 수를 사용
                    if (_lastPlayerMove.HasValue)
                    {
                        var (depth, timeLimit, _) = GetSearchParameters(_strength);

                        Debug.WriteLine($"🎯 Rapfi: 상대방 수 전달 - ({_lastPlayerMove.Value.row}, {_lastPlayerMove.Value.col})");

                        move = await _engine.GetNextMoveAsync(
                             _lastPlayerMove.Value.row,
                    _lastPlayerMove.Value.col,
                       timeLimit);

                        // 사용 후 즉시 초기화
                        _lastPlayerMove = null;
                    }
                    else
                    {
                        // 폴백: BOARD로 전체 동기화 후 다음 수 계산
                        Debug.WriteLine($"⚠️ Rapfi: 마지막 수 없음 - BOARD 동기화");
                        await SyncBoardStateAsync(board);

                        // ⭐ BOARD 후에는 엔진이 응답을 보냄 (BEGIN 불필요)
                        // 응답을 받아서 사용
                        Debug.WriteLine($"⚠️ Rapfi: BOARD 동기화 후 대기 중...");
                        await Task.Delay(200); // BOARD 처리 대기

                        // 폴백: 랜덤 수
                        move = GetRandomValidMove(board);
                        Debug.WriteLine($"⚠️ Rapfi: 폴백 - 랜덤 수 사용");
                    }
                }

                // ⭐ 응답이 null이면 재시도
                if (!move.HasValue)
                {
                    Debug.WriteLine($"❌ Rapfi: 응답 없음 - 랜덤 수 사용");
                    move = GetRandomValidMove(board);
                }

                if (move.HasValue)
                {
                    // ⭐ 보드 상태 검증
                    if (board[move.Value.row, move.Value.col] != 0)
                    {
                        Debug.WriteLine($"❌ Rapfi: 잘못된 수 - ({move.Value.row}, {move.Value.col}) 이미 돌이 있음!");
                        move = GetRandomValidMove(board);
                    }

                    // ⭐ AI의 수를 기록 - Gomocup: 1=AI(나)
                    _moveHistory.Add((move.Value.row, move.Value.col, 1));
                    Debug.WriteLine($"✅ Rapfi: AI 수 - ({move.Value.row}, {move.Value.col}) | AI=1 | 총 {_moveHistory.Count}수");

                    return move.Value;
                }
                else
                {
                    Debug.WriteLine($"❌ Rapfi: 응답 없음. 랜덤 수 사용");
                    var randomMove = GetRandomValidMove(board);
                    _moveHistory.Add((randomMove.row, randomMove.col, 1));
                    return randomMove;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ RapfiAI error: {ex.Message}");
                var randomMove = GetRandomValidMove(board);
                _moveHistory.Add((randomMove.row, randomMove.col, 1));
                return randomMove;
            }
        }

        /// <summary>
        /// ⭐ 보드 전체 상태를 엔진과 동기화
        /// </summary>
        private async Task SyncBoardStateAsync(int[,] board)
        {
            Debug.WriteLine($"🔄 Rapfi: 보드 전체 동기화 시작");

            var moves = new List<(int row, int col, int player)>();

            // ⭐ _moveHistory에서 정확한 플레이어 정보 사용
            foreach (var (row, col, player) in _moveHistory)
            {
                moves.Add((row, col, player));
            }

            // ⭐ 보드에서 누락된 수 찾기 (혹시 모를 경우)
            if (moves.Count == 0)
            {
                Debug.WriteLine($"⚠️ Rapfi: _moveHistory 비어있음 - 보드에서 직접 읽기");

                // 보드 순회하여 수 복구
                for (int i = 0; i < BOARD_SIZE; i++)
                {
                    for (int j = 0; j < BOARD_SIZE; j++)
                    {
                        if (board[i, j] != 0)
                        {
                            // ⚠️ 보드 값으로는 누가 AI인지 알 수 없음!
                            // AI가 첫 수면: 1=AI, 2=플레이어
                            // 플레이어가 첫 수면: 1=플레이어, 2=AI
                            int gomocupPlayer = (_isAIFirstPlayer) ? board[i, j] : (3 - board[i, j]);
                            moves.Add((i, j, gomocupPlayer));
                        }
                    }
                }
            }

            if (moves.Count > 0)
            {
                await _engine.SetBoardStateAsync(moves);
                Debug.WriteLine($"✅ Rapfi: 보드 동기화 완료 - {moves.Count}개 돌");

                // ⭐ 디버그: 전송된 수 로깅
                foreach (var (row, col, player) in moves)
                {
                    string playerName = player == 1 ? "AI" : "플레이어";
                    Debug.WriteLine($"  - ({row},{col}): {playerName}");
                }
            }
            else
            {
                Debug.WriteLine($"⚠️ Rapfi: 동기화할 수 없음 - 보드 비어있음");
            }
        }

        /// <summary>
        /// 사용자의 수를 기록
        /// </summary>
        public async Task RecordPlayerMoveAsync(int row, int col)
        {
            // 중복 기록 방지
            if (_lastPlayerMove.HasValue && _lastPlayerMove.Value == (row, col))
            {
                Debug.WriteLine($"⚠️ Rapfi: 중복 기록 방지 - ({row}, {col})");
                return;
            }

            _lastPlayerMove = (row, col);

            // ⭐ Gomocup 프로토콜: 1=AI(나), 2=상대방(플레이어)
            // 플레이어는 항상 2 (상대방)
            int playerNumber = 2;

            _moveHistory.Add((row, col, playerNumber));
            Debug.WriteLine($"📝 Rapfi: 플레이어 수 기록 - ({row}, {col}) | 플레이어=2 | 총 {_moveHistory.Count}수");
            await Task.CompletedTask;
        }

        /// <summary>
        /// 새 게임 시작
        /// </summary>
        public async Task NewGameAsync()
        {
            _moveCount = 0;
            _isAIFirstPlayer = false;
            _moveHistory.Clear();
            _lastPlayerMove = null;

            await _engine.NewGameAsync();
            await _engine.ConfigureStrengthAsync(_strength);

            Debug.WriteLine($"🔄 Rapfi: 새 게임 시작 - 모든 상태 초기화 완료");
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
}
