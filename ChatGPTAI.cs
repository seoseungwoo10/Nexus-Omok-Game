using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Windows;

namespace Nexus_Omok_Game
{
    /// <summary>
    /// OpenAI ChatGPT를 활용한 AI 플레이어
    /// </summary>
    public class ChatGPTAI : IAIPlayer
    {
        private const int BOARD_SIZE = 15;
        private const string API_ENDPOINT = "https://api.openai.com/v1/chat/completions";
        private const string MODEL = "gpt-4o-mini"; // 비용 효율적

        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly Random _random = new();

        public AIDifficulty Difficulty => AIDifficulty.ChatGPT;

        /// <summary>
        /// AI의 마지막 수에 대한 전략 설명
        /// </summary>
        public string? LastMoveReasoning { get; private set; }

        public ChatGPTAI(string apiKey)
        {
            _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(30)
            };
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
        }

        /// <summary>
        /// AI의 다음 수를 비동기로 계산합니다
        /// </summary>
        public async Task<(int row, int col)> GetNextMoveAsync(int[,] board, int player)
        {
            try
            {
                        // 사용량 체크
                if (!ApiUsageTracker.CanMakeRequest())
                {
                    MessageBox.Show(
                                $"월간 API 사용량 한도에 도달했습니다.\n" +
                                $"남은 요청: {ApiUsageTracker.GetRemainingRequests()}회\n" +
                                $"다음 달에 다시 사용하실 수 있습니다.",
                                "ChatGPT AI",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                    return GetFallbackMove(board, player);
                }

                        // 보드 상태를 문자열로 변환
                string boardState = ConvertBoardToString(board);

                         // ChatGPT API 호출
                var request = CreateChatRequest(boardState, player);
                var response = await _httpClient.PostAsJsonAsync(API_ENDPOINT, request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"OpenAI API Error ({response.StatusCode}): {errorContent}");
                }

                var result = await response.Content.ReadFromJsonAsync<ChatGPTResponse>();
                if (result == null || result.Choices.Length == 0)
                {
                    throw new Exception("Empty response from ChatGPT");
                }

                // JSON 응답 파싱
                var moveData = ParseMoveFromResponse(result);
                LastMoveReasoning = moveData.reasoning;

                // 사용량 증가
                ApiUsageTracker.IncrementUsage();

                // 유효성 검증
                if (IsValidMove(board, moveData.row, moveData.col))
                {
                    return (moveData.row, moveData.col);
                }

                // 폴백: 대안 수 시도
                foreach (var alt in moveData.alternatives)
                {
                    if (IsValidMove(board, alt.row, alt.col))
                    {
                        LastMoveReasoning += $"\n(원래 수가 무효하여 대안 사용: ({alt.row}, {alt.col}))";
                        return (alt.row, alt.col);
                    }
                }

                // 최후 폴백: 랜덤 유효한 수
                LastMoveReasoning = "ChatGPT가 무효한 수를 제공하여 랜덤 수를 선택했습니다.";
                return GetRandomValidMove(board);
            }
            catch (HttpRequestException ex)
            {
                // 네트워크 오류
                MessageBox.Show(
                    $"네트워크 오류가 발생했습니다.\n{ex.Message}\n\n로컬 AI로 전환합니다.",
                    "ChatGPT AI 오류",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                LastMoveReasoning = "네트워크 오류로 인해 로컬 AI가 대신 수를 선택했습니다.";
                return GetFallbackMove(board, player);
            }
            catch (TaskCanceledException)
            {
                // 타임아웃
                MessageBox.Show(
                    "API 응답 시간 초과 (30초).\n로컬 AI로 전환합니다.",
                    "ChatGPT AI 타임아웃",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                LastMoveReasoning = "응답 시간 초과로 인해 로컬 AI가 대신 수를 선택했습니다.";
                return GetFallbackMove(board, player);
            }
            catch (Exception ex)
            {
                // 기타 오류
                MessageBox.Show(
                    $"ChatGPT AI 오류:\n{ex.Message}\n\n로컬 AI로 전환합니다.",
                    "ChatGPT AI 오류",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                LastMoveReasoning = $"오류 발생: {ex.Message}";
                return GetFallbackMove(board, player);
            }
        }

        /// <summary>
        /// ChatGPT API 요청 객체 생성
        /// </summary>
        private object CreateChatRequest(string boardState, int player)
        {
            return new
            {
                model = MODEL,
                messages = new[]
                {
                    new
                    {
                        role = "system",
                        content = @"당신은 15x15 보드에서 플레이하는 세계 최고 수준의 오목(Gomoku) 그랜드마스터 AI입니다.
당신의 임무는 제공된 보드 상태를 분석하여, 승리를 위한 '단 하나의 최선의 수'를 찾는 것입니다.

## 1. 핵심 규칙 (반드시 준수)

* **승리 조건:** 정확히 5개의 돌(흑● 또는 백○)을 가로, 세로, 대각선으로 먼저 연결하면 승리합니다.
* **6목 이상 (오버라인):** 6개 이상의 돌이 연속으로 연결되는 것(예: OOOOOO)은 승리가 아닙니다. '장목'이라 부르며 게임은 계속됩니다.
* **흑(1)의 3-3 금지:** 흑돌(플레이어 1)은 한 번의 수로 2개 이상의 '열린 3'을 동시에 만들 수 없습니다.
    * '열린 3'이란 양쪽이 막히지 않아 '열린 4'로 발전할 수 있는 3목입니다. (예: ·OOO·)
* **백(2)의 3-3 허용:** 백돌(플레이어 2)은 3-3 금지 룰의 영향을 받지 않으며, 3-3을 승리 전략으로 사용할 수 있습니다.
* **착수:** 돌은 반드시 빈 칸(·)에만 놓아야 하며, 좌표는 0-14 범위입니다.

## 2. 승리를 위한 전략적 사고 (매우 중요)

당신은 반드시 다음의 우선순위에 따라 모든 가능한 수를 평가하고 최선의 수를 선택해야 합니다.

**[우선순위 1: 즉시 승리 (필승 공격)]**
* **(나의 5목)**: 이번 턴에 나의 돌 5개를 완성할 수 있는 자리가 있습니까? (예: ·XXXX· 또는 OXXXX·)
* **[결과]**: 있다면, 즉시 그곳에 두어 게임을 승리합니다. (Score 100)

**[우선순위 2: 즉각적 패배 방지 (필수 수비)]**
* **(상대의 4목)**: 상대방이 다음 턴에 5개를 완성할 수 있는 '열린 4'(·OOOO·)나 '닫힌 4'(XOOOO·)를 만들었습니까?
* **[결과]**: 있다면, **반드시** 그 자리를 막아야 합니다. 이것은 다른 어떤 수보다 중요합니다. (Score 95)

**[우선순위 3: 결정타 만들기 (포크 공격)]**
* **(나의 4-3 공격)**: 한 번의 수로 '열린 4'와 '열린 3'을 동시에 만들 수 있는 자리가 있습니까? (상대가 하나만 막을 수 있어 필승입니다.)
* **(나의 3-3 공격 - 백돌만 해당)**: (당신이 백돌일 경우) 한 번의 수로 2개 이상의 '열린 3'을 동시에 만들 수 있습니까?
* **[결과]**: 있다면, 이 '포크(Fork)' 공격을 실행합니다. (Score 90)
* **[흑돌 주의]**: 흑돌은 3-3 공격이 금지 룰에 해당하므로 절대 두어서는 안 됩니다.

**[우선순위 4: 강력한 위협 만들기 (공격)]**
* **(나의 열린 4)**: 상대방이 다음 턴에 무조건 막도록 강제하는 '열린 4'(·OOOO·)를 만들 수 있습니까? (Score 80)
* **(나의 열린 3)**: 다음 턴에 '4-3'이나 '열린 4'로 발전시킬 수 있는 '열린 3'(·OOO·)을 만들 수 있습니까? (Score 60)

**[우선순위 5: 상대 위협 제거 (전략적 수비)]**
* **(상대의 열린 3)**: 상대방의 '열린 3'을 미리 차단하여 '4-3'이나 '열린 4'로 발전하지 못하게 할 수 있습니까? (Score 70)

**[우선순위 6: 포석 (전략적 배치)]**
* 위의 사항이 모두 없을 경우, 중앙을 차지하거나, 내 돌들을 연결하거나, 상대의 돌을 끊는 자리에 둡니다. (Score 10-50)

## 3. 필수 JSON 응답 형식

응답은 다른 설명 없이 **반드시** 다음 JSON 형식으로만 제공해야 합니다. 'reasoning'에는 위의 '전략적 사고' 중 어떤 우선순위에 해당하는지 명시해야 합니다.

{
  ""move"": {""row"": 숫자(0-14), ""col"": 숫자(0-14)},
  ""reasoning"": ""한국어 설명 (예: '우선순위 2: 상대의 열린 4목(·OOOO·)을 막아 즉각적인 패배를 방지하는 필수 수비입니다.')"",
  ""alternatives"": [
    {""row"": 숫자, ""col"": 숫자, ""score"": 숫자(0-100)},
    {""row"": 숫자, ""col"": 숫자, ""score"": 숫자(0-100)}
  ]
}

## 4. 'alternatives' 점수 (score) 기준

'alternatives' 배열의 'score'는 위의 '전략적 사고' 우선순위에 따라 0-100점 사이의 점수를 부여하세요.
* **100:** 즉시 승리 (나의 5목)
* **95:** 상대의 4목을 막는 필수 수비
* **90:** 4-3 또는 (백의 경우) 3-3 포크 공격
* **80:** 나의 열린 4 만들기
* **70:** 상대의 열린 3 막기
* **60:** 나의 열린 3 만들기
* **10-50:** 기타 포석 (중앙 차지, 연결, 견제)
* **0:** 의미 없는 수 또는 금지된 수 (예: 흑의 3-3)"
                    },
                    new
                    {
                        role = "user",
                        content = $@"현재 보드 상태:
                        {boardState}

                        당신은 {(player == 1 ? "흑돌(●, 1)" : "백돌(○, 2)")}입니다.
                        최선의 수와 그 이유를 JSON 형식으로 제공하세요."
                    }
                },
                temperature = 0.7,
                max_tokens = 500,
                response_format = new { type = "json_object" }
            };
        }

        /// <summary>
             /// 보드 상태를 문자열로 변환
        /// </summary>
        private string ConvertBoardToString(int[,] board)
        {
            var sb = new StringBuilder();
            sb.AppendLine("   0 1 2 3 4 5 6 7 8 9 A B C D E");

            for (int i = 0; i < BOARD_SIZE; i++)
            {
                sb.Append($"{i:X} ");
                for (int j = 0; j < BOARD_SIZE; j++)
                {
                    char symbol = board[i, j] switch
                    {
                        0 => '·',
                        1 => '●',
                        2 => '○',
                        _ => '?'
                    };
                    sb.Append($" {symbol}");
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }

        /// <summary>
        /// ChatGPT 응답에서 수를 파싱
        /// </summary>
        private (int row, int col, string reasoning, List<(int row, int col, int score)> alternatives)
        ParseMoveFromResponse(ChatGPTResponse response)
        {
            try
            {
                var content = response.Choices[0].Message.Content;
                var moveData = JsonSerializer.Deserialize<ChatGPTMoveResponse>(content);

                if (moveData == null)
                {
                    throw new Exception("Failed to deserialize move data");
                }

                var alternatives = moveData.Alternatives
                    .Select(a => (a.Row, a.Col, a.Score))
                .ToList();

                return (
                  moveData.Move.Row,
               moveData.Move.Col,
               moveData.Reasoning,
                 alternatives
                      );
            }
            catch (JsonException ex)
            {
                throw new Exception($"Failed to parse ChatGPT response: {ex.Message}");
            }
        }

        /// <summary>
        /// 유효한 수인지 확인
        /// </summary>
        private bool IsValidMove(int[,] board, int row, int col)
        {
            return row >= 0 && row < BOARD_SIZE &&
                col >= 0 && col < BOARD_SIZE &&
                 board[row, col] == 0;
        }

        /// <summary>
        /// 랜덤한 유효한 수 반환
        /// </summary>
        private (int row, int col) GetRandomValidMove(int[,] board)
        {
            var validMoves = new List<(int, int)>();

            for (int i = 0; i < BOARD_SIZE; i++)
            {
                for (int j = 0; j < BOARD_SIZE; j++)
                {
                    if (board[i, j] == 0)
                    {
                        validMoves.Add((i, j));
                    }
                }
            }

            if (validMoves.Count == 0)
            {
                return (BOARD_SIZE / 2, BOARD_SIZE / 2);
            }

            return validMoves[_random.Next(validMoves.Count)];
        }

        /// <summary>
        /// 폴백 AI (HardAI 사용)
        /// </summary>
        private (int row, int col) GetFallbackMove(int[,] board, int player)
        {
            var fallbackAI = new HardAI();
            var task = fallbackAI.GetNextMoveAsync(board, player);
            task.Wait(); // 동기적으로 대기 (이미 비동기 컨텍스트 내부)
            return task.Result;
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
