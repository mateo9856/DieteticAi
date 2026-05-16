---
name: ai-developer
description: Develop new app features, improve existing behavior, refactor safely, and optimize application logic in the DieteticAi repository. Use for implementation tasks touching the .NET API, Blazor WebAssembly UI, Semantic Kernel diet generation service, RabbitMQ messaging library, validation/handler logic, DTOs, tests, or cross-project feature flows.
---

# AI Developer

## Core Workflow

1. Inspect the current implementation before editing. Start from the user-facing behavior, then trace the relevant API endpoint or command handler, service, messaging path, kernel logic, UI page/service, and tests.
2. Keep changes aligned with the existing architecture:
   - `DietAI.Api`: ASP.NET Core API, MediatR commands, FluentValidation, EF Core, authentication, and RabbitMQ integration.
   - `DieteticAI.UI`: Blazor WebAssembly pages, layout, UI services, request models, and app configuration.
   - `DieteticAi.Kernel`: Semantic Kernel setup, AI model selection, diet prompt/plugin logic, and generated plan processing.
   - `DietAI.RabbitServer`: RabbitMQ connection, sender, receiver, and topic abstractions.
   - `DietAI.Tests`: NUnit tests using FluentAssertions and NSubstitute.
3. Make the smallest coherent change that completes the feature. Avoid broad rewrites unless the current design blocks correctness.
4. Add or update tests for changed behavior. Prefer focused unit tests around handlers, validators, services, topic logic, and plugin transformations; broaden coverage when changing cross-project contracts.
5. Run the narrowest useful verification first, then expand if the change affects shared contracts:
   - `dotnet test DietAI.Tests/`
   - `dotnet build DieteticAi.sln`
   - project-specific `dotnet test` or `dotnet build` when faster and sufficient.

## Feature Implementation Guidance

- Preserve request and response contracts across UI, API, RabbitMQ messages, and Kernel models. If a DTO changes, update every project that creates, validates, serializes, deserializes, or displays it.
- Keep validation close to inputs. Use FluentValidation for API command rules and local UI validation or guard logic for client-only constraints.
- Treat asynchronous messaging as a contract boundary. Check topic names, correlation data, payload shape, serializer behavior, cancellation, and exception handling when modifying plan generation flows.
- Keep AI prompt and plugin changes deterministic where possible. Separate prompt text changes from parsing or model-selection changes so failures are easier to diagnose.
- Prefer dependency injection and existing abstractions over direct construction of services, clients, or kernels.
- Preserve nullable annotations and avoid using null-forgiving operators unless the invariant is obvious and already enforced nearby.

## Optimization Guidance

- Identify the bottleneck before optimizing. Inspect loops, async waits, repeated serialization, RabbitMQ connection/channel lifecycle, Kernel calls, database queries, and UI re-render paths.
- Prefer correctness-preserving optimizations with clear scope: reduce duplicate work, reuse existing services appropriately, avoid blocking calls in async paths, and make expensive AI or messaging work explicit.
- Do not cache user-specific diet data, generated plan content, credentials, tokens, or model responses unless invalidation and privacy implications are handled.
- Add regression tests or targeted assertions when optimizing logic that changes ordering, concurrency, retries, or error handling.

## Frontend Guidance

- Build actual usable screens and controls, not explanatory placeholder pages.
- Follow the existing Blazor component style and CSS organization before introducing new patterns.
- Keep forms and result views ergonomic for repeated diet-plan work: clear labels, predictable validation feedback, loading/error states, and responsive layouts.
- Avoid UI text that explains implementation details, keyboard shortcuts, styling choices, or internal feature mechanics.

## Verification Checklist

Before finishing:

1. Confirm all touched projects compile.
2. Confirm tests cover the changed behavior or explain the residual gap.
3. Check for broken cross-project model mappings and mismatched enum values.
4. Check appsettings or secrets changes are not required, or document the exact setting if they are.
5. Summarize changed files and the verification command results.
