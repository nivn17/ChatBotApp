# ChatBotApp

A small full-stack chatbot built as **two cooperating services**:

- **`Backend/`** — an **ASP.NET Core (.NET 10)** minimal API that owns conversation
  state and persists every message with **Entity Framework Core + SQLite**. It
  orchestrates the AI call over HTTP.
- **`python-ai/`** — a **FastAPI** service that calls the **OpenAI API**
  (`gpt-4o-mini`) and returns the assistant's reply.

```
client ──POST /chat──▶ .NET API ──POST /ai/chat──▶ Python (FastAPI) ──▶ OpenAI
                          │
                          ▼
                   SQLite (conversations, messages)
```

## Tech stack
- C# / ASP.NET Core (.NET 10), Entity Framework Core, SQLite, OpenAPI
- Python, FastAPI, OpenAI SDK
- HTTP service-to-service communication (typed `HttpClient`)

## Getting started

### Prerequisites
- .NET 10 SDK
- Python 3.10+
- An OpenAI API key

### 1. Start the Python AI service
```bash
cd python-ai
python -m venv .venv
.venv\Scripts\activate            # Windows  (macOS/Linux: source .venv/bin/activate)
pip install -r requirements.txt

# set your key (PowerShell):
$env:OPENAI_API_KEY = "sk-..."    # bash: export OPENAI_API_KEY="sk-..."

uvicorn main:app --port 8000
```

### 2. Start the .NET backend
```bash
cd Backend
dotnet restore

# create the SQLite database from EF Core migrations (first run only):
dotnet tool install --global dotnet-ef   # if you don't have it yet
dotnet ef database update

dotnet run                                # serves http://localhost:5173
```
The backend reads the Python service URL from `appsettings.json`
(`PythonService:BaseUrl`, default `http://127.0.0.1:8000`).

## API

### .NET backend
| Method | Route | Description |
| ------ | ----- | ----------- |
| `POST` | `/chat` | Body `{ "message": "...", "conversationId": 1 }` (`conversationId` optional). Creates or continues a conversation, stores both messages, returns the AI answer. |
| `GET`  | `/chat/{conversationId}` | Returns the message history for a conversation. |
| `GET`  | `/health` | Health check. |
| `GET`  | `/whoami` | Backend identity/info. |

### Python AI service
| Method | Route | Description |
| ------ | ----- | ----------- |
| `POST` | `/ai/chat` | Body `{ "message": "...", "conversation_id": 1 }` → `{ "answer": "..." }` |
| `GET`  | `/health` | Health check. |

## Notes
- **No secrets are committed.** The OpenAI key is read from the `OPENAI_API_KEY`
  environment variable; copy `python-ai/.env.example` to `.env` for local use.
- The SQLite database (`chat.db`) is generated locally and is gitignored.
