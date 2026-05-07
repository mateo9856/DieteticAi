# DieteticAi 🥗

A comprehensive diet planning application that leverages Microsoft Semantic Kernel and AI models (Ollama or Gemini) to generate personalized diet plans. The app features a microservices architecture with a Blazor WebAssembly frontend, ASP.NET Core API, and a dedicated AI kernel service communicating via RabbitMQ.

## What it does

- Collects user input (age, weight, goals) via a web interface
- Authenticates users with JWT tokens
- Sends plan generation requests to an AI service via RabbitMQ
- Generates personalized diet plans using AI (Ollama Llama3 or Google Gemini)
- Stores and retrieves plans from a PostgreSQL database
- Displays plans in a user-friendly Blazor UI

## Prerequisites

- .NET 8.0/10.0 SDK
- PostgreSQL database
- RabbitMQ server (or Docker Compose for local setup)
- To choice: Ollama installed and running with Llama3 model: `ollama pull llama3` OR
- (Optional) Google Gemini API key for alternative AI model

## Architecture

The application consists of several microservices:

- **DieteticAI.UI**: Blazor WebAssembly frontend for user interaction
- **DietAI.Api**: ASP.NET Core Web API with JWT authentication, EF Core, and RabbitMQ integration
- **DieteticAi.Kernel**: AI service using Semantic Kernel to generate diet plans
- **DietAI.RabbitServer**: Shared RabbitMQ messaging library
- **DietAI.Tests**: Unit and integration tests

Services communicate asynchronously via RabbitMQ for plan generation requests and responses.

## How it works

1. Users input their details through the Blazor UI
2. The UI sends requests to the API with JWT authentication
3. The API publishes plan generation requests to RabbitMQ
4. The Kernel service consumes requests, uses Semantic Kernel with Ollama/Gemini to generate plans
5. Generated plans (JSON) are sent back via RabbitMQ
6. The API stores plans in PostgreSQL and returns them to the UI
7. Users view their personalized diet plans

## Project Structure

```
DieteticAi/
├── DietAI.Api/              # ASP.NET Core Web API
│   ├── Endpoints/           # API endpoints (v1)
│   ├── Services/            # Business logic services
│   ├── Data/                # EF Core DbContext
│   ├── Middleware/          # JWT middleware
│   └── Options/             # Configuration options
├── DieteticAI.UI/           # Blazor WebAssembly frontend
│   ├── Pages/               # UI pages
│   ├── Services/            # UI services
│   └── Layout/              # UI layout components
├── DieteticAi.Kernel/       # AI kernel service
│   ├── Plugins/             # Semantic Kernel plugins
│   ├── Services/            # Diet planning services
│   └── Models/              # Data models
├── DietAI.RabbitServer/     # RabbitMQ messaging library
├── DietAI.Tests/            # Unit and integration tests
├── compose.yaml             # Docker Compose for RabbitMQ
└── rabbitmq.conf            # RabbitMQ configuration
```

## Dependencies

- Microsoft.SemanticKernel
- Microsoft.SemanticKernel.Connectors.Ollama
- Microsoft.AspNetCore (for API and UI)
- Entity Framework Core with Npgsql
- RabbitMQ.Client
- MediatR for CQRS
- FluentValidation
- Serilog for logging

## Setup and Running

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd DieteticAi
   ```

2. **Setup PostgreSQL**
   - Install PostgreSQL
   - Create a database for the app
   - Update connection string in `DietAI.Api/appsettings.json`

3. **Setup RabbitMQ**
   - Option 1: Install RabbitMQ locally or get Docker image from Docker and run:
    ```bash
    docker run -it --rm --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:4-management
    ```
   - Option 2: Use Docker Compose:
     ```bash
     docker-compose up -d
     ```

4. **Setup LLM**
   Choose between Ollama or Google Gemini for AI plan generation:

   - **For Ollama**:
     - Install and run Ollama (see [Ollama installation](https://ollama.ai/))
     - Pull the Llama3 model:
       ```bash
       ollama pull llama3
       ```
     - In `DieteticAi.Kernel/appsettings.json`, set:
       ```json
       "SelectedModelType": "Ollama",
       "Ollama": {
         "Connection": "http://localhost:11434",
         "LlmModel": "llama3"
       }
       ```

   - **For Google Gemini**:
     - Obtain an API key from [Google AI Studio](https://makersuite.google.com/app/apikey)
     - In `DieteticAi.Kernel/appsettings.json`, set:
       ```json
       "SelectedModelType": "Gemini",
       "Gemini": {
         "ApiKey": "your-gemini-api-key-here",
         "LlmModel": "gemini-1.5-flash"
       }
       ```

5. **Configure appsettings**
   - Update `DietAI.Api/appsettings.json` with database and RabbitMQ settings
   - AI model configuration is handled in step 4 above

6. **Run the services**
   - Start the Kernel service: `dotnet run --project DieteticAi.Kernel`
   - Start the API: `dotnet run --project DietAI.Api`
   - Start the UI: `dotnet run --project DieteticAI.UI`

## Testing

Run tests with:
```bash
dotnet test DietAI.Tests/
```

## Contributing

This is 4fun evolving project in free time. Contributions for UI improvements, additional AI models, or new features are welcome.
