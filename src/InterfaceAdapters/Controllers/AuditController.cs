using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SafeVault.Application.DTOs.Audit;
using SafeVault.Application.IServices;

namespace SafeVault.InterfaceAdapters.Controllers;

[ApiController]
[Route("api/audit")]
[Authorize(Roles = "Admin")]
public class AuditController(IAuditService auditService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<AuditLogDto>>> Search([FromQuery] DateTime? fromUtc, [FromQuery] DateTime? toUtc, [FromQuery] Guid? userId, CancellationToken cancellationToken)
    {
        var result = await auditService.SearchAsync(fromUtc, toUtc, userId, cancellationToken);
        return Ok(result);
    }
}
