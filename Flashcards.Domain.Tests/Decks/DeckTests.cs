using Flashcards.Domain.Decks;
using Shouldly;

namespace Flashcards.Domain.Tests.Decks;

public class DeckTests
{
    private const string ValidName = "Spanish Verbs";
    private const string ValidUserId = "user-123";
    private const string ValidDescription = "Common Spanish verbs";

    public class CreateTests
    {
        [Fact]
        public void Create_WithValidInputs_SetsName()
        {
            var deck = Deck.Create(ValidName, ValidUserId);

            deck.Name.ShouldBe(ValidName);
        }

        [Fact]
        public void Create_WithValidInputs_SetsUserId()
        {
            var deck = Deck.Create(ValidName, ValidUserId);

            deck.UserId.ShouldBe(ValidUserId);
        }

        [Fact]
        public void Create_WithValidInputs_SetsNonEmptyId()
        {
            var deck = Deck.Create(ValidName, ValidUserId);

            deck.Id.Value.ShouldNotBe(Guid.Empty);
        }

        [Fact]
        public void Create_WithValidInputs_SetsCreatedAtToUtcNow()
        {
            var before = DateTime.UtcNow;

            var deck = Deck.Create(ValidName, ValidUserId);

            deck.CreatedAt.ShouldBeGreaterThanOrEqualTo(before);
            deck.CreatedAt.ShouldBeLessThanOrEqualTo(DateTime.UtcNow);
        }

        [Fact]
        public void Create_WithDescription_SetsDescription()
        {
            var deck = Deck.Create(ValidName, ValidUserId, ValidDescription);

            deck.Description.ShouldBe(ValidDescription);
        }

        [Fact]
        public void Create_WithoutDescription_LeavesDescriptionNull()
        {
            var deck = Deck.Create(ValidName, ValidUserId);

            deck.Description.ShouldBeNull();
        }

        [Fact]
        public void Create_EachCall_ProducesUniqueId()
        {
            var first = Deck.Create(ValidName, ValidUserId);
            var second = Deck.Create(ValidName, ValidUserId);

            first.Id.ShouldNotBe(second.Id);
        }

        [Fact]
        public void Create_WithNameSurroundedByWhitespace_TrimsName()
        {
            var deck = Deck.Create("  Spanish Verbs  ", ValidUserId);

            deck.Name.ShouldBe("Spanish Verbs");
        }

        [Fact]
        public void Create_WithDescriptionSurroundedByWhitespace_TrimsDescription()
        {
            var deck = Deck.Create(ValidName, ValidUserId, "  some description  ");

            deck.Description.ShouldBe("some description");
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Create_WithBlankName_ThrowsArgumentException(string blankName)
        {
            Should.Throw<ArgumentException>(() => Deck.Create(blankName, ValidUserId));
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Create_WithBlankUserId_ThrowsArgumentException(string blankUserId)
        {
            Should.Throw<ArgumentException>(() => Deck.Create(ValidName, blankUserId));
        }
    }

    public class UpdateTests
    {
        [Fact]
        public void Update_WithValidInputs_UpdatesName()
        {
            var deck = Deck.Create(ValidName, ValidUserId);

            deck.Update("French Nouns", null);

            deck.Name.ShouldBe("French Nouns");
        }

        [Fact]
        public void Update_WithDescription_UpdatesDescription()
        {
            var deck = Deck.Create(ValidName, ValidUserId, "old description");

            deck.Update(ValidName, "new description");

            deck.Description.ShouldBe("new description");
        }

        [Fact]
        public void Update_WithNullDescription_ClearsDescription()
        {
            var deck = Deck.Create(ValidName, ValidUserId, ValidDescription);

            deck.Update(ValidName, null);

            deck.Description.ShouldBeNull();
        }

        [Fact]
        public void Update_WithNameSurroundedByWhitespace_TrimsName()
        {
            var deck = Deck.Create(ValidName, ValidUserId);

            deck.Update("  French Nouns  ", null);

            deck.Name.ShouldBe("French Nouns");
        }

        [Fact]
        public void Update_WithDescriptionSurroundedByWhitespace_TrimsDescription()
        {
            var deck = Deck.Create(ValidName, ValidUserId);

            deck.Update(ValidName, "  some description  ");

            deck.Description.ShouldBe("some description");
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Update_WithBlankName_ThrowsArgumentException(string blankName)
        {
            var deck = Deck.Create(ValidName, ValidUserId);

            Should.Throw<ArgumentException>(() => deck.Update(blankName, null));
        }

        [Fact]
        public void Update_DoesNotChangeId()
        {
            var deck = Deck.Create(ValidName, ValidUserId);
            var originalId = deck.Id;

            deck.Update("French Nouns", null);

            deck.Id.ShouldBe(originalId);
        }

        [Fact]
        public void Update_DoesNotChangeCreatedAt()
        {
            var deck = Deck.Create(ValidName, ValidUserId);
            var originalCreatedAt = deck.CreatedAt;

            deck.Update("French Nouns", null);

            deck.CreatedAt.ShouldBe(originalCreatedAt);
        }

        [Fact]
        public void Update_DoesNotChangeUserId()
        {
            var deck = Deck.Create(ValidName, ValidUserId);

            deck.Update("French Nouns", null);

            deck.UserId.ShouldBe(ValidUserId);
        }
    }

    public class ReconstituteTests
    {
        [Fact]
        public void Reconstitute_PreservesAllProperties()
        {
            var id = DeckId.New();
            var createdAt = new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc);

            var deck = Deck.Reconstitute(id, ValidName, ValidDescription, createdAt, ValidUserId);

            deck.Id.ShouldBe(id);
            deck.Name.ShouldBe(ValidName);
            deck.Description.ShouldBe(ValidDescription);
            deck.CreatedAt.ShouldBe(createdAt);
            deck.UserId.ShouldBe(ValidUserId);
        }

        [Fact]
        public void Reconstitute_WithNullDescription_PreservesNullDescription()
        {
            var deck = Deck.Reconstitute(DeckId.New(), ValidName, null, DateTime.UtcNow, ValidUserId);

            deck.Description.ShouldBeNull();
        }
    }
}
