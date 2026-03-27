using Flashcards.Domain.Cards;
using Shouldly;

namespace Flashcards.Domain.Tests.Cards;

public class CardTests
{
    private const string ValidFrontText = "Hola";
    private const string ValidBackText = "Hello";
    private const string ValidDeckId = "deck-123";
    private const string ValidUserId = "user-123";

    public class CreateTests
    {
        [Fact]
        public void Create_WithValidInputs_SetsFrontText()
        {
            var card = Card.Create(ValidFrontText, ValidBackText, ValidDeckId, ValidUserId);

            card.FrontText.ShouldBe(ValidFrontText);
        }

        [Fact]
        public void Create_WithValidInputs_SetsBackText()
        {
            var card = Card.Create(ValidFrontText, ValidBackText, ValidDeckId, ValidUserId);

            card.BackText.ShouldBe(ValidBackText);
        }

        [Fact]
        public void Create_WithValidInputs_SetsDeckId()
        {
            var card = Card.Create(ValidFrontText, ValidBackText, ValidDeckId, ValidUserId);

            card.DeckId.ShouldBe(ValidDeckId);
        }

        [Fact]
        public void Create_WithValidInputs_SetsUserId()
        {
            var card = Card.Create(ValidFrontText, ValidBackText, ValidDeckId, ValidUserId);

            card.UserId.Value.ShouldBe(ValidUserId);
        }

        [Fact]
        public void Create_WithValidInputs_SetsNonEmptyId()
        {
            var card = Card.Create(ValidFrontText, ValidBackText, ValidDeckId, ValidUserId);

            card.Id.Value.ShouldNotBe(Guid.Empty);
        }

        [Fact]
        public void Create_WithValidInputs_SetsCreatedAtToUtcNow()
        {
            var before = DateTime.UtcNow;

            var card = Card.Create(ValidFrontText, ValidBackText, ValidDeckId, ValidUserId);

            card.CreatedAt.ShouldBeGreaterThanOrEqualTo(before);
            card.CreatedAt.ShouldBeLessThanOrEqualTo(DateTime.UtcNow);
        }

        [Fact]
        public void Create_WithValidInputs_LeavesNextReviewDateNull()
        {
            var card = Card.Create(ValidFrontText, ValidBackText, ValidDeckId, ValidUserId);

            card.NextReviewDate.ShouldBeNull();
        }

        [Fact]
        public void Create_WithNoOptionalFields_LeavesOptionalFieldsNull()
        {
            var card = Card.Create(ValidFrontText, ValidBackText, ValidDeckId, ValidUserId);

            card.FrontPrompt.ShouldBeNull();
            card.BackPrompt.ShouldBeNull();
            card.BackgroundColour.ShouldBeNull();
            card.TextColour.ShouldBeNull();
        }

        [Fact]
        public void Create_WithOptionalFields_SetsOptionalFields()
        {
            var card = Card.Create(ValidFrontText, ValidBackText, ValidDeckId, ValidUserId,
                frontPrompt: "A common greeting",
                backPrompt: "English translation",
                backgroundColour: CardColour.Green,
                textColour: TextColour.Black);

            card.FrontPrompt.ShouldBe("A common greeting");
            card.BackPrompt.ShouldBe("English translation");
            card.BackgroundColour.ShouldBe(CardColour.Green);
            card.TextColour.ShouldBe(TextColour.Black);
        }

        [Fact]
        public void Create_EachCall_ProducesUniqueId()
        {
            var first = Card.Create(ValidFrontText, ValidBackText, ValidDeckId, ValidUserId);
            var second = Card.Create(ValidFrontText, ValidBackText, ValidDeckId, ValidUserId);

            first.Id.ShouldNotBe(second.Id);
        }

        [Fact]
        public void Create_WithFrontTextSurroundedByWhitespace_TrimsFrontText()
        {
            var card = Card.Create("  Hola  ", ValidBackText, ValidDeckId, ValidUserId);

            card.FrontText.ShouldBe("Hola");
        }

