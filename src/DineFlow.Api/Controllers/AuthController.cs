using DineFlow.BusinessObjects.Auth;
using DineFlow.Api.Security;
using DineFlow.Services.Auth;
using Microsoft.AspNetCore.Mvc;

namespace DineFlow.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IApiTokenService _tokenService;

    public AuthController(IAuthService authService, IApiTokenService tokenService)
    {
        _authService = authService;
        _tokenService = tokenService;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequestDto request)
    {
        try
        {
            var user = _authService.Login(request);
            user.AccessToken = _tokenService.CreateToken(user);
            return Ok(user);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
