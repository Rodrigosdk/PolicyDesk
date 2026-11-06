using Microsoft.AspNetCore.Mvc;
using PolicyDesk.Application.Policies.Cancel;
using PolicyDesk.Application.Policies.Register;
using PolicyDesk.Application.Policies.Listing;
using PolicyDesk.Communication.Requests;
using PolicyDesk.Communication.Responses;
using PolicyDesk.Domain.Enums;

namespace PolicyDesk.Api.Controllers;

[ApiController]
[Route("api/apolices")]
public class PoliciesController : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(ResponseCreatedJson), StatusCodes.Status201Created)]
    public async Task<IActionResult> Register(
        [FromBody] RequestPolicyJson request,
        [FromServices] IRegisterPolicyUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(request, cancellationToken);

        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ResponsePolicyJson>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ResponsePolicyJson>>> GetAll(
        [FromQuery] Guid? seguradoId,
        [FromQuery] PolicyStatus? status,
        [FromServices] IListPoliciesUseCase useCase,
        CancellationToken cancellationToken)
    {
        return Ok(await useCase.ListAsync(seguradoId, status, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ResponsePolicyJson), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResponsePolicyJson>> GetById(
        [FromRoute] Guid id,
        [FromServices] IListPoliciesUseCase useCase,
        CancellationToken cancellationToken)
    {
        return Ok(await useCase.GetByIdAsync(id, cancellationToken));
    }

    [HttpPost("{id:guid}/cancelamento")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Cancel(
        [FromRoute] Guid id,
        [FromServices] ICancelPolicyUseCase useCase,
        CancellationToken cancellationToken)
    {
        await useCase.ExecuteAsync(id, cancellationToken);

        return NoContent();
    }
}
