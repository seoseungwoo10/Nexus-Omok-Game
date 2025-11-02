namespace Nexus_Omok_Game
{
    /// <summary>
    /// 쉬움 난이도 AI - 기본 전략 + 가끔 실수
    /// 목표: 초보자도 이길 수 있지만 어느 정도 도전적 (사용자 승률 40-50%)
    /// </summary>
    public class EasyAI : IAIPlayer
    {
        private const int BOARD_SIZE = 15;
        private readonly Random random = new();

        public AIDifficulty Difficulty => AIDifficulty.Easy;

        /// <summary>
        /// AI의 다음 수를 비동기로 계산합니다
        /// 전략: 승리 > 방어(80%) > 공격 > 중앙/연결 > 랜덤 (10% 확률로 실수)
        /// </summary>
        public async Task<(int row, int col)> GetNextMoveAsync(int[,] board, int player)
        {
            // 비동기 딜레이 (사람처럼 보이게)
            await Task.Delay(random.Next(300, 800));

            int opponent = player == 1 ? 2 : 1;

            // 10% 확률로 랜덤한 실수 (쉬움 난이도 특성)
            if (random.NextDouble() < 0.1)
            {
                return GetRandomMove(board);
            }

            // 1순위: 자신이 이길 수 있으면 즉시 승리 (100%)
            var winMove = FindWinningMove(board, player);
            if (winMove.HasValue)
            {
                return winMove.Value;
            }

            // 2순위: 상대의 4목을 막기 (80% 확률)
            if (random.NextDouble() < 0.8)
            {
                var blockFourMove = FindWinningMove(board, opponent);
                if (blockFourMove.HasValue)
                {
                    return blockFourMove.Value;
                }
            }

            // 3순위: 자신의 열린 3목 만들기 (양쪽이 열린 3목)
            var openThreeMove = FindOpenThreeMove(board, player);
            if (openThreeMove.HasValue)
            {
                return openThreeMove.Value;
            }

            // 4순위: 상대의 열린 3목 막기 (80% 확률)
            if (random.NextDouble() < 0.8)
            {
                var blockOpenThreeMove = FindOpenThreeMove(board, opponent);
                if (blockOpenThreeMove.HasValue)
                {
                    return blockOpenThreeMove.Value;
                }
            }

            // 5순위: 자신의 닫힌 3목 만들기
            var threeMove = FindThreeMove(board, player);
            if (threeMove.HasValue)
            {
                return threeMove.Value;
            }

            // 6순위: 상대의 닫힌 3목 막기 (60% 확률)
            if (random.NextDouble() < 0.6)
            {
                var blockThreeMove = FindThreeMove(board, opponent);
                if (blockThreeMove.HasValue)
                {
                    return blockThreeMove.Value;
                }
            }

            // 7순위: 자신의 2목 만들기
            var twoMove = FindTwoMove(board, player);
            if (twoMove.HasValue)
            {
                return twoMove.Value;
            }

            // 8순위: 중앙 근처의 좋은 위치
            var centerMove = FindCenterMove(board, player);
            if (centerMove.HasValue)
            {
                return centerMove.Value;
            }

            // 최후: 랜덤 수
            return GetRandomMove(board);
        }

        /// <summary>
        /// 승리할 수 있는 수를 찾습니다 (4목 → 5목)
        /// </summary>
        private (int row, int col)? FindWinningMove(int[,] board, int player)
        {
            for (int row = 0; row < BOARD_SIZE; row++)
            {
                for (int col = 0; col < BOARD_SIZE; col++)
                {
                    if (board[row, col] == 0)
                    {
                        // 이 위치에 두면 승리하는지 확인
                        board[row, col] = player;
                        bool isWin = CheckWin(board, row, col, player);
                        board[row, col] = 0;

                        if (isWin)
                        {
                            return (row, col);
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 열린 3목을 만들 수 있는 수를 찾습니다 (양쪽이 열림: _OOO_)
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
        /// 열린 3목인지 확인 (양쪽이 비어있는 3개 연속)
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
                // 정방향 끝이 비어있는지 확인
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
                // 역방향 끝이 비어있는지 확인
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
        /// 3목을 만들 수 있는 수를 찾습니다 (닫힌 3목 포함)
        /// </summary>
        private (int row, int col)? FindThreeMove(int[,] board, int player)
        {
            for (int row = 0; row < BOARD_SIZE; row++)
            {
                for (int col = 0; col < BOARD_SIZE; col++)
                {
                    if (board[row, col] == 0)
                    {
                        board[row, col] = player;
                        int maxCount = CountMaxInDirection(board, row, col, player);
                        board[row, col] = 0;

                        if (maxCount == 3)
                        {
                            return (row, col);
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 2목을 만들 수 있는 수를 찾습니다
        /// </summary>
        private (int row, int col)? FindTwoMove(int[,] board, int player)
        {
            for (int row = 0; row < BOARD_SIZE; row++)
            {
                for (int col = 0; col < BOARD_SIZE; col++)
                {
                    if (board[row, col] == 0)
                    {
                        board[row, col] = player;
                        int maxCount = CountMaxInDirection(board, row, col, player);
                        board[row, col] = 0;

                        if (maxCount == 2)
                        {
                            return (row, col);
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 중앙 근처의 좋은 위치를 찾습니다
        /// </summary>
        private (int row, int col)? FindCenterMove(int[,] board, int player)
        {
            // 중앙점 (7, 7)
            int centerRow = 7;
            int centerCol = 7;

            // 첫 수라면 중앙에 두기
            if (IsEmptyBoard(board))
            {
                return (centerRow, centerCol);
            }

            // 중앙 근처의 빈 공간 찾기
            var centerMoves = new List<(int row, int col, int distance)>();

            for (int row = 0; row < BOARD_SIZE; row++)
            {
                for (int col = 0; col < BOARD_SIZE; col++)
                {
                    if (board[row, col] == 0)
                    {
                        // 기존 돌과 인접한 곳만 고려 (연결 가능성)
                        if (HasAdjacentStone(board, row, col))
                        {
                            int distance = Math.Abs(row - centerRow) + Math.Abs(col - centerCol);
                            centerMoves.Add((row, col, distance));
                        }
                    }
                }
            }

            // 중앙에 가까운 순서로 정렬
            if (centerMoves.Count > 0)
            {
                centerMoves.Sort((a, b) => a.distance.CompareTo(b.distance));
                // 상위 5개 중 랜덤 선택
                int index = random.Next(Math.Min(5, centerMoves.Count));
                return (centerMoves[index].row, centerMoves[index].col);
            }

            return null;
        }

        /// <summary>
        /// 인접한 돌이 있는지 확인
        /// </summary>
        private bool HasAdjacentStone(int[,] board, int row, int col)
        {
            for (int dr = -1; dr <= 1; dr++)
            {
                for (int dc = -1; dc <= 1; dc++)
                {
                    if (dr == 0 && dc == 0) continue;

                    int r = row + dr;
                    int c = col + dc;

                    if (r >= 0 && r < BOARD_SIZE && c >= 0 && c < BOARD_SIZE)
                    {
                        if (board[r, c] != 0)
                        {
                            return true;
                        }
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
        /// 특정 위치에서 최대 연속 개수를 세기
        /// </summary>
        private int CountMaxInDirection(int[,] board, int lastRow, int lastCol, int player)
        {
            int[][] directions = new int[][]
            {
                new int[] { 0, 1 },  // 가로
                new int[] { 1, 0 },  // 세로
                new int[] { 1, 1 },  // 대각선 \
                new int[] { 1, -1 }  // 대각선 /
            };

            int maxCount = 0;

            foreach (var dir in directions)
            {
                int count = 1;

                // 정방향 카운트
                int r = lastRow + dir[0];
                int c = lastCol + dir[1];
                while (r >= 0 && r < BOARD_SIZE && c >= 0 && c < BOARD_SIZE && board[r, c] == player)
                {
                    count++;
                    r += dir[0];
                    c += dir[1];
                }

                // 역방향 카운트
                r = lastRow - dir[0];
                c = lastCol - dir[1];
                while (r >= 0 && r < BOARD_SIZE && c >= 0 && c < BOARD_SIZE && board[r, c] == player)
                {
                    count++;
                    r -= dir[0];
                    c -= dir[1];
                }

                maxCount = Math.Max(maxCount, count);
            }

            return maxCount;
        }

        /// <summary>
        /// 승리 조건 확인 (정확히 5개)
        /// </summary>
        private bool CheckWin(int[,] board, int lastRow, int lastCol, int player)
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

                // 정방향 카운트
                int r = lastRow + dir[0];
                int c = lastCol + dir[1];
                while (r >= 0 && r < BOARD_SIZE && c >= 0 && c < BOARD_SIZE && board[r, c] == player)
                {
                    count++;
                    r += dir[0];
                    c += dir[1];
                }

                // 역방향 카운트
                r = lastRow - dir[0];
                c = lastCol - dir[1];
                while (r >= 0 && r < BOARD_SIZE && c >= 0 && c < BOARD_SIZE && board[r, c] == player)
                {
                    count++;
                    r -= dir[0];
                    c -= dir[1];
                }

                if (count == 5)
                {
                    return true;
                }
            }

            return false;
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
                // 보드가 가득 찬 경우 (드물지만 안전장치)
                return (0, 0);
            }

            return validMoves[random.Next(validMoves.Count)];
        }
    }
}
