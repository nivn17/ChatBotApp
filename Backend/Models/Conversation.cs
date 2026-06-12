using System;
using System.Collections.Generic;

namespace Backend.Models;

public class Conversation
{
    public int Id {get; set;}
    public DateTime CreatedAt {get; set;} = DateTime.UtcNow;

    public List<Message> Messages {get; set;} = new();

}