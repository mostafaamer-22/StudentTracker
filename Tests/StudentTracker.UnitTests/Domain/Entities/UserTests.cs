using FluentAssertions;
using StudentTracker.Domain.Entities;
using Xunit;

namespace StudentTracker.UnitTests.Domain.Entities;

public class UserTests
{

    [Fact]
    public void Create_WithValidData_ShouldCreateUser()
    {
        // Arrange
        var firstName = "John";
        var lastName = "Doe";
        var email = "john.doe@example.com";

        // Act
        var user = User.create(firstName, lastName, email);

        // Assert
        user.Should().NotBeNull();
        user.FullName.Should().Be("John Doe");
        user.Email.Should().Be(email.ToLowerInvariant());
        user.NormalizedEmail.Should().Be(email.ToUpperInvariant());
        user.UserName.Should().Be("john.doe");
        user.NormalizedUserName.Should().Be("JOHN.DOE");
        user.IsActive.Should().BeTrue();
        user.Id.Should().NotBeEmpty();
    }



    [Fact]
    public void Create_WithEmptyEmail_ShouldCreateUserWithEmptyEmail()
    {
        // Arrange
        var firstName = "Test";
        var lastName = "User";
        var email = "";

        // Act
        var user = User.create(firstName, lastName, email);

        // Assert
        user.Should().NotBeNull();
        user.Email.Should().BeEmpty();
    }

    [Fact]
    public void Create_WithComplexEmail_ShouldExtractCorrectUserName()
    {
        // Arrange
        var firstName = "Alice";
        var lastName = "Johnson";
        var email = "alice.johnson+test@example.com";

        // Act
        var user = User.create(firstName, lastName, email);

        // Assert
        user.Should().NotBeNull();
        user.UserName.Should().Be("alice.johnson+test");
        user.NormalizedUserName.Should().Be("ALICE.JOHNSON+TEST");
    }

    [Fact]
    public void SetProfileImage_WithValidUrl_ShouldSetProfileImageUrl()
    {
        // Arrange
        var user = CreateTestUser();
        var imageUrl = "https://example.com/profile.jpg";

        // Act
        user.SetProfileImage(imageUrl);

        // Assert
        user.ProfileImageUrl.Should().Be(imageUrl);
        user.HasProfileImage.Should().BeTrue();
    }

    [Fact]
    public void SetProfileImage_WithUrlContainingSpaces_ShouldTrimUrl()
    {
        // Arrange
        var user = CreateTestUser();
        var imageUrl = "  https://example.com/profile.jpg  ";

        // Act
        user.SetProfileImage(imageUrl);

        // Assert
        user.ProfileImageUrl.Should().Be("https://example.com/profile.jpg");
        user.HasProfileImage.Should().BeTrue();
    }

    [Fact]
    public void SetProfileImage_WithNullUrl_ShouldSetProfileImageUrlToNull()
    {
        // Arrange
        var user = CreateTestUser();
        user.SetProfileImage("https://example.com/profile.jpg"); // Set initially

        // Act
        user.SetProfileImage(null);

        // Assert
        user.ProfileImageUrl.Should().BeNull();
        user.HasProfileImage.Should().BeFalse();
    }

    [Fact]
    public void SetProfileImage_WithEmptyUrl_ShouldSetProfileImageUrlToEmpty()
    {
        // Arrange
        var user = CreateTestUser();

        // Act
        user.SetProfileImage("");

        // Assert
        user.ProfileImageUrl.Should().Be("");
        user.HasProfileImage.Should().BeFalse();
    }

    [Fact]
    public void SetProfileImage_WithWhitespaceUrl_ShouldSetProfileImageUrlToEmpty()
    {
        // Arrange
        var user = CreateTestUser();

        // Act
        user.SetProfileImage("   ");

        // Assert
        user.ProfileImageUrl.Should().Be("");
        user.HasProfileImage.Should().BeFalse();
    }

    [Fact]
    public void HasProfileImage_WithValidUrl_ShouldReturnTrue()
    {
        // Arrange
        var user = CreateTestUser();
        user.SetProfileImage("https://example.com/profile.jpg");

        // Act
        var hasProfileImage = user.HasProfileImage;

        // Assert
        hasProfileImage.Should().BeTrue();
    }

    [Fact]
    public void HasProfileImage_WithNullUrl_ShouldReturnFalse()
    {
        // Arrange
        var user = CreateTestUser();
        user.SetProfileImage(null);

        // Act
        var hasProfileImage = user.HasProfileImage;

        // Assert
        hasProfileImage.Should().BeFalse();
    }

