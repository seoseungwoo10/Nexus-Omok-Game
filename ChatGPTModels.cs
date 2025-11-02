using System.Text.Json.Serialization;

namespace Nexus_Omok_Game
{
    /// <summary>
    /// OpenAI API 응답 DTO
  /// </summary>
    public record ChatGPTResponse(
    [property: JsonPropertyName("choices")] ChatGPTChoice[] Choices
    );

    public record ChatGPTChoice(
        [property: JsonPropertyName("message")] ChatGPTMessage Message
    );

    public record ChatGPTMessage(
  [property: JsonPropertyName("content")] string Content
    );

    /// <summary>
    /// ChatGPT 수 응답 DTO
    /// </summary>
    public record ChatGPTMoveResponse(
        [property: JsonPropertyName("move")] Move Move,
        [property: JsonPropertyName("reasoning")] string Reasoning,
        [property: JsonPropertyName("alternatives")] Alternative[] Alternatives
    );

    public record Move(
        [property: JsonPropertyName("row")] int Row,
        [property: JsonPropertyName("col")] int Col
    );

    public record Alternative(
     [property: JsonPropertyName("row")] int Row,
      [property: JsonPropertyName("col")] int Col,
        [property: JsonPropertyName("score")] int Score
    );
}
