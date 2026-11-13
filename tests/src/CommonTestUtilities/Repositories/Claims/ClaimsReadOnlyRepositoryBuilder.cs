using Moq;
using PolicyDesk.Domain.Entities;
using PolicyDesk.Domain.Repositories.Claims;

namespace CommonTestUtilities.Repositories.Claims;

public class ClaimsReadOnlyRepositoryBuilder
{
    private readonly Mock<IClaimsReadOnlyRepository> _mock = new();

    public void GetById(Claim claim)
    {
        _mock.Setup(repository => repository.GetByIdAsync(claim.Id, It.IsAny<CancellationToken>())).ReturnsAsync(claim);
    }

    public void GetByPolicyId(IEnumerable<Claim> claims)
    {
        _mock.Setup(repository => repository.GetByPolicyIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(claims);
    }

    public void SumCommittedAmount(decimal amount)
    {
        _mock
            .Setup(repository => repository.SumCommittedAmountAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(amount);
    }

    public IClaimsReadOnlyRepository Build() => _mock.Object;
}
