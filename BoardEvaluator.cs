namespace Nexus_Omok_Game
{
    /// <summary>
    /// 오목 보드 상태를 평가하는 클래스
    /// </summary>
 public class BoardEvaluator
    {
        private const int BOARD_SIZE = 15;

        // 점수 가중치
        private const int WIN_SCORE = 100000;      // 5목 (즉시 승리)
        private const int FOUR_SCORE = 10000;      // 4목
        private const int OPEN_THREE_SCORE = 1000; // 열린 3목
    private const int THREE_SCORE = 100;       // 닫힌 3목
        private const int OPEN_TWO_SCORE = 50;     // 열린 2목
        private const int TWO_SCORE = 10;          // 닫힌 2목
        private const int ONE_SCORE = 1;     // 1목

     /// <summary>
        /// 보드 상태를 평가하여 점수를 반환합니다
   /// 양수: player에게 유리, 음수: opponent에게 유리
        /// </summary>
        public int Evaluate(int[,] board, int player)
     {
            int opponent = player == 1 ? 2 : 1;

            // 플레이어 점수 계산
 int playerScore = EvaluatePlayer(board, player);
            int opponentScore = EvaluatePlayer(board, opponent);

     // 상대방 점수는 더 높은 가중치로 감점 (방어 중시)
       return playerScore - (int)(opponentScore * 1.1);
        }

    /// <summary>
        /// 특정 플레이어의 보드 상태 점수 계산
     /// </summary>
        private int EvaluatePlayer(int[,] board, int player)
        {
      int score = 0;

            // 4방향 검사
      int[][] directions = new int[][]
      {
       new int[] { 0, 1 },  // 가로
      new int[] { 1, 0 },  // 세로
   new int[] { 1, 1 },  // 대각선 \
          new int[] { 1, -1 }  // 대각선 /
            };

            for (int row = 0; row < BOARD_SIZE; row++)
     {
   for (int col = 0; col < BOARD_SIZE; col++)
 {
         if (board[row, col] == player)
      {
    foreach (var dir in directions)
          {
     score += EvaluateLine(board, row, col, dir[0], dir[1], player);
        }
   }
       }
   }

            // 중앙 위치 보너스
            score += EvaluatePosition(board, player);

         return score;
   }

        /// <summary>
        /// 특정 방향의 라인을 평가
        /// </summary>
        private int EvaluateLine(int[,] board, int row, int col, int dRow, int dCol, int player)
        {
       int count = 1;  // 현재 위치 포함
            bool leftOpen = false;
            bool rightOpen = false;

 // 정방향 카운트
       int r = row + dRow;
 int c = col + dCol;
       while (r >= 0 && r < BOARD_SIZE && c >= 0 && c < BOARD_SIZE && board[r, c] == player)
      {
 count++;
      r += dRow;
                c += dCol;
            }
  // 정방향 끝 확인
       if (r >= 0 && r < BOARD_SIZE && c >= 0 && c < BOARD_SIZE && board[r, c] == 0)
      {
    rightOpen = true;
      }

   // 역방향 카운트 (중복 방지 위해 현재 위치는 제외)
            r = row - dRow;
   c = col - dCol;
       while (r >= 0 && r < BOARD_SIZE && c >= 0 && c < BOARD_SIZE && board[r, c] == player)
            {
         count++;
  r -= dRow;
   c -= dCol;
 }
  // 역방향 끝 확인
            if (r >= 0 && r < BOARD_SIZE && c >= 0 && c < BOARD_SIZE && board[r, c] == 0)
     {
         leftOpen = true;
       }

    // 중복 계산 방지: 시작점에서만 점수 부여
            if (dRow > 0 || (dRow == 0 && dCol > 0) || (dRow == 1 && dCol == -1 && col > row))
            {
     return GetScore(count, leftOpen, rightOpen);
   }

            return 0;
        }

        /// <summary>
        /// 연속 개수와 열림 상태에 따른 점수 반환
        /// </summary>
        private int GetScore(int count, bool leftOpen, bool rightOpen)
        {
            if (count >= 5)
            {
             return WIN_SCORE;
    }

            bool isOpen = leftOpen && rightOpen;
            bool isHalfOpen = leftOpen || rightOpen;

    return count switch
       {
                4 when isOpen => FOUR_SCORE,
      4 when isHalfOpen => FOUR_SCORE / 2,
    3 when isOpen => OPEN_THREE_SCORE,
   3 when isHalfOpen => THREE_SCORE,
  2 when isOpen => OPEN_TWO_SCORE,
        2 when isHalfOpen => TWO_SCORE,
   1 when isHalfOpen => ONE_SCORE,
     _ => 0
     };
        }

        /// <summary>
        /// 위치 평가 (중앙 보너스)
        /// </summary>
        private int EvaluatePosition(int[,] board, int player)
   {
      int score = 0;
            int center = BOARD_SIZE / 2;

      for (int row = 0; row < BOARD_SIZE; row++)
        {
   for (int col = 0; col < BOARD_SIZE; col++)
    {
    if (board[row, col] == player)
    {
       // 중앙에 가까울수록 높은 점수
        int distance = Math.Abs(row - center) + Math.Abs(col - center);
  score += Math.Max(0, 14 - distance);
         }
 }
            }

   return score;
        }

        /// <summary>
        /// 즉시 승리 가능한지 확인
        /// </summary>
      public bool IsWinningMove(int[,] board, int row, int col, int player)
        {
            board[row, col] = player;
    bool isWin = CheckWin(board, row, col, player);
         board[row, col] = 0;
       return isWin;
        }

     /// <summary>
      /// 승리 조건 확인 (정확히 5개)
        /// </summary>
      private bool CheckWin(int[,] board, int lastRow, int lastCol, int player)
        {
        int[][] directions = new int[][]
    {
         new int[] { 0, 1 },
                new int[] { 1, 0 },
           new int[] { 1, 1 },
      new int[] { 1, -1 }
            };

            foreach (var dir in directions)
    {
    int count = 1;

       int r = lastRow + dir[0];
          int c = lastCol + dir[1];
    while (r >= 0 && r < BOARD_SIZE && c >= 0 && c < BOARD_SIZE && board[r, c] == player)
          {
  count++;
          r += dir[0];
        c += dir[1];
                }

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
    }
}
