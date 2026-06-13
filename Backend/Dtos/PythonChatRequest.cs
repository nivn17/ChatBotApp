using System.Text.Json.Serialization;

namespace Backend.Dtos;

public class PythonChatRequest
{
    [JsonPropertyName("conversation_id")]
    public int ConversationId { get; set; }

    [JsonPropertyName("Message")]
    public string Message { get; set; } = "";

    [JsonPropertyName("persona_id")]
    public string? PersonaId { get; set; }
}