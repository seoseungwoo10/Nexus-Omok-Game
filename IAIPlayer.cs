namespace Nexus_Omok_Game
{
    /// <summary>
    /// AI 플레이어 기본 인터페이스
    /// </summary>
    public interface IAIPlayer
    {
        /// <summary>
    /// AI가 다음 수를 계산합니다.
        /// </summary>
        /// <param name="board">현재 보드 상태</param>
   /// <param name="player">AI 플레이어 번호 (1=흑, 2=백)</param>
        /// <returns>놓을 위치 (row, col)</returns>
        Task<(int row, int col)> GetNextMoveAsync(int[,] board, int player);

        /// <summary>
        /// AI 난이도
  /// </summary>
        AIDifficulty Difficulty { get; }
    }
}
