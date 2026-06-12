using Backend.Models;
using Backend.Data;
using Backend.Dtos;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddHttpClient("Python", client=>
{
    var baseUrl = builder.Configuration["PythonService:BaseUrl"];
    client.BaseAddress = new Uri(baseUrl!);
});

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Apply EF Core migrations on startup so the SQLite database is created automatically.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

//app.UseHttpsRedirection();

app.MapGet("/chat/{conversationId:int}", async (int conversationId, AppDbContext db) =>
{
    var messages = await db.Messages
        .Where(m => m.ConversationId == conversationId)
        .OrderBy(m => m.CreatedAt)
        .Select(m => new { m.Role, m.Content, m.CreatedAt })
        .ToListAsync();

    return Results.Ok(messages);
});


app.MapGet("/whoami", () =>
{
    return Results.Ok(new
    {
        app = ".NET Backend",
        machine = Environment.MachineName,
        time = DateTime.Now.ToString("O")
    });
});

app.MapPost("/chat", async (ChatRequest req, AppDbContext db, IHttpClientFactory http) =>
{
    if (string.IsNullOrWhiteSpace(req.Message))
        return Results.BadRequest(new { error = "Message is required" });

    Conversation conversation;

    if (req.ConversationId.HasValue)
    {
        conversation = await db.Conversations.FindAsync(req.ConversationId.Value);
        if (conversation == null)
            return Results.NotFound(new { error = "Conversation not found" });
    }
    else
    {
        conversation = new Conversation();
        db.Conversations.Add(conversation);
        await db.SaveChangesAsync(); // generates conversation.Id
    }

    db.Messages.Add(new Message
    {
        ConversationId = conversation.Id,
        Role = "user",
        Content = req.Message
    });

    var httpClientFactory = app.Services.GetRequiredService<IHttpClientFactory>();
    var client = http.CreateClient("Python");

    var pyRes = await client.PostAsJsonAsync("/ai/chat",new PythonChatRequest
    {
        ConversationId = conversation.Id,
        Message=req.Message
    });

if (!pyRes.IsSuccessStatusCode)
{
    var errorText = await pyRes.Content.ReadAsStringAsync();
    return Results.Problem($"Python failed: {(int)pyRes.StatusCode} {pyRes.StatusCode}. {errorText}");
}
    var pyData = await pyRes.Content.ReadFromJsonAsync<PythonChatResponse>();
    var answer = pyData?.answer ?? "No Answer from AI Service";

    db.Messages.Add(new Message
    {
        ConversationId = conversation.Id,
        Role = "assistant",
        Content = answer
    });

    await db.SaveChangesAsync();

    return Results.Ok(new { conversationId = conversation.Id, answer });
});


app.MapGet("/health", ()=>
{
    return Results.Ok(new {status= "ok"});
});

app.Run();

