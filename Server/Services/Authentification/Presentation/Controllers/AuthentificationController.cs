using Application.Abstractions;
using Application.DTOs.Requests;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers;

[ApiController]
[Route("/api/authentification")]
public class AuthentificationController : ControllerBase
{
    private readonly IAuthentificationService _authentificationService;

    public AuthentificationController(IAuthentificationService authentificationService)
    {
        _authentificationService = authentificationService ?? throw new ArgumentNullException(nameof(authentificationService));
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserRequest request, CancellationToken cancellationToken = default)
    {
  
        var response = await _authentificationService.RegisterUser(request, cancellationToken);
        return response.IsSuccess ? Ok(response) : BadRequest(response);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginUserRequest request,  CancellationToken cancellationToken = default)
    {
 
        var response = await _authentificationService.LoginUser(request, cancellationToken);
        return response.IsSuccess ? Ok(response) : BadRequest(response);
    }

    [HttpPut("reset-password")]
    public async Task<IActionResult> InitiatePasswordReset([FromBody] ForgotPasswordRequest request,
        CancellationToken cancellationToken = default)
    {

        var response = await _authentificationService.InitiatePasswordReset(request, cancellationToken);
        return response.IsSuccess ? Ok(response) : BadRequest(response);
    }

    [HttpPut("complete-reset-password")]
    public async Task<IActionResult> CompletePasswordReset([FromBody] ResetPasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await _authentificationService.CompletePasswordReset(request, cancellationToken);
        return response.IsSuccess ? Ok(response) : BadRequest(response);
    }
}