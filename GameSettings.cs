namespace Nexus_Omok_Game
{
    /// <summary>
    /// 게임 설정
    /// </summary>
    public class GameSettings
    {
        /// <summary>
   /// 게임 모드
        /// </summary>
     public GameMode Mode { get; set; } = GameMode.TwoPlayer;

        /// <summary>
        /// AI 난이도 (AI 모드일 때만 유효)
   /// </summary>
        public AIDifficulty AIDifficulty { get; set; } = AIDifficulty.Easy;

        /// <summary>
  /// 플레이어가 흑돌인지 여부
        /// </summary>
  public bool IsPlayerBlack { get; set; } = true;

        /// <summary>
   /// AI 사고 과정 표시 여부
 /// </summary>
  public bool ShowAIThinking { get; set; } = false;
    }
}
