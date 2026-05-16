# DieteticAi Project

## Overview
This repository contains the source code for the DieteticAi application, which provides AI-driven dietary planning and analysis capabilities.

## Architecture
- **DietAI.Api** – REST API layer handling client requests.
- **DieteticAi.Kernel** – Core business logic and models.
- **DieteticAi.RabbitServer** – Message queue and background job processing.
- **DieteticAi.UI** – User interface components.

## Claude Code Integration
This project uses Claude Code for various automation tasks. Agent definitions are stored in the `agents/` directory.

### Agents
When a task matches one of the agents below, read the corresponding file and follow its instructions.

| Agent file | When to use |
|---|---|
| `agents/agent-name-folder/your-agent.md` | Replace with your actual agent file and description |
| Agent folders |
| `ai-developer` | `ai-project-manager` |

> **How to add a new agent:** create a markdown file in `agents/agent-name-folder`, then add a row to the table above
> with the filename and a short description of when to use it.

## Development Workflow
1. **Build** – Run `dotnet build` in the solution directory to restore and compile all projects.
2. **Test** – Execute unit tests using `dotnet test` in the `DietAI.Tests` project.
3. **Run** – Start the API with `dotnet run` in `DietAI.Api` or the UI with appropriate project commands.
4. **Deploy** – Use Docker Compose (`docker-compose up`) for local containerized deployment.

## Configuration
- Development and production settings are stored in `appsettings.Development.json`, `appsettings.Production.json`, and `appsettings.json`.
- Environment-specific configurations can be overridden via environment variables.

## Security
- Ensure sensitive data (API keys, connection strings) is stored securely and not committed to version control.
- Review security posture regularly, especially around authentication and data storage.

## Extending the Project
- To add new features, create separate branches, implement changes, and submit pull requests following the established PR template.
- For API contract changes, update the OpenAPI spec in the relevant project and regenerate client stubs.

## Troubleshooting
- Common issues include missing dependencies, misconfigured environment variables, and port conflicts.
- Check logs in the `logs/` directory (if present) or use `dotnet` runtime output for detailed error messages.

---

*Generated for internal use with Claude Code. For further details, refer to the `agents/` directory for agent definitions.*
