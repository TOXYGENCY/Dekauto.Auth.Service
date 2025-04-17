using System.Security.Claims;

namespace Dekauto.Auth.Service.Domain.Interfaces
{
    public interface IJwtTokenService
    {
        string GenerateToken(string login, Guid userId, string role);
        ClaimsPrincipal? ValidateToken(string token);
    }
}
