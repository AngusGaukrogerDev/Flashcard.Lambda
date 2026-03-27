# Flashcards Backend

A .NET 8 serverless backend for a flashcard application, built on AWS Lambda and DynamoDB. The project follows clean architecture with CQRS for all use cases.

## Architecture

```
Flashcards.Functions       ← Lambda entry points (HTTP API v2)
Flashcards.Application     ← CQRS commands/queries and handlers
Flashcards.Domain          ← Core entities and domain exceptions
Flashcards.Infrastructure  ← DynamoDB repository implementations
```

### Project dependencies

```
Functions → Application + Infrastructure
Infrastructure → Application + Domain
Application → Domain
```

## Lambda Functions

### Deck

| Class | HTTP Method | Description |
|---|---|---|
| `CreateDeckFunction` | `POST /decks` | Create a new flashcard deck |
| `GetDecksFunction` | `GET /decks` | List decks for the authenticated user (paginated) |
| `UpdateDeckFunction` | `PUT /decks/{deckId}` | Update a deck's name or description |
| `DeleteDeckFunction` | `DELETE /decks/{deckId}` | Delete a deck |

### Card (stubs — not yet implemented)

| Class | Description |
|---|---|
| `AddCardToDeckFunction` | Add a new card to a deck |
| `UpdateCardFunction` | Update an existing card |
| `RemoveCardFromDeckFunction` | Remove a card from a deck |
| `DeleteCardFunction` | Permanently delete a card |
| `ReviewDeckFunction` | Start a review session for a deck |

## Authentication

All functions expect an **API Gateway HTTP API v2 JWT authorizer**. The `sub` claim from the JWT is used as the `userId`. Requests without a valid `sub` receive a `401 Unauthorised` response.

For update and delete operations, the caller's `userId` must match the `userId` stored on the deck. A mismatch returns `403 Forbidden`.

## CQRS

All use cases follow a command/query pattern:

| Type | Class | Description |
|---|---|---|
| Command | `CreateDeckCommand` / `CreateDeckCommandHandler` | Create a deck |
| Command | `UpdateDeckCommand` / `UpdateDeckCommandHandler` | Update a deck |
| Command | `DeleteDeckCommand` / `DeleteDeckCommandHandler` | Delete a deck |
| Query | `GetDecksQuery` / `GetDecksQueryHandler` | List decks by user |

## Data Storage

Decks are stored in a **DynamoDB** table with the following structure:

| Attribute | Type | Notes |
|---|---|---|
| `Id` | String (UUID) | Partition key |
| `UserId` | String | GSI partition key (for listing by user) |
| `Name` | String | |
| `Description` | String | Optional |
| `CreatedAt` | String (ISO 8601) | |

The GSI name is supplied via the `DECK_USER_ID_INDEX_NAME` environment variable.

### Environment variables

| Variable | Description |
|---|---|
| `DECK_TABLE_NAME` | DynamoDB table name |
| `DECK_USER_ID_INDEX_NAME` | Name of the GSI on `UserId` |

## Tech Stack

- **.NET 8**
- **AWS Lambda** (`Amazon.Lambda.Core`, `Amazon.Lambda.APIGatewayEvents`)
- **Amazon DynamoDB** (`AWSSDK.DynamoDBv2`)
- **xUnit** + **NSubstitute** + **Shouldly** (testing)

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
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

Deploy an individual Lambda function from the `Flashcards.Functions` directory:

```bash
cd Flashcards.Functions
dotnet lambda deploy-function --function-handler "Flashcards.Functions::Flashcards.Functions.CreateDeckFunction::FunctionHandler"
```

`Flashcards.Functions/aws-lambda-tools-defaults.json` contains default deployment configuration (runtime `dotnet8`, 512 MB memory, 30 s timeout). Ensure your AWS profile and region are configured before deploying.
