namespace Nexus_Omok_Game
{
    /// <summary>
    /// AI 플레이어 팩토리
    /// </summary>
 public static class AIPlayerFactory
    {
        public static IAIPlayer Create(AIDifficulty difficulty)
        {
            return difficulty switch
            {
      AIDifficulty.Easy => new EasyAI(),
      AIDifficulty.Normal => new EasyAI(), // TODO: Phase 3 - NormalAI 구현 예정
        AIDifficulty.Hard => new EasyAI(),   // TODO: Phase 4 - HardAI 구현 예정
           _ => new EasyAI() // 기본값
            };
    }
    }
}
