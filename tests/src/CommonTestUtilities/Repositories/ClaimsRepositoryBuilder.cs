using Moq;
using PolicyDesk.Domain.Entities;
using PolicyDesk.Domain.Repositories.Claims;

namespace CommonTestUtilities.Repositories;

public class ClaimsReadOnlyRepositoryBuilder
{
    private readonly Mock<IClaimsReadOnlyRepository> _repository = new();

    public Mock<IClaimsReadOnlyRepository> Mock => _repository;

    public ClaimsReadOnlyRepositoryBuilder GetById(Claim claim)
    {
        _repository.Setup(repository => repository.GetById(claim.Id)).ReturnsAsync(claim);
        return this;
    }

    public ClaimsReadOnlyRepositoryBuilder GetByPolicyId(IEnumerable<Claim> claims)
    {
        _repository.Setup(repository => repository.GetByPolicyId(It.IsAny<Guid>())).ReturnsAsync(claims);
        return this;
    }

    public IClaimsReadOnlyRepository Build() => _repository.Object;
}

public class ClaimsUpdateOnlyRepositoryBuilder
{
    private readonly Mock<IClaimsUpdateOnlyRepository> _repository = new();

    public Mock<IClaimsUpdateOnlyRepository> Mock => _repository;

    public ClaimsUpdateOnlyRepositoryBuilder GetByIdForUpdate(Claim claim)
    {
        _repository.Setup(repository => repository.GetByIdForUpdate(claim.Id)).ReturnsAsync(claim);
        return this;
    }

    public IClaimsUpdateOnlyRepository Build() => _repository.Object;
}

public static class ClaimsWriteOnlyRepositoryBuilder
{
    public static Mock<IClaimsWriteOnlyRepository> Build()
    {
        var mock = new Mock<IClaimsWriteOnlyRepository>();
        mock.Setup(repository => repository.Add(It.IsAny<Claim>())).Returns(Task.CompletedTask);
        return mock;
    }
}
