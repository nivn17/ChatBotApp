using System.Text.Json.Serialization;
namespace Backend.Dtos;

public class PythonChatResponse
{
    [JsonPropertyName("answer")]
    public string answer { get; set; } = "";
}