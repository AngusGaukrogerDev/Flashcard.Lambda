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

### Card

| Class | HTTP Method | Description |
|---|---|---|
| `AddCardToDeckFunction` | `POST /decks/{deckId}/cards` | Add a new card to a deck |
| `GetCardFunction` | `GET /cards/{cardId}` | Get a card by ID |
| `UpdateCardFunction` | `PUT /cards/{cardId}` | Update a card's front or back text |
| `DeleteCardFunction` | `DELETE /cards/{cardId}` | Delete a card |

## Authentication

All functions expect an **API Gateway HTTP API v2 JWT authorizer**. The `sub` claim from the JWT is used as the `userId`. Requests without a valid `sub` receive a `401 Unauthorised` response.

For mutation operations, the caller's `userId` must match the `userId` stored on the resource. A mismatch returns `403 Forbidden`.

`AddCardToDeckFunction` additionally verifies that the target deck exists and belongs to the authenticated user before creating the card.

## CQRS

All use cases follow a command/query pattern:

### Deck

| Type | Class | Description |
|---|---|---|
| Command | `CreateDeckCommand` / `CreateDeckCommandHandler` | Create a deck |
| Command | `UpdateDeckCommand` / `UpdateDeckCommandHandler` | Update a deck |
| Command | `DeleteDeckCommand` / `DeleteDeckCommandHandler` | Delete a deck |
| Query | `GetDecksQuery` / `GetDecksQueryHandler` | List decks by user |

### Card

| Type | Class | Description |
|---|---|---|
| Command | `AddCardToDeckCommand` / `AddCardToDeckCommandHandler` | Add a card to a deck |
| Command | `UpdateCardCommand` / `UpdateCardCommandHandler` | Update a card |
| Command | `DeleteCardCommand` / `DeleteCardCommandHandler` | Delete a card |
| Query | `GetCardByIdQuery` / `GetCardByIdQueryHandler` | Get a card by ID |

## Data Storage

### Decks

Stored in a DynamoDB table with the following structure:

| Attribute | Type | Notes |
|---|---|---|
| `Id` | String (UUID) | Partition key |
| `UserId` | String | GSI partition key (for listing by user) |
| `Name` | String | |
| `Description` | String | Optional |
| `CreatedAt` | String (ISO 8601) | |

The GSI name is supplied via the `DECK_USER_ID_INDEX_NAME` environment variable.

### Cards

Stored in a separate DynamoDB table with the following structure:

| Attribute | Type | Notes |
|---|---|---|
| `Id` | String (UUID) | Partition key |
| `DeckId` | String | The deck this card belongs to |
| `UserId` | String | The user this card belongs to |
| `FrontText` | String | |
| `BackText` | String | |
| `CreatedAt` | String (ISO 8601) | |
| `NextReviewDate` | String (ISO 8601) | Optional — set by review logic |

### Environment variables

| Variable | Description |
|---|---|
| `DECK_TABLE_NAME` | DynamoDB table name for decks |
| `DECK_USER_ID_INDEX_NAME` | Name of the GSI on `UserId` for the decks table |
| `CARD_TABLE_NAME` | DynamoDB table name for cards |

`AddCardToDeckFunction` requires all three variables. All other card functions require only `CARD_TABLE_NAME`. Deck functions require only `DECK_TABLE_NAME` and `DECK_USER_ID_INDEX_NAME`.

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

Replace the handler class name with the function you want to deploy. Available handlers:

| Handler class | Description |
|---|---|
| `CreateDeckFunction` | Create deck |
| `GetDecksFunction` | List decks |
| `UpdateDeckFunction` | Update deck |
| `DeleteDeckFunction` | Delete deck |
| `AddCardToDeckFunction` | Add card to deck |
| `GetCardFunction` | Get card by ID |
| `UpdateCardFunction` | Update card |
| `DeleteCardFunction` | Delete card |

`Flashcards.Functions/aws-lambda-tools-defaults.json` contains default deployment configuration (runtime `dotnet8`, 512 MB memory, 30 s timeout). Ensure your AWS profile and region are configured before deploying.
