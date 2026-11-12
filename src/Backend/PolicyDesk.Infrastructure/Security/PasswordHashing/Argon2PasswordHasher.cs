using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;
using PolicyDesk.Domain.Security.PasswordHashing;

namespace PolicyDesk.Infrastructure.Security.PasswordHashing;

internal sealed class Argon2PasswordHasher : IPasswordHashing
{
    private const int DegreeOfParallelism = 1;
    private const int Iteration = 1;
    private const int MemorySize = 20 * 1024; // 20MB
    private const int SaltSize = 20;
    private const int HashSize = 32;
    
    public string HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);

        var hash = HashPassword(password, salt);
        var combinedBytes = new byte[hash.Length + salt.Length];
        
        salt.CopyTo(combinedBytes);
        hash.CopyTo(combinedBytes, salt.Length); 
        
        return Convert.ToBase64String(combinedBytes);
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        var combinedBytes = Convert.FromBase64String(passwordHash);

        var salt = new byte[SaltSize];
        var hash = new byte[HashSize];
        
        Array.Copy(combinedBytes, salt, SaltSize);
        Array.Copy(combinedBytes, SaltSize, hash, 0,HashSize);
        
        var newHash = HashPassword(password, salt);

        return CryptographicOperations.FixedTimeEquals(hash, newHash);
    }

    private static byte[] HashPassword(string password, byte[] salt)
    {
        var passwordBytes = Encoding.UTF8.GetBytes(password);

        var hashAlgorithm = new Argon2id(passwordBytes)
        {
            DegreeOfParallelism = DegreeOfParallelism,
            Iterations = Iteration,
            MemorySize = MemorySize,
            Salt = salt
        };
        return hashAlgorithm.GetBytes(HashSize);
    }
}
