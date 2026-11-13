using Moq;
using PolicyDesk.Domain.Entities;
using PolicyDesk.Domain.Repositories.Claims;

namespace CommonTestUtilities.Repositories.Claims;

public class ClaimsUpdateOnlyRepositoryBuilder
{
    private readonly Mock<IClaimsUpdateOnlyRepository> _mock = new();

    public void GetByIdForUpdate(Claim claim)
    {
        _mock.Setup(repository => repository.GetByIdForUpdateAsync(claim.Id, It.IsAny<CancellationToken>())).ReturnsAsync(claim);
    }

    public IClaimsUpdateOnlyRepository Build() => _mock.Object;
}
