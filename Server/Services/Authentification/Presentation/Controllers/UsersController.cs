using System.Security.Claims;
using Application.Abstractions;
using Application.DTOs.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IUsersService _userService;
    
    public UsersController(IUsersService userService)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
    }

    [Authorize(Policy = "AdministratorOnly")]
    [HttpPatch("block-status")]
    public async Task<IActionResult> UpdateBlockStatusAsync([FromBody] UpdateBlockStatusRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _userService.UpdateBLockStatusAsync(request, cancellationToken);
        return response.IsSuccess ? Ok(response) : BadRequest(response);
    }

    [Authorize]
    [HttpPut]
    public async Task<IActionResult> UpdateProfileAsync([FromBody] UpdateUserRequest request,
        CancellationToken cancellationToken = default)
    { 
        var userId = Guid.Parse(User.FindFirstValue("sub")!);
        var response = await _userService.UpdateProfileAsync(userId, request, cancellationToken);
        return response.IsSuccess ? Ok(response) : BadRequest(response);
    }

    [Authorize(Policy = "AdministratorOnly")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync([FromRoute] Guid id, CancellationToken cancellationToken = default)
    {
        var response = await _userService.SoftDeleteAsync(id, cancellationToken);
        return response.IsSuccess ? Ok(response) :  BadRequest(response);
    }
    
}