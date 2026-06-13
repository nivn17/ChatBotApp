namespace Backend.Models;

public class ChatRequest
{
    //optional, if null, new conversation will be created
    public int? ConversationId {get; set;}

    //for user messages
    public string Message {get; set;} = string.Empty;

    public string? PersonaId {get; set;}
}