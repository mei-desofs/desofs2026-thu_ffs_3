using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SafeVault.Application.DTOs.Vaults;
using SafeVault.Application.IServices;

namespace SafeVault.InterfaceAdapters.Controllers;

[ApiController]
[Route("api/vaults")]
[Authorize(Roles = "Admin,Manager")]
public class VaultsController(IVaultService vaultService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<VaultDto>>> GetAccessible(CancellationToken cancellationToken)
    {
        var userId = User.GetRequiredUserId();
        var result = await vaultService.GetAccessibleAsync(userId, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<VaultDto>> Create([FromBody] CreateVaultRequest request, CancellationToken cancellationToken)
    {
        var userId = User.GetRequiredUserId();
        var result = await vaultService.CreateAsync(userId, request, cancellationToken);
        return CreatedAtAction(nameof(GetAccessible), new { id = result.Id }, result);
    }

    [HttpPut("{vaultId:guid}")]
    public async Task<ActionResult<VaultDto>> Update(Guid vaultId, [FromBody] UpdateVaultRequest request, CancellationToken cancellationToken)
    {
        var userId = User.GetRequiredUserId();
        var result = await vaultService.UpdateAsync(vaultId, userId, request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{vaultId:guid}/archive")]
    public async Task<IActionResult> Archive(Guid vaultId, CancellationToken cancellationToken)
    {
        var userId = User.GetRequiredUserId();
        await vaultService.ArchiveAsync(vaultId, userId, cancellationToken);
        return NoContent();
    }

    [HttpPost("{vaultId:guid}/access")]
    public async Task<IActionResult> GrantAccess(Guid vaultId, [FromBody] GrantVaultAccessRequest request, CancellationToken cancellationToken)
    {
        var userId = User.GetRequiredUserId();
        await vaultService.GrantAccessAsync(vaultId, userId, request, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{vaultId:guid}/access/{userId:guid}")]
    public async Task<IActionResult> RevokeAccess(Guid vaultId, Guid userId, CancellationToken cancellationToken)
    {
        var actorId = User.GetRequiredUserId();
        await vaultService.RevokeAccessAsync(vaultId, actorId, userId, cancellationToken);
        return NoContent();
    }
}
