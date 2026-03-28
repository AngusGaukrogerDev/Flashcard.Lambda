#!/usr/bin/env bash
# Deploy the review and study-queue Lambdas using the configured AWS CLI profile/region.
# Prerequisites: dotnet tool install -g Amazon.Lambda.Tools
set -euo pipefail
cd "$(dirname "$0")/../Flashcards.Functions"

: "${AWS_PROFILE:=default}"
: "${AWS_REGION:=eu-west-2}"

export AWS_PROFILE AWS_REGION

dotnet lambda deploy-function \
  --function-handler "Flashcards.Functions::Flashcards.Functions.ReviewCardFunction::FunctionHandler"

dotnet lambda deploy-function \
  --function-handler "Flashcards.Functions::Flashcards.Functions.GetCardsForStudyFunction::FunctionHandler"

echo "Done. Add API Gateway routes: POST /cards/{cardId}/review and GET /decks/{deckId}/cards/study"
