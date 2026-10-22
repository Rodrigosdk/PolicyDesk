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
    [ProducesResponseType(typeof(IEnumerable<ResponsePolicyholdersListingJson>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ResponsePolicyholdersListingJson>>> GetAll(
        [FromServices] IListPolicyholdersUseCase useCase)
    {
        var result = await useCase.Listing();

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ResponsePolicyholdersListingJson), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResponsePolicyholdersListingJson>> GetById(
        [FromRoute] Guid id,
        [FromServices] IListPolicyholdersUseCase useCase)
    {
        var result = await useCase.GetById(id);

        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> Register(
        [FromBody] RequestPolicyholdersJson request,
        [FromServices] IRegisterPolicyholderUseCase useCase)
    {
        await useCase.Execute(request);

        return StatusCode(StatusCodes.Status201Created);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Update(
        [FromRoute] Guid id,
        [FromBody] RequestPolicyholdersUpdateJson request,
        [FromServices] IUpdatePolicyholderCommand useCase)
    {
        await useCase.Execute(id, request);

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Remove(
        [FromRoute] Guid id,
        [FromServices] IDeactivatePolicyholderUseCase useCase)
    {
        await useCase.Execute(id);

        return NoContent();
    }
}
