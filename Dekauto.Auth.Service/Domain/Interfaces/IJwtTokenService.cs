using Dekauto.Auth.Service.Domain.Entities;
using Dekauto.Auth.Service.Domain.Entities.Adapters;
using Dekauto.Auth.Service.Domain.Entities.DTO;
using System.Security.Claims;

namespace Dekauto.Auth.Service.Domain.Interfaces
{
    public interface IJwtTokenService
    {
        AuthTokensAdapter GenerateToken(UserDto account);
        ClaimsPrincipal? ValidateToken(string token);
    }
}
