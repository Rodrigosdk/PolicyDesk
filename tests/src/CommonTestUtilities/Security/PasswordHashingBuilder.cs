using PolicyDesk.Domain.Security.PasswordHashing;
using PolicyDesk.Infrastructure.Security.PasswordHashing;

namespace CommonTestUtilities.Security;

public static class PasswordHashingBuilder
{
    public static IPasswordHashing Build() => new Argon2PasswordHasher();
}
