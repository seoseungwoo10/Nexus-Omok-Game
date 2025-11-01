namespace Nexus_Omok_Game
{
    /// <summary>
    /// 쉬움 난이도 AI - 랜덤 + 기본 방어
    /// </summary>
    public class EasyAI : IAIPlayer
    {
        private const int BOARD_SIZE = 15;
        private readonly Random random = new Random();

        public AIDifficulty Difficulty => AIDifficulty.Easy;

        public async Task<(int row, int col)> GetNextMoveAsync(int[,] board, int player)
        {
            return await Task.Run(() =>
            {
    int opponent = player == 1 ? 2 : 1;

         // 30% 확률로 상대의 4목을 방어
  if (random.Next(100) < 30)
       {
         var blockMove = FindBlockingMove(board, opponent);
        if (blockMove.HasValue)
             return blockMove.Value;
                }

           // 70% 확률로 랜덤 수
    return GetRandomMove(board);
            });
        }

        /// <summary>
  /// 상대의 4목을 막는 수를 찾습니다
        /// </summary>
        private (int row, int col)? FindBlockingMove(int[,] board, int opponent)
        {
    int[][] directions = new int[][]
 {
         new int[] { 0, 1 },   // 가로
        new int[] { 1, 0 },   // 세로
        new int[] { 1, 1 },   // 대각선 \
         new int[] { 1, -1 }   // 대각선 /
    };

            for (int row = 0; row < BOARD_SIZE; row++)
            {
   for (int col = 0; col < BOARD_SIZE; col++)
   {
     if (board[row, col] != 0) continue;

        // 이 위치에 상대 돌을 놓았다고 가정
           board[row, col] = opponent;

   // 상대가 이기는지 확인
                if (CheckWin(board, row, col, opponent))
     {
           board[row, col] = 0;
   return (row, col);
       }

    board[row, col] = 0;
 }
            }

            return null;
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
    return (0, 0);

            return validMoves[random.Next(validMoves.Count)];
 }

        /// <summary>
        /// 승리 조건 확인
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
         return true;
    }

     return false;
}
    }
}
