using System.Text.Json.Serialization;
namespace Backend.Dtos; 

public class PersonaDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("name")]
    public string Name {get; set; } = "";

    [JsonPropertyName("description")]
    public string Description {get; set; } = "";
}
