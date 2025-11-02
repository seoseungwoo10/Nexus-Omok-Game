namespace Nexus_Omok_Game
{
    /// <summary>
    /// Gomocup 프로토콜 커맨드 목록
    /// </summary>
    public static class GomocupCommands
    {
        // 초기화
   public const string START = "START";           // 보드 크기 설정 (예: START 15)
public const string INFO = "INFO";   // 옵션 설정 (예: INFO timeout_turn 5000)

 // 게임 진행
  public const string BEGIN = "BEGIN";           // AI가 첫 수를 둠
      public const string TURN = "TURN";   // 상대방 수 통보 (예: TURN 7,7)
        public const string BOARD = "BOARD";     // 전체 보드 상태 설정

   // 분석 (확장 기능)
    public const string NBEST = "NBEST";// 다중 후보 분석 (예: NBEST 5)

     // 종료
public const string END = "END"; // 게임 종료
    }

    /// <summary>
    /// Gomocup 프로토콜 응답 목록
    /// </summary>
    public static class GomocupResponses
  {
        public const string OK = "OK";          // 준비 완료
     public const string ERROR = "ERROR";  // 오류 발생
   public const string MESSAGE = "MESSAGE";       // 메시지 출력
        public const string DEBUG = "DEBUG";    // 디버그 정보
        // 응답: "X,Y" 형식으로 좌표 반환 (예: "7,7")
    }

  /// <summary>
    /// Rapfi 엔진 설정 옵션
    /// </summary>
    public static class RapfiOptions
    {
        // 기본 설정
     public const string TIMEOUT_TURN = "timeout_turn";     // 턴당 시간 제한(ms)
  public const string TIMEOUT_MATCH = "timeout_match";   // 매치 전체 시간(ms)
        public const string MAX_MEMORY = "max_memory";         // 최대 메모리(byte)
        public const string TIME_LEFT = "time_left";      // 남은 시간(ms)

        // 게임 룰
        public const string GAME_TYPE = "game_type";           // 0=자유형, 1=표준, 2=렌주
        public const string RULE = "rule";      // 0=자유형, 1=표준, 4=렌주

        // 고급 설정
        public const string MAX_NODE = "max_node";             // 최대 탐색 노드
        public const string MAX_DEPTH = "max_depth";       // 최대 탐색 깊이
    public const string CAUTION_FACTOR = "caution_factor"; // 0~4 (수 선택 범위)
 public const string NBEST = "nbest";          // 다중 후보 분석 개수
 }
}