    [Fact]
    public void HasProfileImage_WithEmptyUrl_ShouldReturnFalse()
    {
        // Arrange
        var user = CreateTestUser();
        user.SetProfileImage("");

        // Act
        var hasProfileImage = user.HasProfileImage;

        // Assert
        hasProfileImage.Should().BeFalse();
    }

    [Fact]
    public void RecordLogin_ShouldSetLastLoginAt()
    {
        // Arrange
        var user = CreateTestUser();
        var beforeLogin = DateTime.UtcNow;

        // Act
        user.RecordLogin();

        // Assert
        var afterLogin = DateTime.UtcNow;
        user.LastLoginAt.Should().NotBeNull();
        user.LastLoginAt!.Value.Should().BeOnOrAfter(beforeLogin);
        user.LastLoginAt.Value.Should().BeOnOrBefore(afterLogin);
    }




    [Fact]
    public void TimeSinceLastLogin_WithNoLastLogin_ShouldReturnNull()
    {
        // Arrange
        var user = CreateTestUser();

        // Act
        var timeSinceLastLogin = user.TimeSinceLastLogin;

        // Assert
        timeSinceLastLogin.Should().BeNull();
    }

    [Fact]
    public void Activate_ShouldSetIsActiveToTrue()
    {
        // Arrange
        var user = CreateTestUser();
        user.Deactivate(); // First deactivate

        // Act
        user.Activate();

        // Assert
        user.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var user = CreateTestUser();

        // Act
        user.Deactivate();

        // Assert
        user.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Activate_WhenAlreadyActive_ShouldRemainActive()
    {
        // Arrange
        var user = CreateTestUser();
        user.IsActive.Should().BeTrue(); // Initially active

        // Act
        user.Activate();

        // Assert
        user.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Deactivate_WhenAlreadyInactive_ShouldRemainInactive()
    {
        // Arrange
        var user = CreateTestUser();
        user.Deactivate(); // First deactivate
        user.IsActive.Should().BeFalse();

        // Act
        user.Deactivate();

        // Assert
        user.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Create_ShouldSetDefaultValuesCorrectly()
    {
        // Arrange & Act
        var user = CreateTestUser();

        // Assert
        user.IsActive.Should().BeTrue();
        user.LastLoginAt.Should().BeNull();
        user.ProfileImageUrl.Should().BeNull();
        user.HasProfileImage.Should().BeFalse();
        user.TimeSinceLastLogin.Should().BeNull();
        user.CreatedOnUtc.Should().Be(default(DateTime)); // Should be set by EF Core
        user.ModifiedOnUtc.Should().BeNull();
        user.DeletedAtUtc.Should().BeNull();
    }

    [Theory]
    [InlineData("user@domain.com", "user")]
    [InlineData("test.user@example.org", "test.user")]
    [InlineData("admin+role@company.net", "admin+role")]
    [InlineData("simple@test.io", "simple")]
    public void Create_WithVariousEmailFormats_ShouldExtractCorrectUserName(string email, string expectedUserName)
    {
        // Arrange & Act
        var user = User.create("Test", "User", email);

        // Assert
        user.UserName.Should().Be(expectedUserName);
        user.NormalizedUserName.Should().Be(expectedUserName.ToUpperInvariant());
    }

    [Fact]
    public void Create_WithLongNames_ShouldHandleLongNames()
    {
        // Arrange
        var firstName = "Christopher Alexander";
        var lastName = "Montgomery-Williams";
        var email = "chris.montgomery@example.com";

        // Act
        var user = User.create(firstName, lastName, email);

        // Assert
        user.FullName.Should().Be("Christopher Alexander Montgomery-Williams");
        user.Email.Should().Be(email.ToLowerInvariant());
        user.NormalizedEmail.Should().Be(email.ToUpperInvariant());
    }

    [Fact]
    public void Create_WithSpecialCharactersInEmail_ShouldHandleCorrectly()
    {
        // Arrange
        var firstName = "Test";
        var lastName = "User";
        var email = "test.user+tag@sub.domain.com";

        // Act
        var user = User.create(firstName, lastName, email);

        // Assert
        user.Email.Should().Be(email.ToLowerInvariant());
        user.NormalizedEmail.Should().Be(email.ToUpperInvariant());
        user.UserName.Should().Be("test.user+tag");
        user.NormalizedUserName.Should().Be("TEST.USER+TAG");
    }

    #region Helper Methods

    private User CreateTestUser(string firstName = "Test", string lastName = "User", string email = "test@example.com")
    {
        return User.create(firstName, lastName, email);
    }

    #endregion
}