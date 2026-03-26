# Flashcards Backend

A .NET 10 backend for a flashcard application, designed to run on AWS Lambda. The project follows a clean architecture structure and supports both an ASP.NET Core REST API and discrete Lambda function handlers.

## Architecture

The solution is organised into the following projects:

| Project | Description |
|---|---|
| `Flashcards.Api` | ASP.NET Core web API hosted on AWS Lambda via API Gateway |
| `Flashcards.Functions` | Individual AWS Lambda function handlers for deck and card operations |
| `Flashcards.Domain` | Core domain entities and business rules |
| `Flashcards.Application` | Application logic and use cases |
| `Flashcards.Infrastructure` | Data access and external service integrations |
| `Flashcards.Application.Tests` | Unit tests for the application layer |
| `Flashcards.Domain.Tests` | Unit tests for the domain layer |
| `Flashcards.Infrastructure.Tests` | Unit tests for the infrastructure layer |

## Tech Stack

- **.NET 10**
- **ASP.NET Core** (REST API)
- **AWS Lambda** (`Amazon.Lambda.AspNetCoreServer.Hosting`)
- **AWS API Gateway** (via SAM `serverless.template`)
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

### Run the API locally

```bash
cd Flashcards.Api
dotnet run
```

The API will be available at the URL printed in the console output.

### Run tests

```bash
dotnet test Flashcards.sln
```

## API Endpoints

| Method | Path | Description |
|---|---|---|
| `GET` | `/` | Health check |

> Deck and card endpoints are under development.

## Deployment

### Deploy the API (SAM / serverless)

```bash
cd Flashcards.Api
dotnet lambda deploy-serverless
```

### Deploy an individual Lambda function

```bash
cd Flashcards.Functions
dotnet lambda deploy-function
```

Both projects include `aws-lambda-tools-defaults.json` with default deployment settings. Ensure your AWS profile and region are configured before deploying.

## Project Status

This project is in early development. The clean architecture scaffolding (Domain, Application, Infrastructure) is in place, and the Lambda function stubs for deck and card operations have been created. Core business logic and persistence are yet to be implemented.
