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
    [ProducesResponseType(typeof(ResponsePagedJson<ResponsePolicyholdersListingJson>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResponsePagedJson<ResponsePolicyholdersListingJson>>> GetAll(
        [FromQuery] RequestPaginationJson pagination,
        [FromServices] IListPolicyholdersUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.ListAsync(pagination, cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ResponsePolicyholdersListingJson), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResponsePolicyholdersListingJson>> GetById(
        [FromRoute] Guid id,
        [FromServices] IListPolicyholdersUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.GetByIdAsync(id, cancellationToken);

        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> Register(
        [FromBody] RequestPolicyholdersJson request,
        [FromServices] IRegisterPolicyholderUseCase useCase,
        CancellationToken cancellationToken)
    {
        await useCase.ExecuteAsync(request, cancellationToken);

        return StatusCode(StatusCodes.Status201Created);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Update(
        [FromRoute] Guid id,
        [FromBody] RequestPolicyholdersUpdateJson request,
        [FromServices] IUpdatePolicyholderUseCase useCase,
        CancellationToken cancellationToken)
    {
        await useCase.ExecuteAsync(id, request, cancellationToken);

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Remove(
        [FromRoute] Guid id,
        [FromServices] IDeactivatePolicyholderUseCase useCase,
        CancellationToken cancellationToken)
    {
        await useCase.ExecuteAsync(id, cancellationToken);

        return NoContent();
    }
}
