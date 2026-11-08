using System.Security.Claims;
using Domain.Entities;

namespace Application.Abstractions;

public interface ITokenService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    ClaimsPrincipal GetPrincipalFromExpiredToken(string token); // update tokens
}