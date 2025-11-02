namespace Nexus_Omok_Game
{
    /// <summary>
    /// 어려움 난이도 AI - 미니맥스 + 알파-베타 가지치기
    /// 목표: 고급 플레이어에게도 도전적 (AI 승률 70%)
    /// </summary>
    public class HardAI : IAIPlayer
    {
        private const int BOARD_SIZE = 15;
        private const int MAX_DEPTH = 4;  // 탐색 깊이
        private const int MOVE_SEARCH_RADIUS = 2;  // 기존 돌 주변 탐색 반경

        private readonly Random random = new();
        private readonly BoardEvaluator evaluator = new();

        public AIDifficulty Difficulty => AIDifficulty.Hard;

        /// <summary>
        /// AI의 다음 수를 비동기로 계산합니다
        /// 전략: 미니맥스 + 알파-베타 프루닝
        /// </summary>
        public async Task<(int row, int col)> GetNextMoveAsync(int[,] board, int player)
        {
            // 비동기 딜레이 (사람처럼 보이게)
            await Task.Delay(random.Next(500, 1200));

            int opponent = player == 1 ? 2 : 1;

            // 1순위: 즉시 승리
            var winMove = FindImmediateWin(board, player);
            if (winMove.HasValue)
            {
                return winMove.Value;
            }

            // 2순위: 상대의 승리 저지
            var blockMove = FindImmediateWin(board, opponent);
            if (blockMove.HasValue)
            {
                return blockMove.Value;
            }

            // 3순위: 미니맥스 알고리즘으로 최선의 수 찾기
            return await Task.Run(() => FindBestMove(board, player));
        }

        /// <summary>
        /// 즉시 승리할 수 있는 수를 찾습니다
        /// </summary>
        private (int row, int col)? FindImmediateWin(int[,] board, int player)
        {
            for (int row = 0; row < BOARD_SIZE; row++)
            {
                for (int col = 0; col < BOARD_SIZE; col++)
                {
                    if (board[row, col] == 0)
                    {
                        if (evaluator.IsWinningMove(board, row, col, player))
                        {
                            return (row, col);
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 미니맥스 알고리즘으로 최선의 수를 찾습니다
        /// </summary>
        private (int row, int col) FindBestMove(int[,] board, int player)
        {
            var validMoves = GetPrioritizedMoves(board, player);

            if (validMoves.Count == 0)
            {
                return (BOARD_SIZE / 2, BOARD_SIZE / 2);  // 첫 수는 중앙
            }

            int bestScore = int.MinValue;
            (int row, int col) bestMove = validMoves[0];

            // 알파-베타 초기값
            int alpha = int.MinValue;
            int beta = int.MaxValue;

            foreach (var move in validMoves)
            {
                // 수를 두어봄
                board[move.row, move.col] = player;

                // 미니맥스 평가
                int score = Minimax(board, MAX_DEPTH - 1, alpha, beta, false, player);

                // 수를 되돌림
                board[move.row, move.col] = 0;

                if (score > bestScore)
                {
                    bestScore = score;
                    bestMove = move;
                }

                alpha = Math.Max(alpha, score);
            }

            return bestMove;
        }

        /// <summary>
        /// 미니맥스 알고리즘 (알파-베타 가지치기 포함)
        /// </summary>
        private int Minimax(int[,] board, int depth, int alpha, int beta, bool isMaximizing, int player)
        {
            // 종료 조건
            if (depth == 0)
            {
                return evaluator.Evaluate(board, player);
            }

            int opponent = player == 1 ? 2 : 1;

            // 게임 종료 확인
            if (IsGameOver(board))
            {
                return evaluator.Evaluate(board, player);
            }

            var moves = GetPrioritizedMoves(board, isMaximizing ? player : opponent);

            // 이동 가능한 수가 없으면 현재 상태 평가
            if (moves.Count == 0)
            {
                return evaluator.Evaluate(board, player);
            }

            if (isMaximizing)
            {
                int maxEval = int.MinValue;

                foreach (var move in moves)
                {
                    board[move.row, move.col] = player;
                    int eval = Minimax(board, depth - 1, alpha, beta, false, player);
                    board[move.row, move.col] = 0;

                    maxEval = Math.Max(maxEval, eval);
                    alpha = Math.Max(alpha, eval);

                    if (beta <= alpha)
                    {
                        break;// 베타 컷오프
                    }
                }

                return maxEval;
            }
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
                    {
                        break;  // 알파 컷오프
                    }
                }

                return minEval;
            }
        }

        /// <summary>
        /// 우선순위가 높은 이동 목록을 반환합니다
        /// 성능 최적화: 기존 돌 주변만 탐색
        /// </summary>
        private List<(int row, int col)> GetPrioritizedMoves(int[,] board, int player)
        {
            var moves = new List<(int row, int col, int priority)>();
            bool isEmpty = IsEmptyBoard(board);

            // 보드가 비어있으면 중앙만 반환
            if (isEmpty)
            {
                return new List<(int row, int col)> { (BOARD_SIZE / 2, BOARD_SIZE / 2) };
            }

            for (int row = 0; row < BOARD_SIZE; row++)
            {
                for (int col = 0; col < BOARD_SIZE; col++)
                {
                    if (board[row, col] == 0 && HasNearbyStone(board, row, col))
                    {
                        int priority = CalculateMovePriority(board, row, col, player);
                        moves.Add((row, col, priority));
                    }
                }
            }

            // 우선순위 높은 순으로 정렬 (최대 20개만 고려)
            return moves.OrderByDescending(m => m.priority)
           .Take(20)
              .Select(m => (m.row, m.col))
            .ToList();
        }

        /// <summary>
        /// 이동의 우선순위 계산
        /// </summary>
        private int CalculateMovePriority(int[,] board, int row, int col, int player)
        {
            board[row, col] = player;
            int score = evaluator.Evaluate(board, player);
            board[row, col] = 0;
            return score;
        }

        /// <summary>
        /// 주변에 돌이 있는지 확인 (반경 내)
        /// </summary>
        private bool HasNearbyStone(int[,] board, int row, int col)
        {
            for (int dr = -MOVE_SEARCH_RADIUS; dr <= MOVE_SEARCH_RADIUS; dr++)
            {
                for (int dc = -MOVE_SEARCH_RADIUS; dc <= MOVE_SEARCH_RADIUS; dc++)
                {
                    if (dr == 0 && dc == 0) continue;

                    int r = row + dr;
                    int c = col + dc;

                    if (r >= 0 && r < BOARD_SIZE && c >= 0 && c < BOARD_SIZE && board[r, c] != 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 보드가 비어있는지 확인
        /// </summary>
        private bool IsEmptyBoard(int[,] board)
        {
            for (int row = 0; row < BOARD_SIZE; row++)
            {
                for (int col = 0; col < BOARD_SIZE; col++)
                {
                    if (board[row, col] != 0)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// 게임이 종료되었는지 확인 (승리 또는 무승부)
        /// </summary>
        private bool IsGameOver(int[,] board)
        {
            // 간단한 체크: 보드가 가득 찼는지만 확인
            // 승리 체크는 평가 함수에서 처리
            for (int row = 0; row < BOARD_SIZE; row++)
            {
                for (int col = 0; col < BOARD_SIZE; col++)
                {
                    if (board[row, col] == 0)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
