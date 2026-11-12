using Microsoft.EntityFrameworkCore;
using PolicyDesk.Domain.Entities;

namespace PolicyDesk.Api.IntegrationTests.Users;

public abstract class UsersTestBase(PolicyDeskApplicationFactory factory) : BaseIntegrationTest(factory)
{
    protected const string RegisterUri = "/api/autenticacao/registrar";
    protected const string LoginUri = "/api/autenticacao/entrar";

    protected Task<User> FindById(Guid id) =>
        DbContext.Users.IgnoreQueryFilters().AsNoTracking().SingleAsync(user => user.Id == id);

    protected Task<int> CountByEmail(string email) =>
        DbContext.Users.IgnoreQueryFilters().CountAsync(user => user.Email == email);
}
