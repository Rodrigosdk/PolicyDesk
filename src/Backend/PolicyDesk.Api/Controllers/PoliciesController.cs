using Microsoft.AspNetCore.Mvc;
using PolicyDesk.Application.Policies.Cancel;
using PolicyDesk.Application.Policies.Listing;
using PolicyDesk.Application.Policies.Register;
using PolicyDesk.Communication.Enums;
using PolicyDesk.Communication.Requests;
using PolicyDesk.Communication.Responses;

namespace PolicyDesk.Api.Controllers;

[ApiController]
[Route("api/apolices")]
public class PoliciesController : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<ResponseCreatedJson>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, ProblemJson.ContentType)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, ProblemJson.ContentType)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, ProblemJson.ContentType)]
    public async Task<IActionResult> Register(
        [FromBody] RequestPolicyJson request,
        [FromServices] IRegisterPolicyUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(request, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet]
    [ProducesResponseType<ResponsePagedJson<ResponsePolicyJson>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, ProblemJson.ContentType)]
    public async Task<ActionResult<ResponsePagedJson<ResponsePolicyJson>>> GetAll(
        [FromQuery] Guid? seguradoId,
        [FromQuery] StatusApolice? status,
        [FromQuery] RequestPaginationJson pagination,
        [FromServices] IListPoliciesUseCase useCase,
        CancellationToken cancellationToken)
    {
        return Ok(await useCase.ListAsync(seguradoId, status, pagination, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<ResponsePolicyJson>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, ProblemJson.ContentType)]
    public async Task<ActionResult<ResponsePolicyJson>> GetById(
        [FromRoute] Guid id,
        [FromServices] IListPoliciesUseCase useCase,
        CancellationToken cancellationToken)
    {
        return Ok(await useCase.GetByIdAsync(id, cancellationToken));
    }

    [HttpPost("{id:guid}/cancelamento")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, ProblemJson.ContentType)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, ProblemJson.ContentType)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, ProblemJson.ContentType)]
    public async Task<IActionResult> Cancel(
        [FromRoute] Guid id,
        [FromServices] ICancelPolicyUseCase useCase,
        CancellationToken cancellationToken)
    {
        await useCase.ExecuteAsync(id, cancellationToken);

        return NoContent();
    }
}
