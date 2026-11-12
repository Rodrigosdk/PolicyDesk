namespace PolicyDesk.Domain.Security.Tokens;

public sealed record AccessToken(string Token, DateTime ExpiresAt);
