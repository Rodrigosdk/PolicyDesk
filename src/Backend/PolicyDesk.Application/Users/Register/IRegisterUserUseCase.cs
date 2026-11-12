using PolicyDesk.Communication.Requests;
using PolicyDesk.Communication.Responses;

namespace PolicyDesk.Application.Users.Register;

public interface IRegisterUserUseCase
{
    public Task<ResponseCreatedJson> ExecuteAsync(RequestUserJson request, CancellationToken cancellationToken = default);
}
