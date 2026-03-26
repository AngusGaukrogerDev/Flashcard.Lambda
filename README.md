# Flashcards Backend

A .NET 10 serverless backend for a flashcard application, built with AWS Lambda. The project follows a clean architecture structure with discrete Lambda function handlers for deck and card operations.

## Architecture

The solution is organised into the following projects:

| Project | Description |
|---|---|
| `Flashcards.Functions` | AWS Lambda function handlers for deck and card operations |
| `Flashcards.Domain` | Core domain entities and business rules |
| `Flashcards.Application` | Application logic and use cases |
| `Flashcards.Infrastructure` | Data access and external service integrations |
| `Flashcards.Application.Tests` | Unit tests for the application layer |
| `Flashcards.Domain.Tests` | Unit tests for the domain layer |
| `Flashcards.Infrastructure.Tests` | Unit tests for the infrastructure layer |

## Lambda Functions

### Deck

| Class | Description |
|---|---|
| `CreateDeckFunction` | Create a new flashcard deck |
| `GetDeckFunction` | Retrieve a deck |
| `UpdateDeckFunction` | Update an existing deck |
| `ReviewDeckFunction` | Start a review session for a deck |

### Card

| Class | Description |
|---|---|
| `AddCardToDeckFunction` | Add a new card to a deck |
| `UpdateCardFunction` | Update an existing card |
| `RemoveCardFromDeckFunction` | Remove a card from a deck |
| `DeleteCardFunction` | Permanently delete a card |

## Tech Stack

- **.NET 10**
- **AWS Lambda** (`Amazon.Lambda.Core`, `Amazon.Lambda.Serialization.SystemTextJson`)
- **xUnit** (testing)

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download)
- [AWS CLI](https://aws.amazon.com/cli/) (for deployment)
- [Amazon.Lambda.Tools](https://github.com/aws/aws-extensions-for-dotnet-cli) (for deployment)

```bash
dotnet tool install -g Amazon.Lambda.Tools
```

## Getting Started

### Build

```bash
dotnet build Flashcards.sln
```

### Run tests

```bash
dotnet test Flashcards.sln
```

## Deployment

### Deploy an individual Lambda function

```bash
cd Flashcards.Functions
dotnet lambda deploy-function
```

`Flashcards.Functions/aws-lambda-tools-defaults.json` contains the default deployment configuration (runtime `dotnet10`, 512 MB memory, 30 s timeout). Ensure your AWS profile and region are configured before deploying.

## Project Status

This project is in early development. The clean architecture scaffolding (Domain, Application, Infrastructure) is in place and the Lambda function handlers for all deck and card operations have been stubbed out. Core business logic and persistence are yet to be implemented.
