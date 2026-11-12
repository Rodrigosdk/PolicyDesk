using CommonTestUtilities.Time;
using Moq;
using PolicyDesk.Domain.Security.Tokens;

namespace CommonTestUtilities.Security;

public static class AccessTokenGeneratorBuilder
{
    public static readonly DateTime ExpiresAt = BusinessClockBuilder.DefaultUtcNow.UtcDateTime.AddHours(1);

    public static string TokenFor(Guid userId) => $"token-{userId}";

    public static Mock<IAccessTokenGenerator> Build()
    {
        var mock = new Mock<IAccessTokenGenerator>();
        mock.Setup(generator => generator.Generate(It.IsAny<Guid>()))
            .Returns((Guid userId) => new AccessToken(TokenFor(userId), ExpiresAt));
        return mock;
    }
}
