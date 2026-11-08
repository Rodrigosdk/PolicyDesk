using Microsoft.AspNetCore.Mvc;
using PolicyDesk.Application.Policyholders.Deactivate;
using PolicyDesk.Application.Policyholders.Listing;
using PolicyDesk.Application.Policyholders.Register;
using PolicyDesk.Application.Policyholders.Update;
using PolicyDesk.Communication.Requests;
using PolicyDesk.Communication.Responses;

namespace PolicyDesk.Api.Controllers;

[ApiController]
[Route("api/segurados")]
public class PolicyholdersController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<ResponsePagedJson<ResponsePolicyholderJson>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, ProblemJson.ContentType)]
    public async Task<ActionResult<ResponsePagedJson<ResponsePolicyholderJson>>> GetAll(
        [FromQuery] RequestPaginationJson pagination,
        [FromServices] IListPolicyholdersUseCase useCase,
        CancellationToken cancellationToken)
    {
        return Ok(await useCase.ListAsync(pagination, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<ResponsePolicyholderJson>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, ProblemJson.ContentType)]
    public async Task<ActionResult<ResponsePolicyholderJson>> GetById(
        [FromRoute] Guid id,
        [FromServices] IListPolicyholdersUseCase useCase,
        CancellationToken cancellationToken)
    {
        return Ok(await useCase.GetByIdAsync(id, cancellationToken));
    }

    [HttpPost]
    [ProducesResponseType<ResponseCreatedJson>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, ProblemJson.ContentType)]
    public async Task<IActionResult> Register(
        [FromBody] RequestPolicyholderJson request,
        [FromServices] IRegisterPolicyholderUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(request, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, ProblemJson.ContentType)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, ProblemJson.ContentType)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, ProblemJson.ContentType)]
    public async Task<IActionResult> Update(
        [FromRoute] Guid id,
        [FromBody] RequestUpdatePolicyholderJson request,
        [FromServices] IUpdatePolicyholderUseCase useCase,
        CancellationToken cancellationToken)
    {
        await useCase.ExecuteAsync(id, request, cancellationToken);

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, ProblemJson.ContentType)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, ProblemJson.ContentType)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, ProblemJson.ContentType)]
    public async Task<IActionResult> Remove(
        [FromRoute] Guid id,
        [FromServices] IDeactivatePolicyholderUseCase useCase,
        CancellationToken cancellationToken)
    {
        await useCase.ExecuteAsync(id, cancellationToken);

        return NoContent();
    }
}
