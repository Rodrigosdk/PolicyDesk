using PolicyDesk.Communication.Requests;
using PolicyDesk.Communication.Responses;

namespace PolicyDesk.Application.Users.Login;

public interface ILoginUseCase
{
    public Task<ResponseLoginJson> ExecuteAsync(RequestLoginJson request, CancellationToken cancellationToken = default);
}
