using Application.DTOs.Requests;
using Domain.Entities;

namespace Application.Mappers;

public static class AuthentificationMapper
{
    public static User FromAuthUserRequestToUserRegisterEntity(RegisterUserRequest userRequest)
    {
        
        return User.Register(
            email: userRequest.Email,
            passwordHash: userRequest.Password,
            role: userRequest.Role
        );
    }
}