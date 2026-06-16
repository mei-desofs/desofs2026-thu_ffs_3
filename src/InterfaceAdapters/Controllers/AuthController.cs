using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SafeVault.Application.DTOs.Auth;
using SafeVault.Application.IServices;
using SafeVault.InterfaceAdapters;

namespace SafeVault.InterfaceAdapters.Controllers;

[ApiController]
[Route("api/auth")]
[EnableRateLimiting("AuthPolicy")]
public class AuthController(IAuthService authService, ICsrfTokenService csrfTokenService) : ControllerBase
{
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var result = await authService.RegisterAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var userAgent = Request.Headers.UserAgent.ToString();
        var result = await authService.LoginAsync(request, ip, userAgent, cancellationToken);
        return Ok(result);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Refresh([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var result = await authService.RefreshTokenAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("csrf")]
    [Authorize]
    public ActionResult<CsrfTokenResponse> GetCsrfToken()
    {
        var userId = User.GetRequiredUserId();
        var token = csrfTokenService.IssueToken(userId);
        return Ok(new CsrfTokenResponse(token, DateTime.UtcNow.AddMinutes(30)));
    }
}
