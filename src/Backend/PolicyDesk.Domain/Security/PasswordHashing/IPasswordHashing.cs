namespace PolicyDesk.Domain.Security.PasswordHashing;

public interface IPasswordHashing
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string passwordHash);
}
