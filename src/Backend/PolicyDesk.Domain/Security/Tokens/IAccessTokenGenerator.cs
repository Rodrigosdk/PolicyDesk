namespace PolicyDesk.Domain.Security.Tokens;

public interface IAccessTokenGenerator
{
    AccessToken Generate(Guid userId);
}
