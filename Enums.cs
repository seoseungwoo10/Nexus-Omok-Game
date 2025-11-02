namespace Nexus_Omok_Game
{
    /// <summary>
    /// 게임 모드
    /// </summary>
    public enum GameMode
    {
     TwoPlayer,  // 2인 플레이
        VsAI        // AI 상대
    }

    /// <summary>
    /// AI 난이도 열거형
    /// </summary>
    public enum AIDifficulty
    {
    Easy,       // 쉬움 - 랜덤 + 기본 방어
      Normal,     // 보통 - 휴리스틱 평가
        Hard,       // 어려움 - 미니맥스 + 알파-베타
        ChatGPT,    // ChatGPT - OpenAI API 기반
        Rapfi       // Rapfi - 세계 최강 오목 엔진 ⭐ 신규
    }

    /// <summary>
    /// Rapfi 엔진 강도 레벨
    /// </summary>
    public enum RapfiStrength
    {
  Beginner,       // 초급 - 깊이 8, 1초, 2칸 범위
        Intermediate,   // 중급 - 깊이 12, 2초, 3칸 범위
        Advanced,       // 고급 - 깊이 16, 5초, 3.5칸 범위
  Master,  // 마스터 - 무제한 깊이, 10초, 전체 보드
  GrandMaster     // 그랜드마스터 - 무제한 깊이, 30초, 전체 보드 (프로 수준)
    }
}
