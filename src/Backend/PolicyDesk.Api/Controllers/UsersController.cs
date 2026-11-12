using Microsoft.AspNetCore.Mvc;
using PolicyDesk.Application.Users.Register;
using PolicyDesk.Communication.Requests;
using PolicyDesk.Communication.Responses;

namespace PolicyDesk.Api.Controllers;

[ApiController]
[Route("api/usuarios")]
public class UsersController : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<ResponseCreatedJson>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, ProblemJson.ContentType)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, ProblemJson.ContentType)]
    public async Task<IActionResult> Register(
        [FromBody] RequestUserJson request,
        [FromServices] IRegisterUserUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(request, cancellationToken);

        return StatusCode(StatusCodes.Status201Created, result);
    }
}
