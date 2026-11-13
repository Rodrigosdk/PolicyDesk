using Moq;
using PolicyDesk.Domain.Security.PasswordHashing;

namespace CommonTestUtilities.Security;

public class PasswordHashingBuilder
{
    private readonly Mock<IPasswordHashing> _mock = new();

    public PasswordHashingBuilder()
    {
        _mock.Setup(passwordHashing => passwordHashing.HashPassword(It.IsAny<string>())).Returns("hashed-password");
    }

    public void VerifyPassword(string password)
    {
        _mock.Setup(passwordHashing => passwordHashing.VerifyPassword(password, It.IsAny<string>())).Returns(true);
    }

    public IPasswordHashing Build() => _mock.Object;
}
