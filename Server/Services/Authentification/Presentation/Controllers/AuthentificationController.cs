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
    public async Task<IActionResult> InitiateRegisterAsync([FromBody] RegisterUserRequest request, CancellationToken cancellationToken = default)
    {
  
        var response = await _authentificationService.InitiateRegisterUserAsync(request, cancellationToken);
        return response.IsSuccess ? Ok(response) : BadRequest(response);
    }

    [HttpPost("confirm-register")]
    public async Task<IActionResult> ConfirmRegisterAsync([FromBody] ConfirmEmailRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await _authentificationService.ConfirmEmailAsync(request, cancellationToken);
        return response.IsSuccess ? Ok(response) : BadRequest(response);
    }

    [HttpPost("login")]
    public async Task<IActionResult> LoginAsync([FromBody] LoginUserRequest request,  CancellationToken cancellationToken = default)
    {
 
        var response = await _authentificationService.LoginUserAsync(request, cancellationToken);
        return response.IsSuccess ? Ok(response) : BadRequest(response);
    }

    [HttpPut("reset-password")]
    public async Task<IActionResult> InitiatePasswordResetAsync([FromBody] ForgotPasswordRequest request,
        CancellationToken cancellationToken = default)
    {

        var response = await _authentificationService.InitiatePasswordResetAsync(request, cancellationToken);
        return response.IsSuccess ? Ok(response) : BadRequest(response);
    }

    [HttpPut("confirm-reset-password")]
    public async Task<IActionResult> ConfirmPasswordResetAsync([FromBody] ResetPasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await _authentificationService.ConfirmPasswordResetAsync(request, cancellationToken);
        return response.IsSuccess ? Ok(response) : BadRequest(response);
    }
}