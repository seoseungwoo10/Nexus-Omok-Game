namespace Nexus_Omok_Game
{
    /// <summary>
    /// 보통 난이도 AI - 휴리스틱 평가
    /// 목표: 중급자에게 적합한 균형잡힌 플레이 (승률 50:50)
    /// </summary>
    public class NormalAI : IAIPlayer
    {
        private const int BOARD_SIZE = 15;
        private readonly Random random = new();
        private readonly BoardEvaluator evaluator = new();

        public AIDifficulty Difficulty => AIDifficulty.Normal;

        /// <summary>
        /// AI의 다음 수를 비동기로 계산합니다
        /// 전략: 패턴 인식 + 휴리스틱 평가 + 간단한 lookahead
        /// </summary>
        public async Task<(int row, int col)> GetNextMoveAsync(int[,] board, int player)
        {
            // 비동기 딜레이 (사람처럼 보이게)
            await Task.Delay(random.Next(400, 1000));

            int opponent = player == 1 ? 2 : 1;

            // 5% 확률로 랜덤한 실수 (보통 난이도 특성)
            if (random.NextDouble() < 0.05)
            {
                return GetRandomMove(board);
            }

            // 1순위: 자신의 즉시 승리
            var winMove = FindWinningMove(board, player);
            if (winMove.HasValue)
            {
                return winMove.Value;
            }

            // 2순위: 상대의 즉시 승리 차단 (95% 확률)
            if (random.NextDouble() < 0.95)
            {
                var blockMove = FindWinningMove(board, opponent);
                if (blockMove.HasValue)
                {
                    return blockMove.Value;
                }
            }

            // 3순위: 열린 4목 만들기 또는 막기
            var openFourMove = FindOpenFourMove(board, player);
            if (openFourMove.HasValue)
            {
                return openFourMove.Value;
            }

            var blockOpenFourMove = FindOpenFourMove(board, opponent);
            if (blockOpenFourMove.HasValue && random.NextDouble() < 0.9)
            {
                return blockOpenFourMove.Value;
            }

            // 4순위: 열린 3목 만들기
            var openThreeMove = FindOpenThreeMove(board, player);
            if (openThreeMove.HasValue)
            {
                return openThreeMove.Value;
            }

            // 5순위: 상대의 열린 3목 막기 (85% 확률)
            if (random.NextDouble() < 0.85)
            {
                var blockOpenThreeMove = FindOpenThreeMove(board, opponent);
                if (blockOpenThreeMove.HasValue)
                {
                    return blockOpenThreeMove.Value;
                }
            }

            // 6순위: 최선의 수 찾기 (휴리스틱 평가)
            return FindBestMoveByEvaluation(board, player);
        }

        /// <summary>
        /// 즉시 승리할 수 있는 수를 찾습니다
        /// </summary>
        private (int row, int col)? FindWinningMove(int[,] board, int player)
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
        /// 열린 4목을 만들 수 있는 수를 찾습니다 (_OOOO_)
        /// </summary>
        private (int row, int col)? FindOpenFourMove(int[,] board, int player)
        {
            for (int row = 0; row < BOARD_SIZE; row++)
            {
                for (int col = 0; col < BOARD_SIZE; col++)
                {
                    if (board[row, col] == 0)
                    {
                        board[row, col] = player;
                        if (IsOpenFour(board, row, col, player))
                        {
                            board[row, col] = 0;
                            return (row, col);
                        }
                        board[row, col] = 0;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 열린 4목인지 확인
        /// </summary>
        private bool IsOpenFour(int[,] board, int row, int col, int player)
        {
            int[][] directions = new int[][]
            {
        new int[] { 0, 1 },  // 가로
 new int[] { 1, 0 },  // 세로
              new int[] { 1, 1 },  // 대각선 \
   new int[] { 1, -1 }  // 대각선 /
                      };

            foreach (var dir in directions)
            {
                int count = 1;
                bool leftOpen = false;
                bool rightOpen = false;

                // 정방향 확인
                int r = row + dir[0];
                int c = col + dir[1];
                while (r >= 0 && r < BOARD_SIZE && c >= 0 && c < BOARD_SIZE && board[r, c] == player)
                {
                    count++;
                    r += dir[0];
                    c += dir[1];
                }
                if (r >= 0 && r < BOARD_SIZE && c >= 0 && c < BOARD_SIZE && board[r, c] == 0)
                {
                    rightOpen = true;
                }

                // 역방향 확인
                r = row - dir[0];
                c = col - dir[1];
                while (r >= 0 && r < BOARD_SIZE && c >= 0 && c < BOARD_SIZE && board[r, c] == player)
                {
                    count++;
                    r -= dir[0];
                    c -= dir[1];
                }
                if (r >= 0 && r < BOARD_SIZE && c >= 0 && c < BOARD_SIZE && board[r, c] == 0)
                {
                    leftOpen = true;
                }

                // 정확히 4개이고 양쪽이 열려있으면 열린 4목
                if (count == 4 && leftOpen && rightOpen)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 열린 3목을 만들 수 있는 수를 찾습니다 (_OOO_)
        /// </summary>
        private (int row, int col)? FindOpenThreeMove(int[,] board, int player)
        {
            for (int row = 0; row < BOARD_SIZE; row++)
            {
                for (int col = 0; col < BOARD_SIZE; col++)
                {
                    if (board[row, col] == 0)
                    {
                        board[row, col] = player;
                        if (IsOpenThree(board, row, col, player))
                        {
                            board[row, col] = 0;
                            return (row, col);
                        }
                        board[row, col] = 0;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 열린 3목인지 확인
        /// </summary>
        private bool IsOpenThree(int[,] board, int row, int col, int player)
        {
            int[][] directions = new int[][]
                  {
           new int[] { 0, 1 },  // 가로
     new int[] { 1, 0 },  // 세로
                new int[] { 1, 1 },  // 대각선 \
 new int[] { 1, -1 }  // 대각선 /
                  };

            foreach (var dir in directions)
            {
                int count = 1;
                bool leftOpen = false;
                bool rightOpen = false;

                // 정방향 확인
                int r = row + dir[0];
                int c = col + dir[1];
                while (r >= 0 && r < BOARD_SIZE && c >= 0 && c < BOARD_SIZE && board[r, c] == player)
                {
                    count++;
                    r += dir[0];
                    c += dir[1];
                }
                if (r >= 0 && r < BOARD_SIZE && c >= 0 && c < BOARD_SIZE && board[r, c] == 0)
                {
                    rightOpen = true;
                }

                // 역방향 확인
                r = row - dir[0];
                c = col - dir[1];
                while (r >= 0 && r < BOARD_SIZE && c >= 0 && c < BOARD_SIZE && board[r, c] == player)
                {
                    count++;
                    r -= dir[0];
                    c -= dir[1];
                }
                if (r >= 0 && r < BOARD_SIZE && c >= 0 && c < BOARD_SIZE && board[r, c] == 0)
                {
                    leftOpen = true;
                }

                // 정확히 3개이고 양쪽이 열려있으면 열린 3목
                if (count == 3 && leftOpen && rightOpen)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 휴리스틱 평가로 최선의 수를 찾습니다
        /// </summary>
        private (int row, int col) FindBestMoveByEvaluation(int[,] board, int player)
        {
            var candidateMoves = GetCandidateMoves(board);

            if (candidateMoves.Count == 0)
            {
                // 첫 수는 중앙
                return (BOARD_SIZE / 2, BOARD_SIZE / 2);
            }

            int bestScore = int.MinValue;
            (int row, int col) bestMove = candidateMoves[0];

            foreach (var move in candidateMoves)
            {
                // 수를 두어봄
                board[move.row, move.col] = player;

                // 평가 (간단한 1-step lookahead)
                int score = evaluator.Evaluate(board, player);

                // 상대의 최선 응수 고려 (lookahead)
                score -= GetBestOpponentResponse(board, player);

                // 수를 되돌림
                board[move.row, move.col] = 0;

                if (score > bestScore)
                {
                    bestScore = score;
                    bestMove = move;
                }
            }

            return bestMove;
        }

        /// <summary>
        /// 상대의 최선 응수 점수를 계산 (간단한 lookahead)
        /// </summary>
        private int GetBestOpponentResponse(int[,] board, int player)
        {
            int opponent = player == 1 ? 2 : 1;
            var moves = GetCandidateMoves(board).Take(5).ToList(); // 상위 5개만 확인

            if (moves.Count == 0)
            {
                return 0;
            }

            int bestOpponentScore = int.MinValue;

            foreach (var move in moves)
            {
                board[move.row, move.col] = opponent;
                int score = evaluator.Evaluate(board, opponent);
                board[move.row, move.col] = 0;

                bestOpponentScore = Math.Max(bestOpponentScore, score);
            }

            return bestOpponentScore / 2; // 가중치 감소
        }

        /// <summary>
        /// 후보 수 목록을 반환합니다 (평가 점수 기준 정렬)
        /// </summary>
        private List<(int row, int col)> GetCandidateMoves(int[,] board)
        {
            var moves = new List<(int row, int col, int score)>();
            bool isEmpty = IsEmptyBoard(board);

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
                        int score = CalculatePositionScore(board, row, col);
                        moves.Add((row, col, score));
                    }
                }
            }

            // 점수 높은 순으로 정렬 (최대 15개)
            return moves.OrderByDescending(m => m.score)
            .Take(15)
                       .Select(m => (m.row, m.col))
             .ToList();
        }

        /// <summary>
        /// 위치의 간단한 점수 계산
        /// </summary>
        private int CalculatePositionScore(int[,] board, int row, int col)
        {
            int score = 0;
            int center = BOARD_SIZE / 2;

            // 중앙 보너스
            int distanceFromCenter = Math.Abs(row - center) + Math.Abs(col - center);
            score += Math.Max(0, 14 - distanceFromCenter);

            // 인접 돌 개수
            int adjacentStones = CountAdjacentStones(board, row, col);
            score += adjacentStones * 5;

            return score;
        }

        /// <summary>
        /// 인접한 돌의 개수를 셉니다
        /// </summary>
        private int CountAdjacentStones(int[,] board, int row, int col)
        {
            int count = 0;
            for (int dr = -1; dr <= 1; dr++)
            {
                for (int dc = -1; dc <= 1; dc++)
                {
                    if (dr == 0 && dc == 0) continue;

                    int r = row + dr;
                    int c = col + dc;

                    if (r >= 0 && r < BOARD_SIZE && c >= 0 && c < BOARD_SIZE && board[r, c] != 0)
                    {
                        count++;
                    }
                }
            }
            return count;
        }

        /// <summary>
        /// 주변에 돌이 있는지 확인 (반경 2칸)
        /// </summary>
        private bool HasNearbyStone(int[,] board, int row, int col)
        {
            for (int dr = -2; dr <= 2; dr++)
            {
                for (int dc = -2; dc <= 2; dc++)
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
        /// 랜덤한 유효한 수를 반환합니다
        /// </summary>
        private (int row, int col) GetRandomMove(int[,] board)
        {
            var validMoves = new List<(int row, int col)>();

            for (int row = 0; row < BOARD_SIZE; row++)
            {
                for (int col = 0; col < BOARD_SIZE; col++)
                {
                    if (board[row, col] == 0)
                    {
                        validMoves.Add((row, col));
                    }
                }
            }

            if (validMoves.Count == 0)
            {
                return (0, 0);
            }

            return validMoves[random.Next(validMoves.Count)];
        }
    }
}
