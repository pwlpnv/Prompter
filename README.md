Prompter is a batch prompt processing system that sends user-submitted prompts to a local LLM (Ollama) and stores the
responses. It consists of a React frontend, an ASP.NET Core API, a background worker, PostgreSQL, and Ollama.

## Quick Start (Docker Compose)

```bash
# 1. Create .env from template
cp .env.template .env

# 2. Start all services
docker compose up --build
```

This starts:

- **Frontend** at http://localhost:3000
- **API** at http://localhost:5251 (Swagger at http://localhost:5251/swagger)
- **PostgreSQL** on port 5433
- **Ollama** on port 11434 (auto-pulls phi3 model on first run)

Database migrations run automatically on startup.

## Prerequisites

- Docker and Docker Compose
- .NET 8 SDK (for local development)
- Node.js 20+ (for local frontend development)

## Architecture

```
Prompter.Client  (React + Vite, port 3000)
    |
Prompter.Web     (ASP.NET Core API, port 5251)
    |
Prompter.Worker  (BackgroundService, polls for pending prompts)
    |
Ollama           (LLM inference, port 11434)
    |
PostgreSQL       (storage, port 5433)
```

**Projects:**

| Project                   | Role                                                 |
|---------------------------|------------------------------------------------------|
| `Prompter.Core`           | Entities, interfaces, enums                          |
| `Prompter.Data`           | EF Core DbContext, repositories, migrations          |
| `Prompter.Services`       | Application services, orchestrator, prompt processor |
| `Prompter.Infrastructure` | DI registration, Ollama LLM client                   |
| `Prompter.Web`            | API controllers, DTOs, validation                    |
| `Prompter.Worker`         | BackgroundService host for batch processing          |
| `Prompter.Client`         | React frontend (Vite + TypeScript)                   |
| `Prompter.Tests`          | Unit tests (xUnit, NSubstitute, FluentAssertions)    |

## Tests

```bash
dotnet test
```

## API Endpoints

| Method | Endpoint                          | Description                                            |
|--------|-----------------------------------|--------------------------------------------------------|
| `POST` | `/api/prompts`                    | Submit prompts (1-50 per request, max 4000 chars each) |
| `GET`  | `/api/prompts?page=1&pageSize=20` | List prompts (paginated, pageSize max 100)             |

### Example: Submit prompts

```bash
curl -X POST http://localhost:5251/api/prompts \
  -H "Content-Type: application/json" \
  -d '{"prompts": ["What is the capital of France?", "Explain recursion"]}'
```

## Configuration

Environment variables for the worker:

| Variable                               | Default                  | Description                    |
|----------------------------------------|--------------------------|--------------------------------|
| `Ollama__BaseUrl`                      | `http://localhost:11434` | Ollama API URL                 |
| `Ollama__Model`                        | `phi3`                   | LLM model name                 |
| `Ollama__MaxTokens`                    | `40`                     | Max output tokens per response |
| `ConnectionStrings__DefaultConnection` | —                        | PostgreSQL connection string   |
