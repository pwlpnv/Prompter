Prompter is a batch prompt processing system that sends user-submitted prompts to a local LLM (Ollama) and stores the
responses. It consists of a React frontend, an ASP.NET Core API, a worker with MassTransit consumers, RabbitMQ, PostgreSQL, and Ollama.

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
- **RabbitMQ** on port 5672 (management UI at http://localhost:15672)
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
    |  publishes ProcessPrompt messages via MassTransit
    |
RabbitMQ         (message broker, port 5672)
    |
Prompter.Worker  (MassTransit consumers, processes prompts)
    |
Ollama           (LLM inference, port 11434)
    |
PostgreSQL       (storage, port 5433)
```

When a user submits prompts, the API saves them to PostgreSQL and publishes a `ProcessPrompt` message per prompt via MassTransit (using the EF Core transactional outbox for reliability). The Worker consumes these messages, calls Ollama, and updates the prompt status. Failed prompts are retried with delayed redelivery before being marked as Failed by a fault consumer.

**Projects:**

| Project                   | Role                                                         |
|---------------------------|--------------------------------------------------------------|
| `Prompter.Core`           | Entities, interfaces, enums, message contracts               |
| `Prompter.Data`           | EF Core DbContext, repositories, migrations, unit of work    |
| `Prompter.Services`       | Application services (publishes messages via MassTransit)    |
| `Prompter.Infrastructure` | DI registration, Ollama LLM client, MassTransit bus config   |
| `Prompter.Web`            | API controllers, DTOs, validation                            |
| `Prompter.Worker`         | MassTransit consumer host (ProcessPrompt, fault consumer)    |
| `Prompter.Client`         | React frontend (Vite + TypeScript)                           |
| `Prompter.Tests`          | Unit tests (xUnit, NSubstitute, FluentAssertions)            |

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

Environment variables (set via `docker-compose.yaml` or `.env`):

| Variable                               | Default                  | Description                    |
|----------------------------------------|--------------------------|--------------------------------|
| `Ollama__BaseUrl`                      | `http://localhost:11434` | Ollama API URL                 |
| `Ollama__Model`                        | `phi3`                   | LLM model name                 |
| `Ollama__MaxTokens`                    | `40`                     | Max output tokens per response |
| `RabbitMQ__Host`                       | `localhost`              | RabbitMQ hostname              |
| `ConnectionStrings__DefaultConnection` | —                        | PostgreSQL connection string   |

## Previous Implementation (Background Worker with DB Polling)

The first version of this project used a `BackgroundService` that polled PostgreSQL for pending prompts instead of MassTransit + RabbitMQ. That implementation is preserved under the `v1-polling` tag.

To check it out:

```bash
git checkout v1-polling
```

Key differences from the current version:
- Worker used `BackgroundService` with a polling loop (`SELECT ... FOR UPDATE SKIP LOCKED`)
- No RabbitMQ or MassTransit dependency
- No transactional outbox — prompts were picked up directly from the database by the worker
