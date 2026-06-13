from fastapi import FastAPI, Request
from openai import OpenAI
import os
from personas import list_personas, get_persona

app = FastAPI()

@app.get("/health")
def health():
    return {"status": "ok"}

@app.get("/ai/personas")
def personas():
    return list_personas()

@app.post("/ai/chat")
async def chat(request: Request):
    data = await request.json()
   
    persona_id = data.get("persona_id") or data.get("personaId") or data.get("PersonaId")
    persona = get_persona(persona_id)
   
    # Accept many possible field names from .NET
    message = data.get("message") or data.get("Message")
    conversation_id = (
        data.get("conversation_id")
        or data.get("conversationId")
        or data.get("ConversationId")
    )

    if not message:
        return {"answer": "(python) missing 'message' in body"}
    
    api_key = os.getenv("OPENAI_API_KEY")
    if not api_key:
        return {"answer": "OPENAI_API_KEY is not set for this Python process"}
    
    client = OpenAI(api_key=api_key)

    resp = client.responses.create(
        model="gpt-4.1-2025-04-14",
        instructions=persona["system_prompt"],
        input=message
    )

    return {"answer": resp.output_text}
