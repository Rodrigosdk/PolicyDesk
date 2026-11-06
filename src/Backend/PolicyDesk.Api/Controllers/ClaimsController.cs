using Microsoft.AspNetCore.Mvc;
using PolicyDesk.Application.Claims.Approve;
using PolicyDesk.Application.Claims.Analysis;
using PolicyDesk.Application.Claims.Deny;
using PolicyDesk.Application.Claims.Listing;
using PolicyDesk.Application.Claims.Payment;
using PolicyDesk.Application.Claims.Register;
using PolicyDesk.Communication.Requests;
using PolicyDesk.Communication.Responses;

namespace PolicyDesk.Api.Controllers;

[ApiController]
[Route("api")]
public class ClaimsController : ControllerBase
{
    [HttpPost("apolices/{id:guid}/sinistros")]
    [ProducesResponseType(typeof(ResponseCreatedJson), StatusCodes.Status201Created)]
    public async Task<IActionResult> Register(
        [FromRoute] Guid id,
        [FromBody] RequestClaimJson request,
        [FromServices] IRegisterClaimUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(id, request, cancellationToken);

        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpGet("apolices/{id:guid}/sinistros")]
    [ProducesResponseType(typeof(IEnumerable<ResponseClaimJson>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ResponseClaimJson>>> GetByPolicy(
        [FromRoute] Guid id,
        [FromServices] IListClaimsUseCase useCase,
        CancellationToken cancellationToken)
    {
        return Ok(await useCase.ListByPolicyAsync(id, cancellationToken));
    }

    [HttpGet("sinistros/{id:guid}")]
    [ProducesResponseType(typeof(ResponseClaimJson), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResponseClaimJson>> GetById(
        [FromRoute] Guid id,
        [FromServices] IListClaimsUseCase useCase,
        CancellationToken cancellationToken)
    {
        return Ok(await useCase.GetByIdAsync(id, cancellationToken));
    }

    [HttpPost("sinistros/{id:guid}/analise")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> StartAnalysis(
        [FromRoute] Guid id,
        [FromServices] IAnalyzeClaimUseCase useCase,
        CancellationToken cancellationToken)
    {
        await useCase.ExecuteAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPost("sinistros/{id:guid}/aprovacao")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Approve(
        [FromRoute] Guid id,
        [FromServices] IApproveClaimUseCase useCase,
        CancellationToken cancellationToken)
    {
        await useCase.ExecuteAsync(id, cancellationToken);

        return NoContent();
    }

    [HttpPost("sinistros/{id:guid}/negacao")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Deny(
        [FromRoute] Guid id,
        [FromBody] RequestDenyClaimJson request,
        [FromServices] IDenyClaimUseCase useCase,
        CancellationToken cancellationToken)
    {
        await useCase.ExecuteAsync(id, request, cancellationToken);
        return NoContent();
    }

    [HttpPost("sinistros/{id:guid}/pagamento")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RegisterPayment(
        [FromRoute] Guid id,
        [FromServices] IPayClaimUseCase useCase,
        CancellationToken cancellationToken)
    {
        await useCase.ExecuteAsync(id, cancellationToken);
        return NoContent();
    }
}