        [Fact]
        public void Create_WithBackTextSurroundedByWhitespace_TrimsBackText()
        {
            var card = Card.Create(ValidFrontText, "  Hello  ", ValidDeckId, ValidUserId);

            card.BackText.ShouldBe("Hello");
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Create_WithBlankFrontText_ThrowsArgumentException(string blank)
        {
            Should.Throw<ArgumentException>(() => Card.Create(blank, ValidBackText, ValidDeckId, ValidUserId));
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Create_WithBlankBackText_ThrowsArgumentException(string blank)
        {
            Should.Throw<ArgumentException>(() => Card.Create(ValidFrontText, blank, ValidDeckId, ValidUserId));
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Create_WithBlankDeckId_ThrowsArgumentException(string blank)
        {
            Should.Throw<ArgumentException>(() => Card.Create(ValidFrontText, ValidBackText, blank, ValidUserId));
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Create_WithBlankUserId_ThrowsArgumentException(string blank)
        {
            Should.Throw<ArgumentException>(() => Card.Create(ValidFrontText, ValidBackText, ValidDeckId, blank));
        }
    }

    public class UpdateTests
    {
        [Fact]
        public void Update_WithValidInputs_UpdatesFrontText()
        {
            var card = Card.Create(ValidFrontText, ValidBackText, ValidDeckId, ValidUserId);

            card.Update("Adios", "Goodbye");

            card.FrontText.ShouldBe("Adios");
        }

        [Fact]
        public void Update_WithValidInputs_UpdatesBackText()
        {
            var card = Card.Create(ValidFrontText, ValidBackText, ValidDeckId, ValidUserId);

            card.Update("Adios", "Goodbye");

            card.BackText.ShouldBe("Goodbye");
        }

        [Fact]
        public void Update_WithTextSurroundedByWhitespace_TrimsText()
        {
            var card = Card.Create(ValidFrontText, ValidBackText, ValidDeckId, ValidUserId);

            card.Update("  Adios  ", "  Goodbye  ");

            card.FrontText.ShouldBe("Adios");
            card.BackText.ShouldBe("Goodbye");
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Update_WithBlankFrontText_ThrowsArgumentException(string blank)
        {
            var card = Card.Create(ValidFrontText, ValidBackText, ValidDeckId, ValidUserId);

            Should.Throw<ArgumentException>(() => card.Update(blank, ValidBackText));
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Update_WithBlankBackText_ThrowsArgumentException(string blank)
        {
            var card = Card.Create(ValidFrontText, ValidBackText, ValidDeckId, ValidUserId);

            Should.Throw<ArgumentException>(() => card.Update(ValidFrontText, blank));
        }

        [Fact]
        public void Update_DoesNotChangeId()
        {
            var card = Card.Create(ValidFrontText, ValidBackText, ValidDeckId, ValidUserId);
            var originalId = card.Id;

            card.Update("Adios", "Goodbye");

            card.Id.ShouldBe(originalId);
        }

        [Fact]
        public void Update_DoesNotChangeDeckId()
        {
            var card = Card.Create(ValidFrontText, ValidBackText, ValidDeckId, ValidUserId);

            card.Update("Adios", "Goodbye");

            card.DeckId.ShouldBe(ValidDeckId);
        }

        [Fact]
        public void Update_DoesNotChangeCreatedAt()
        {
            var card = Card.Create(ValidFrontText, ValidBackText, ValidDeckId, ValidUserId);
            var originalCreatedAt = card.CreatedAt;

            card.Update("Adios", "Goodbye");

            card.CreatedAt.ShouldBe(originalCreatedAt);
        }

        [Fact]
        public void Update_WithOptionalFields_UpdatesOptionalFields()
        {
            var card = Card.Create(ValidFrontText, ValidBackText, ValidDeckId, ValidUserId);

            card.Update("Adios", "Goodbye",
                frontPrompt: "A farewell",
                backPrompt: "English translation",
                backgroundColour: CardColour.Yellow,
                textColour: TextColour.Black);

            card.FrontPrompt.ShouldBe("A farewell");
            card.BackPrompt.ShouldBe("English translation");
            card.BackgroundColour.ShouldBe(CardColour.Yellow);
            card.TextColour.ShouldBe(TextColour.Black);
        }

        [Fact]
        public void Update_WithNullOptionalFields_ClearsOptionalFields()
        {
            var card = Card.Create(ValidFrontText, ValidBackText, ValidDeckId, ValidUserId,
                frontPrompt: "A greeting",
                backgroundColour: CardColour.Red);

            card.Update("Adios", "Goodbye");

            card.FrontPrompt.ShouldBeNull();
            card.BackgroundColour.ShouldBeNull();
        }
    }

    public class ReconstituteTests
    {
        [Fact]
        public void Reconstitute_PreservesAllProperties()
        {
            var id = CardId.New();
            var createdAt = new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc);
            var nextReviewDate = new DateTime(2024, 1, 22, 10, 30, 0, DateTimeKind.Utc);

            var card = Card.Reconstitute(
                id,
                ValidFrontText,
                ValidBackText,
                ValidDeckId,
                ValidUserId,
                createdAt,
                nextReviewDate,
                frontPrompt: "Think about a greeting",
                backPrompt: "The English equivalent",
                backgroundColour: CardColour.Blue,
                textColour: TextColour.White);

            card.Id.ShouldBe(id);
            card.FrontText.ShouldBe(ValidFrontText);
            card.BackText.ShouldBe(ValidBackText);
            card.DeckId.ShouldBe(ValidDeckId);
            card.UserId.Value.ShouldBe(ValidUserId);
            card.CreatedAt.ShouldBe(createdAt);
            card.NextReviewDate.ShouldBe(nextReviewDate);
            card.FrontPrompt.ShouldBe("Think about a greeting");
            card.BackPrompt.ShouldBe("The English equivalent");
            card.BackgroundColour.ShouldBe(CardColour.Blue);
            card.TextColour.ShouldBe(TextColour.White);
        }

        [Fact]
        public void Reconstitute_WithNullNextReviewDate_PreservesNullNextReviewDate()
        {
            var card = Card.Reconstitute(CardId.New(), ValidFrontText, ValidBackText, ValidDeckId, ValidUserId, DateTime.UtcNow, null);

            card.NextReviewDate.ShouldBeNull();
        }

        [Fact]
        public void Reconstitute_WithNullOptionalFields_PreservesNullOptionalFields()
        {
            var card = Card.Reconstitute(CardId.New(), ValidFrontText, ValidBackText, ValidDeckId, ValidUserId, DateTime.UtcNow, null);

            card.FrontPrompt.ShouldBeNull();
            card.BackPrompt.ShouldBeNull();
            card.BackgroundColour.ShouldBeNull();
            card.TextColour.ShouldBeNull();
        }
    }
}
