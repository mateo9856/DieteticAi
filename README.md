# DieteticAi 🥗

A simple diet planning app that uses Microsoft Semantic Kernel and Ollama LLM to generate personalized diet plans.

## What it does

- Takes user input (age, weight, sex)
- Searches for existing diet plans
- Generates new AI-powered plans using Ollama when needed
- Stores plans in memory

## Prerequisites

- .NET 8.0 SDK
- Ollama installed and running
- Llama3 model: `ollama pull llama3`

## How it works

The app uses Semantic Kernel to connect with Ollama's Llama3 model. It first tries to find an existing diet plan that matches your criteria. If none exists, it generates a new personalized plan using AI.

## Project Structure

```
DieteticAi/
├── Models/          # Data classes
├── Plugins/         # Semantic Kernel plugin
└── Program.cs       # Main entry point
```

## Dependencies

- Microsoft.SemanticKernel
- Microsoft.SemanticKernel.Connectors.Ollama

---

**Note**: This is a demo project.